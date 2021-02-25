using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ExploreAsOne.Utilities;
using UnityEngine.EventSystems;
using System.Collections;

namespace ExploreTogether.Patches
{
    class Chat_Patch
    {
        [HarmonyPatch(typeof(Chat), "Awake")]
        [HarmonyPostfix]
        private static void Chat_Awake(Chat __instance)
        {
            Plugin.AddString(__instance, "/shareMap - Share your current map exploration progress with other players");
            Plugin.AddString(__instance, "/sharePins - Share all of your map pins with other players");
        }
        
        [HarmonyPatch(typeof(Chat), "InputText")]
        [HarmonyPrefix]
        private static bool Chat_InputText(Chat __instance, InputField ___m_input)
        {
            if (Player.m_localPlayer)
            {
                string text = ___m_input.text;
                
                if (text.ToLower().StartsWith("/sharemap"))
                {
                    if (!Plugin.busy)
                        Plugin.ShareMap();
                    else
                        Plugin.AddString(__instance, "Can't share map just yet!");
                    return false;
                }

                if (text.ToLower().StartsWith("/sharepins"))
                {
                    var pins = Minimap.instance.GetPrivateField<List<Minimap.PinData>>("m_pins").ToArray();
                    foreach (var pin in pins)
                    {
                        var name = pin.m_name;
                        
                        Plugin.SendPin(pin, name);
                    }
                    return false;
                }
            }
            return true;
        }
        
        [HarmonyPatch(typeof(Chat), "SendPing")]
        [HarmonyPrefix]
        private static bool Chat_SendPingPatch(Vector3 position)
        {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer)
            {
                Vector3 vector = position;
                vector.y = localPlayer.transform.position.y;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                {
                vector,
                3,
                localPlayer.GetPlayerName(),
                $"Ping! ({Mathf.FloorToInt(vector.x)}, {Mathf.FloorToInt(vector.z)})"
                });
            }
            return false;
        }
    }
}
