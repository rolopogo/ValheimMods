using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ExploreTogether.Patches
{
    class Minimap_Patch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Minimap), "Explore", new Type[] { typeof(Vector3), typeof(float) })]
        public static void Explore(object instance, Vector3 p, float radius) => throw new NotImplementedException();

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Minimap), "Explore", new Type[] { typeof(int), typeof(int) })]
        public static bool Explore(object instance, int x, int y) => throw new NotImplementedException();

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Minimap), "ScreenToWorldPoint", new Type[] { typeof(Vector3) })]
        public static Vector3 ScreenToWorldPoint(object instance, Vector3 screenPos) => throw new NotImplementedException();

        [HarmonyPatch(typeof(Minimap), "UpdateExplore")]
        [HarmonyPrefix]
        private static bool Minimap_UpdateExplore(ref Minimap __instance, Player player, float ___m_exploreTimer, float ___m_exploreInterval, float ___m_exploreRadius, List<ZNet.PlayerInfo> ___m_tempPlayerInfo)
        {
            if (Settings.OthersRevealMap.Value)
            {
                if (___m_exploreTimer + Time.deltaTime > ___m_exploreInterval)
                {
                    ___m_tempPlayerInfo.Clear();
                    ZNet_Patch.GetOtherPublicPlayers(ZNet.instance, ___m_tempPlayerInfo);

                    foreach (ZNet.PlayerInfo m_Player in ___m_tempPlayerInfo)
                    {
                        Explore(__instance, m_Player.m_position, ___m_exploreRadius);
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(Minimap), "UpdateBiome")]
        [HarmonyPostfix]
        private static void Minimap_UpdateBiomePatch(Player player, Minimap __instance, Text ___m_biomeNameLarge, Text ___m_biomeNameSmall)
        {
            if (Settings.CoordsInMinimap.Value)
            {
                string coords = $"({Mathf.FloorToInt(player.transform.position.x)}, {Mathf.FloorToInt(player.transform.position.z)})";
                ___m_biomeNameSmall.text = ___m_biomeNameLarge.text.Split('\n')[0] + $"\n{coords}";
            }
            if (Settings.CoordsInMap.Value)
            {
                Vector3 cursor = ScreenToWorldPoint(__instance, UnityEngine.Input.mousePosition);
                string cursorCoords = $"({Mathf.FloorToInt(cursor.x)}, {Mathf.FloorToInt(cursor.z)})";
                ___m_biomeNameLarge.text = ___m_biomeNameSmall.text.Split('\n')[0] + $"\n{cursorCoords}";
            }
        }

        [HarmonyPatch(typeof(Minimap), "UpdateProfilePins")]
        [HarmonyPrefix]
        private static void Minimap_UpdateProfilePins(Minimap __instance, ref Minimap.PinData ___m_deathPin)
        {
            PlayerProfile prof = Game.instance.GetPlayerProfile();
            if (prof.HaveDeathPoint() && ___m_deathPin == null)
            {
                var text = string.Empty;
                if (Settings.MoreDetailsOnDeathMarkers.Value)
                    text = prof.GetName() + "\n" + DateTime.Now.ToString("hh:mm");
                __instance.AddPin(prof.GetDeathPoint(), Minimap.PinType.Death, text, true, false);
                if (Settings.ShareDeathMarkers.Value)
                    Plugin.SendPin(___m_deathPin, text);
            }
        }

        //[HarmonyPatch(typeof(Minimap), "UpdateNameInput")]
        //[HarmonyPrefix]
        //private static bool Minimap_UpdateNameInput(Minimap __instance, Minimap.PinData ___m_namePin, bool ___m_wasFocused)
        //{
        //    if (Settings.SharePinsWithOtherPlayers.Value)
        //    {
        //        if (__instance.m_nameInput.isFocused)
        //        {
        //            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        //            {
        //                string text = __instance.m_nameInput.text;
        //                text = text.Replace('$', ' ');
        //                text = text.Replace('<', ' ');
        //                text = text.Replace('>', ' ');

        //                if (Player.m_localPlayer)
        //                {
        //                    Plugin.SendPin(___m_namePin, text);
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        
    }
}
