using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using UnityEngine;

using static Gizmo.PluginConfig;

namespace Gizmo {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ComfyGizmo : BaseUnityPlugin {
    public const string PluginGUID = "com.rolopogo.gizmo.comfy";
    public const string PluginName = "ComfyGizmo";
    public const string PluginVersion = "1.5.0";

    static GameObject _gizmoPrefab = null;
    static Transform _gizmoRoot;

    static Transform _xGizmo;
    static Transform _yGizmo;
    static Transform _zGizmo;

    static Transform _xGizmoRoot;
    static Transform _yGizmoRoot;
    static Transform _zGizmoRoot;

    static GameObject _comfyGizmo;
    static Transform _comfyGizmoRoot;

    static int _xRot;
    static int _yRot;
    static int _zRot;

    static bool _localFrame;

    static float _snapAngle;

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      SnapDivisions.SettingChanged += (sender, eventArgs) => _snapAngle = 180f / SnapDivisions.Value;
      _snapAngle = 180f / SnapDivisions.Value;

      _gizmoPrefab = LoadGizmoPrefab();
     
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Game))]
    static class GamePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Game.Start))]
      static void StartPostfix() {
        Destroy(_gizmoRoot);
        _gizmoRoot = CreateGizmoRoot();

        Destroy(_comfyGizmo);
        _comfyGizmo = new("ComfyGizmo");
        _comfyGizmoRoot = _comfyGizmo.transform;
        _localFrame = UseLocalFrame.Value;
      }
    }

    [HarmonyPatch(typeof(Player))]
    static class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
      static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_placeRotation))),
                new CodeMatch(OpCodes.Conv_R4),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_S))
            .Advance(offset: 5)
            .InsertAndAdvance(Transpilers.EmitDelegate<Func<Quaternion, Quaternion>>(_ => _comfyGizmoRoot.rotation))
            .InstructionEnumeration();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.UpdatePlacement))]
      static void UpdatePlacementPostfix(ref Player __instance, ref bool takeInput) {
        if (__instance.m_placementMarkerInstance) {
          _gizmoRoot.gameObject.SetActive(ShowGizmoPrefab.Value && __instance.m_placementMarkerInstance.activeSelf);
          _gizmoRoot.position = __instance.m_placementMarkerInstance.transform.position + (Vector3.up * 0.5f);
        }

        if (!__instance.m_buildPieces || !takeInput) {
          return;
        }

        if (Input.GetKey(CopyPieceRotationKey.Value.MainKey) && __instance.m_hoveringPiece != null) {
          MatchPieceRotation(__instance.m_hoveringPiece);
        }

        if (Input.GetKey(ChangeRotationModeKey.Value.MainKey)) {
          UseLocalFrame.Value = !UseLocalFrame.Value;
        }

        // Reset rotations on changing from default rotations to local frame rotations and vice versa
        if (_localFrame != UseLocalFrame.Value) {
          if(_localFrame) {
            ResetRotationsLocalFrame();
          } else {
            ResetRotations();
          }
          
          _localFrame = UseLocalFrame.Value;
          return;
        }

        _xGizmo.localScale = Vector3.one;
        _yGizmo.localScale = Vector3.one;
        _zGizmo.localScale = Vector3.one;

        if (!UseLocalFrame.Value) {
          Rotate();
        } else {
          int direction = Math.Sign(Input.GetAxis("Mouse ScrollWheel"));
          RotateLocalFrame(direction);
        }
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
    static void UpdateCrosshairPostfix(ref Hud __instance, Player player) {
      
    }

    static void ResetRotations() {
      _xRot = 0;
      _yRot = 0;
      _zRot = 0;

      _comfyGizmo.transform.localRotation =
        Quaternion.Euler(_xRot * _snapAngle, _yRot * _snapAngle, _zRot * _snapAngle);

      _xGizmoRoot.localRotation = Quaternion.Euler(_xRot * _snapAngle, 0f, 0f);
      _yGizmoRoot.localRotation = Quaternion.Euler(0f, _yRot * _snapAngle, 0f);
      _zGizmoRoot.localRotation = Quaternion.Euler(0f, 0f, _zRot * _snapAngle);
    }

    static void ResetRotationsLocalFrame() {
      _comfyGizmo.transform.rotation = Quaternion.AngleAxis(0f, Vector3.up);
      _comfyGizmo.transform.rotation = Quaternion.AngleAxis(0f, Vector3.right);
      _comfyGizmo.transform.rotation = Quaternion.AngleAxis(0f, Vector3.forward);

      _gizmoRoot.rotation = Quaternion.AngleAxis(0f, Vector3.up);
      _gizmoRoot.rotation = Quaternion.AngleAxis(0f, Vector3.right);
      _gizmoRoot.rotation = Quaternion.AngleAxis(0f, Vector3.forward);
    }

    static void Rotate() {
      if (Input.GetKey(ResetAllRotationKey.Value.MainKey)) {
        ResetRotations();
      } else if (Input.GetKey(XRotationKey.Value.MainKey)) {
        _xGizmo.localScale = Vector3.one * 1.5f;
        HandleAxisInput(ref _xRot, _xGizmo);
      } else if (Input.GetKey(ZRotationKey.Value.MainKey)) {
        _zGizmo.localScale = Vector3.one * 1.5f;
        HandleAxisInput(ref _zRot, _zGizmo);
      } else {
        _yGizmo.localScale = Vector3.one * 1.5f;
        HandleAxisInput(ref _yRot, _yGizmo);
      }

      _comfyGizmo.transform.localRotation =
          Quaternion.Euler(_xRot * _snapAngle, _yRot * _snapAngle, _zRot * _snapAngle);

      _xGizmoRoot.localRotation = Quaternion.Euler(_xRot * _snapAngle, 0f, 0f);
      _yGizmoRoot.localRotation = Quaternion.Euler(0f, _yRot * _snapAngle, 0f);
      _zGizmoRoot.localRotation = Quaternion.Euler(0f, 0f, _zRot * _snapAngle);
    }

    static void RotateLocalFrame(int direction) {
      if (Input.GetKey(ResetAllRotationKey.Value.MainKey)) {
        ResetRotationsLocalFrame();
        return;
      }

      float rotation = direction * _snapAngle;
      Vector3 rotVector;

      if (Input.GetKey(XRotationKey.Value.MainKey)) {
        _xGizmo.localScale = Vector3.one * 1.5f;
        rotVector = Vector3.right;
        HandleAxisInputLocalFrame(rotVector, _xGizmo);
      } else if (Input.GetKey(ZRotationKey.Value.MainKey)) {
        _zGizmo.localScale = Vector3.one * 1.5f;
        rotVector = Vector3.forward;
        HandleAxisInputLocalFrame(rotVector, _zGizmo);
      } else {
        _yGizmo.localScale = Vector3.one * 1.5f;
        rotVector = Vector3.up;
        HandleAxisInputLocalFrame(rotVector, _yGizmo);
      }

      RotateAxes(rotation, rotVector);
    }

    static void RotateAxes(float rotation, Vector3 rotVector) {
      _comfyGizmo.transform.rotation *= Quaternion.AngleAxis(rotation, rotVector);
      _gizmoRoot.rotation *= Quaternion.AngleAxis(rotation, rotVector);
    }

    static void HandleAxisInput(ref int rotation, Transform gizmo) {
      gizmo.localScale = Vector3.one * 1.5f;
      rotation = (rotation + Math.Sign(Input.GetAxis("Mouse ScrollWheel"))) % (SnapDivisions.Value * 2);

      if (Input.GetKey(ResetRotationKey.Value.MainKey)) {
        rotation = 0;
      }
    }

    static void HandleAxisInputLocalFrame(Vector3 rotVector, Transform gizmo) {
      gizmo.localScale = Vector3.one * 1.5f;

      if (Input.GetKey(ResetRotationKey.Value.MainKey)) {
        _comfyGizmo.transform.rotation = Quaternion.AngleAxis(0f, rotVector);
        _xGizmoRoot.rotation = Quaternion.AngleAxis(0f, rotVector);
        _yGizmoRoot.rotation = Quaternion.AngleAxis(0f, rotVector);
        _zGizmoRoot.rotation = Quaternion.AngleAxis(0f, rotVector);
      }
    }
    static void MatchPieceRotation(Piece target) {
      if (_localFrame) {
        Quaternion targetQuaternion = GetQuaternion(target);
        _comfyGizmo.transform.rotation = targetQuaternion;
        _gizmoRoot.rotation = targetQuaternion;
      } else {
        Vector3 eulerAngles = GetEulerAngles(target);
        _xRot = (int)Math.Round(eulerAngles.x/_snapAngle);
        _yRot = (int)Math.Round(eulerAngles.y / _snapAngle);
        _zRot = (int)Math.Round(eulerAngles.z / _snapAngle);
        Rotate();
      }
    }

    static Quaternion GetQuaternion(Piece target) {
      return target.GetComponent<Transform>().localRotation;
    }

    static Vector3 GetEulerAngles(Piece target) {
      return target.GetComponent<Transform>().eulerAngles;
    }

    static GameObject LoadGizmoPrefab() {
      AssetBundle bundle = AssetBundle.LoadFromMemory(
          GetResource(Assembly.GetExecutingAssembly(), "Gizmo.Resources.gizmos"));

      GameObject prefab = bundle.LoadAsset<GameObject>("GizmoRoot");
      bundle.Unload(unloadAllLoadedObjects: false);

      return prefab;
    }

    static byte[] GetResource(Assembly assembly, string resourceName) {
      Stream stream = assembly.GetManifestResourceStream(resourceName);

      byte[] data = new byte[stream.Length];
      stream.Read(data, offset: 0, count: (int) stream.Length);

      return data;
    }

    static Transform CreateGizmoRoot() {
      _gizmoRoot = Instantiate(_gizmoPrefab).transform;

      // ??? Something about quaternions.
      _xGizmo = _gizmoRoot.Find("YRoot/ZRoot/XRoot/X");
      _yGizmo = _gizmoRoot.Find("YRoot/Y");
      _zGizmo = _gizmoRoot.Find("YRoot/ZRoot/Z");

      _xGizmoRoot = _gizmoRoot.Find("YRoot/ZRoot/XRoot");
      _yGizmoRoot = _gizmoRoot.Find("YRoot");
      _zGizmoRoot = _gizmoRoot.Find("YRoot/ZRoot");

      return _gizmoRoot.transform;
    }
  }
}
