using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gizmo
{
    class UpdatePlacement_Patch
    {
        [HarmonyPatch(typeof(Player), "UpdatePlacement", new Type[] { typeof(bool), typeof(float) })]
        [HarmonyPostfix]
        private static void Player_UpdatePlacement(Player __instance, GameObject ___m_placementGhost, bool takeInput, float dt)
        {
            Plugin.instance.UpdatePlacement(__instance, ___m_placementGhost, takeInput);
        }
    }
}
