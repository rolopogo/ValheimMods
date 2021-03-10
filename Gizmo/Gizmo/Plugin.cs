using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RoloPogo.Utilities;
using RoloPogo.Utils;
using UnityEngine;

namespace Gizmo
{
    [BepInPlugin("com.rolopogo.Gizmo","Gizmo", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;

        int xRot;
        int yRot;
        int zRot;

        Transform gizmoRoot;

        Transform xGizmo;
        Transform yGizmo;
        Transform zGizmo;

        Transform xGizmoRoot;
        Transform yGizmoRoot;
        Transform zGizmoRoot;

        private ConfigEntry<int> snapDivisions;
        private ConfigEntry<string> xKey;
        private ConfigEntry<string> zKey;
        private ConfigEntry<string> resetKey;

        float snapAngle => 180f / snapDivisions.Value;

        GameObject gizmoPrefab;

        private void Awake()
        {
            instance = this;
            snapDivisions = Config.Bind<int>("General", "SnapDivisions", 16, "Number of snap angles per 180 degrees. Vanilla uses 8");
            xKey = Config.Bind<string>("General", "xKey", "LeftShift", "Hold this key to rotate in the x plane (red circle)");
            zKey = Config.Bind<string>("General", "zKey", "LeftAlt", "Hold this key to rotate in the z plane (blue circle)");
            resetKey = Config.Bind<string>("General", "resetKey", "V", "Press this key to reset the selected axis to zero rotation");

            var bundle = AssetBundle.LoadFromMemory(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "Gizmo.Resources.gizmos"));
            gizmoPrefab = bundle.LoadAsset<GameObject>("GizmoRoot");
            bundle.Unload(false);

            Harmony.CreateAndPatchAll(typeof(UpdatePlacementGhost_Patch));
            Harmony.CreateAndPatchAll(typeof(UpdatePlacement_Patch));
        }

        public void UpdatePlacement(Player player, GameObject placementGhost, bool takeInput) {
            if (player != Player.m_localPlayer) return;

            if (!gizmoRoot)
            {
                gizmoRoot = Instantiate(gizmoPrefab).transform;
                xGizmo = gizmoRoot.Find("YRoot/ZRoot/XRoot/X");
                yGizmo = gizmoRoot.Find("YRoot/Y");
                zGizmo = gizmoRoot.Find("YRoot/ZRoot/Z");
                xGizmoRoot = gizmoRoot.Find("YRoot/ZRoot/XRoot");
                yGizmoRoot = gizmoRoot.Find("YRoot");
                zGizmoRoot = gizmoRoot.Find("YRoot/ZRoot");
            }
            var marker = player.GetPrivateField<GameObject>("m_placementMarkerInstance");
            if (marker)
            {   
                gizmoRoot.gameObject.SetActive(marker.activeSelf);
                gizmoRoot.position = marker.transform.position + Vector3.up * .5f;
            }

            if (!player.InPlaceMode())
                return;

            if (!takeInput)
                return;

            xGizmo.localScale = Vector3.one;
            yGizmo.localScale = Vector3.one;
            zGizmo.localScale = Vector3.one;

            var scrollWheelInput = Math.Sign(Input.GetAxis("Mouse ScrollWheel"));

            if (Enum.TryParse<KeyCode>(xKey.Value, out var xKeyCode) && Input.GetKey(xKeyCode))
            {
                HandleAxisInput(scrollWheelInput, ref xRot, xGizmo);
            }
            else if (Enum.TryParse<KeyCode>(zKey.Value, out var zKeyCode) && Input.GetKey(zKeyCode))
            {
                HandleAxisInput(scrollWheelInput, ref zRot, zGizmo);
            }
            else
            {
                HandleAxisInput(scrollWheelInput, ref yRot, yGizmo);
            }

            xGizmoRoot.localRotation = Quaternion.Euler(xRot * snapAngle, 0, 0);
            yGizmoRoot.localRotation = Quaternion.Euler(0, yRot * snapAngle, 0);
            zGizmoRoot.localRotation = Quaternion.Euler(0, 0, zRot * snapAngle);
        }

        private void HandleAxisInput(int scrollWheelInput, ref int rot, Transform gizmo)
        {
            gizmo.localScale = Vector3.one * 1.5f;
            rot = (rot + scrollWheelInput) % (snapDivisions.Value*2);
            if (Enum.TryParse<KeyCode>(resetKey.Value, out var resetKeyCode) && Input.GetKey(resetKeyCode))
                rot = 0;
        }

        private static Quaternion GetPlacementAngle(float x, float y, float z)
        {
            return instance.xGizmoRoot.rotation;
        }
    }
}
