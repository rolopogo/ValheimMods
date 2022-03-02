using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace Gizmo {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ComfyGizmo : BaseUnityPlugin {
    public const string PluginGUID = "com.rolopogo.gizmo.comfy";
    public const string PluginName = "ComfyGizmo";
    public const string PluginVersion = "1.2.0";

    static ConfigEntry<int> _snapDivisions;

    static ConfigEntry<KeyboardShortcut> _xRotationKey;
    static ConfigEntry<KeyboardShortcut> _zRotationKey;
    static ConfigEntry<KeyboardShortcut> _resetRotationKey;

    static ConfigEntry<bool> _showGizmoPrefab;

    static GameObject _gizmoPrefab = null;
    static Transform _gizmoRoot;

    static Transform _xGizmo;
    static Transform _yGizmo;
    static Transform _zGizmo;

    static Transform _xGizmoRoot;
    static Transform _yGizmoRoot;
    static Transform _zGizmoRoot;

    static int _xRot;
    static int _yRot;
    static int _zRot;

    static float _snapAngle;

    Harmony _harmony;

    public void Awake() {
      _snapDivisions =
          Config.Bind(
              "Gizmo",
              "snapDivisions",
              16,
              new ConfigDescription(
                  "Number of snap angles per 180 degrees. Vanilla uses 8.",
                  new AcceptableValueRange<int>(2, 128)));

      _snapDivisions.SettingChanged += (sender, eventArgs) => _snapAngle = 180f / _snapDivisions.Value;
      _snapAngle = 180f / _snapDivisions.Value;

      _xRotationKey =
          Config.Bind(
              "Keys",
              "xRotationKey",
              new KeyboardShortcut(KeyCode.LeftShift),
              "Hold this key to rotate on the x-axis/plane (red circle).");

      _zRotationKey =
          Config.Bind(
              "Keys",
              "zRotationKey",
              new KeyboardShortcut(KeyCode.LeftAlt),
              "Hold this key to rotate on the z-axis/plane (blue circle).");

      _resetRotationKey =
          Config.Bind(
              "Keys",
              "resetRotationKey",
              new KeyboardShortcut(KeyCode.V),
              "Press this key to reset the selected axis to zero rotation.");

      _showGizmoPrefab = Config.Bind("UI", "showGizmoPrefab", true, "Show the Gizmo prefab in placement mode.");

      _gizmoPrefab = LoadGizmoPrefab();
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Game))]
    class GamePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Game.Start))]
      static void StartPostfix() {
        Destroy(_gizmoRoot);
        _gizmoRoot = CreateGizmoRoot();
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
      static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.Method(
                        typeof(Quaternion),
                        nameof(Quaternion.Euler),
                        new Type[] { typeof(float), typeof(float), typeof(float) })))
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<float, float, float, Quaternion>>(
                    (x, y, z) => _xGizmoRoot.rotation).operand)
            .InstructionEnumeration();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.UpdatePlacement))]
      static void UpdatePlacementPostfix(ref Player __instance, ref bool takeInput) {
        if (__instance.m_placementMarkerInstance) {
          _gizmoRoot.gameObject.SetActive(_showGizmoPrefab.Value && __instance.m_placementMarkerInstance.activeSelf);
          _gizmoRoot.position = __instance.m_placementMarkerInstance.transform.position + (Vector3.up * 0.5f);
        }

        if (!__instance.m_buildPieces || !takeInput) {
          return;
        }

        _xGizmo.localScale = Vector3.one;
        _yGizmo.localScale = Vector3.one;
        _zGizmo.localScale = Vector3.one;

        if (Input.GetKey(_xRotationKey.Value.MainKey)) {
          HandleAxisInput(ref _xRot, _xGizmo);
        } else if (Input.GetKey(_zRotationKey.Value.MainKey)) {
          HandleAxisInput(ref _zRot, _zGizmo);
        } else {
          HandleAxisInput(ref _yRot, _yGizmo);
        }

        _xGizmoRoot.localRotation = Quaternion.Euler(_xRot * _snapAngle, 0f, 0f);
        _yGizmoRoot.localRotation = Quaternion.Euler(0f, _yRot * _snapAngle, 0f);
        _zGizmoRoot.localRotation = Quaternion.Euler(0f, 0f, _zRot * _snapAngle);
      }
    }

    private static void HandleAxisInput(ref int rotation, Transform gizmo) {
      gizmo.localScale = Vector3.one * 1.5f;
      rotation = (rotation + Math.Sign(Input.GetAxis("Mouse ScrollWheel"))) % (_snapDivisions.Value * 2);

      if (Input.GetKey(_resetRotationKey.Value.MainKey)) {
        rotation = 0;
      }
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
