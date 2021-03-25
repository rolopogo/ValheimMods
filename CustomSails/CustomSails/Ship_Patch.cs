using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomSails
{
    class Ship_Patch
    {
        [HarmonyPatch(typeof(Ship), "Awake", new Type[] { })]
        [HarmonyPostfix]
        private static void Ship_Awake(Ship __instance)
        {
            if(!__instance.gameObject.GetComponent<ShipSailUrlSetter>())
                __instance.gameObject.AddComponent<ShipSailUrlSetter>();
        }
    }
}
