using RoloPogo.Utilities;
using HarmonyLib;
using System;
using UnityEngine;

namespace ExploreTogether.Patches
{
    class Player_Patch
    {
        [HarmonyPatch(typeof(Player), "Update")]
        [HarmonyPostfix]
        private static void Player_Update(Player __instance)
        {
            if (Player.m_localPlayer == __instance)
            {
                if (Settings.PingWhereLooking.Value)
                {
                    if (Enum.TryParse(Settings.PingKey.Value, out KeyCode key))
                    {
                        if (__instance.InvokeMethod<bool>("TakeInput"))
                            if (Input.GetKeyDown(key))
                            {
                                // TODO: improve object detection range

                                var dir = __instance.GetAimDir(Vector3.zero);
                                var ray = new Ray(GameCamera.instance.transform.position, GameCamera.instance.transform.forward);

                                var mask = Pathfinding.instance.m_layers | Pathfinding.instance.m_waterLayers;

                                Physics.Raycast(ray, out var hit, 500f, mask);

                                string pingText = "Ping!";
                                Vector3 pos = hit.point;

                                if (__instance.GetHoverCreature() != null)
                                {
                                    pingText = __instance.GetHoverCreature().GetHoverName();
                                    pos = __instance.GetHoverCreature().GetCenterPoint();
                                }
                                else if (__instance.GetHoverObject())
                                {
                                    Hoverable hoverable = __instance.GetHoverObject().GetComponentInParent<Hoverable>();
                                    if (hoverable != null)
                                    {
                                        pingText = hoverable.GetHoverText().Split('\n')[0];
                                    }
                                    pos = __instance.GetHoverObject().transform.position;
                                }
                                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                                {
                                pos,
                                3,
                                __instance.GetPlayerName(),
                                $"{pingText} at ({Mathf.FloorToInt(pos.x)}, {Mathf.FloorToInt(pos.z)})"
                                });
                            }
                    }
                }

                if (Enum.TryParse(Settings.ShareMapKey.Value, out KeyCode mapKey))
                    if(Input.GetKeyDown(mapKey))
                        Plugin.ShareMap();


                if (Enum.TryParse(Settings.SharePinsKey.Value, out KeyCode pinKey))
                    if(Input.GetKeyDown(pinKey))
                        Plugin.SharePins();

            }
        }
    }
}
