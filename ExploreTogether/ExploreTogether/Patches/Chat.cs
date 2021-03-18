using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ExploreTogether.Patches
{
    class Chat_Patch
    {
        [HarmonyPatch(typeof(Chat), "Awake")]
        [HarmonyPostfix]
        private static void Chat_Awake(Chat __instance)
        {
            Plugin.AddString("/shareMap - Share your current map exploration progress with other players");
            Plugin.AddString("/sharePins - Share all of your map pins with other players");
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
                    Plugin.ShareMap();
                    return false;
                }

                if (text.ToLower().StartsWith("/sharepins"))
                {
                    Plugin.SharePins();
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
