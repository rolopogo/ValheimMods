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
    [BepInPlugin("com.rolopogo.Gizmo", "Gizmo", "1.0.0")]
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

        public ConfigEntry<int> snapDivisions;
        private ConfigEntry<string> xKey;
        private ConfigEntry<string> zKey;
        private ConfigEntry<string> resetKey;
        public ConfigEntry<string> cycleSnapKey;
        public int currentSnapDivisions;
        public float currentSnapAngle;

        float snapAngle => 180f / snapDivisions.Value;

        GameObject gizmoPrefab;

        private void Awake()
        {
            instance = this;
            snapDivisions = Config.Bind<int>("General", "SnapDivisions", 8, "Number of snap angles per 180 degrees. Vanilla uses 8");
            cycleSnapKey = Config.Bind<string>("General", "CycleSnapDivisions", "KeypadPlus", "Press this key to cycle between 8, 16 and 32 Snap Divisions");
            xKey = Config.Bind<string>("General", "xKey", "LeftShift", "Hold this key to rotate in the x plane (red circle)");
            zKey = Config.Bind<string>("General", "zKey", "LeftAlt", "Hold this key to rotate in the z plane (blue circle)");
            resetKey = Config.Bind<string>("General", "resetKey", "V", "Press this key to reset the selected axis to zero rotation");

            var bundle = AssetBundle.LoadFromMemory(ResourceUtils.GetResource(Assembly.GetExecutingAssembly(), "Gizmo.Resources.gizmos"));
            gizmoPrefab = bundle.LoadAsset<GameObject>("GizmoRoot");
            bundle.Unload(false);

            Harmony.CreateAndPatchAll(typeof(UpdatePlacementGhost_Patch));
            Harmony.CreateAndPatchAll(typeof(UpdatePlacement_Patch));

            currentSnapDivisions = snapDivisions.Value;
            currentSnapAngle = 180f / currentSnapDivisions;
        }

        public void UpdatePlacement(Player player, GameObject placementGhost, bool takeInput)
        {
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

            var buildMode = Player.m_localPlayer.GetRightItem().m_shared.m_name == "$item_hammer";

            xGizmo.localScale = Vector3.one;
            yGizmo.localScale = Vector3.one;
            zGizmo.localScale = Vector3.one;

            if (Enum.TryParse<KeyCode>(cycleSnapKey.Value, out var cycleSnapKeyCode) && Input.GetKeyUp(cycleSnapKeyCode) && buildMode)
            {
                switch (currentSnapDivisions)
                {
                    case 8:
                        currentSnapDivisions = 16;
                        currentSnapAngle = 180f / currentSnapDivisions;
                        break;
                    case 16:
                        currentSnapDivisions = 32;
                        currentSnapAngle = 180f / currentSnapDivisions;
                        break;
                    case 32:
                        currentSnapDivisions = 8;
                        currentSnapAngle = 180f / currentSnapDivisions;
                        break;
                }
                notifyUser("Changed number of snap divisions to " + currentSnapDivisions);
            }

            var scrollWheelInput = Math.Sign(Input.GetAxis("Mouse ScrollWheel"));

            if (Enum.TryParse<KeyCode>(xKey.Value, out var xKeyCode) && Input.GetKey(xKeyCode) && buildMode)
            {
                HandleAxisInput(scrollWheelInput, ref xRot, xGizmo);
            }
            else if (Enum.TryParse<KeyCode>(zKey.Value, out var zKeyCode) && Input.GetKey(zKeyCode) && buildMode)
            {
                HandleAxisInput(scrollWheelInput, ref zRot, zGizmo);
            }
            else if (Player.m_localPlayer.GetRightItem().m_shared.m_name == "$item_hammer")
            {
                HandleAxisInput(scrollWheelInput, ref yRot, yGizmo);
            }

            if (Enum.TryParse<KeyCode>(resetKey.Value, out var resetKeyCode) && Input.GetKeyUp(resetKeyCode) && buildMode)
            {
                xRot = 0;
                yRot = 0;
                zRot = 0;
            }

            xGizmoRoot.localRotation = Quaternion.Euler(xRot * currentSnapAngle, 0, 0);
            yGizmoRoot.localRotation = Quaternion.Euler(0, yRot * currentSnapAngle, 0);
            zGizmoRoot.localRotation = Quaternion.Euler(0, 0, zRot * currentSnapAngle);

        }

        private void HandleAxisInput(int scrollWheelInput, ref int rot, Transform gizmo)
        {
            gizmo.localScale = Vector3.one * 1.5f;
            rot = (rot + scrollWheelInput) % (currentSnapDivisions * 2);
        }

        private static Quaternion GetPlacementAngle(float x, float y, float z)
        {
            return instance.xGizmoRoot.rotation;
        }

        private static void notifyUser(string Message, MessageHud.MessageType position = MessageHud.MessageType.TopLeft)
        {
            MessageHud.instance.ShowMessage(position, "Gizmo: " + Message);
        }
    }
}
