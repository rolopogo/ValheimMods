using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DaisyChain
{
    class CraftingStation_Patch
    {
        [HarmonyPatch(typeof(CraftingStation), "HaveBuildStationInRange", new Type[] { typeof(string), typeof(Vector3) })]
        [HarmonyPostfix]
        private static void CraftingStation_HaveBuildStationInRange(string name, Vector3 point, List<CraftingStation> ___m_allStations, ref CraftingStation __result)
        {
            if (!Plugin.instance.enabledConfig.Value) return;
            if (__result != null) return;

            foreach (CraftingStation craftingStation in ___m_allStations)
            {
                if (Vector3.Distance(craftingStation.transform.position, point) < craftingStation.m_rangeBuild)
                {
                    List<CraftingStation> networkStations = new List<CraftingStation>();
                    RecursiveAddNearbyStations(craftingStation, ref networkStations, ___m_allStations);

                    foreach (var item in networkStations)
                    {
                        if (Input.GetKeyDown(KeyCode.N)) Debug.Log(item.m_name);
                        if (item.m_name == name)
                        {
                            __result = item;
                            craftingStation.ShowAreaMarker();
                        }
                    }
                }
            }
        }

        private static List<CraftingStation> RecursiveAddNearbyStations(CraftingStation from, ref List<CraftingStation> networkStations, List<CraftingStation> allStations)
        {
            if (networkStations.Contains(from)) return networkStations;

            networkStations.Add(from);

            foreach (CraftingStation craftingStation in allStations)
            {
                if (Vector3.Distance(craftingStation.transform.position, from.transform.position) < Mathf.Max(craftingStation.m_rangeBuild, from.m_rangeBuild))
                {
                    RecursiveAddNearbyStations(craftingStation, ref networkStations, allStations);
                }
            }
            return networkStations;
        }
    }
}
