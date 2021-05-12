using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CraftyCarts.Patches
{
    [Harmony]
    class CraftingStation_Patch
    {
        [HarmonyPatch(typeof(CraftingStation), "Start")]
        [HarmonyPostfix]
        private static void CraftingStation_Start(CraftingStation __instance, ref List<CraftingStation> ___m_allStations)
        {
            if (__instance.name == "CraftyCarts.CraftingStation")
            {
                if(!___m_allStations.Contains(__instance))
                    ___m_allStations.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(CraftingStation), "FixedUpdate")]
        [HarmonyPostfix]
        private static void CraftingStation_FixedUpdate(CraftingStation __instance, ref float ___m_useTimer, ref float ___m_updateExtensionTimer, GameObject ___m_inUseObject)
        {
            if (__instance.name == "CraftyCarts.CraftingStation")
            {
                ___m_useTimer += Time.fixedDeltaTime;
                ___m_updateExtensionTimer += Time.fixedDeltaTime;
                if (___m_inUseObject)
                {
                    ___m_inUseObject.SetActive(___m_useTimer < 1f);
                }
            }
        }
    }
}
