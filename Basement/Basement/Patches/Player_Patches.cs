using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace Basement
{
    [Harmony]
    static class Player_Patches
    {
        const float overlapRadius = 20;

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void Player_UpdatePlacementGhost(Player __instance, GameObject ___m_placementGhost)
        {
            if (!___m_placementGhost) return;
            var basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (!basementComponent) return;

            if (Basement.allBasements == null) return;

            // Check for existing basements in range
            var ol = Basement.allBasements.Where(x => Vector3.Distance(x.transform.position, ___m_placementGhost.transform.position) < overlapRadius).Where(x => x.gameObject != ___m_placementGhost);

            // Reflection to override private member using private nested type
            Type type = typeof(Player).Assembly.GetType("Player+PlacementStatus");
            object moreSpace = type.GetField("MoreSpace").GetValue(__instance);
            FieldInfo statusField = __instance.GetType().GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance);

            if (ol.Any(x => x.GetComponentInParent<Basement>()) || ___m_placementGhost.transform.position.y > 2500 * Mathf.Max(Plugin.NestedBasementLimit.Value,0) + 2000)
            {
                statusField.SetValue(__instance, moreSpace);
            }
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            /*
            158 ldfld System.Boolean m_groundPiece
            159 brfalse System.Reflection.Emit.Label
            160 ldloc.s Heightmap (5)
            161 ldnull
            162 call Boolean op_Equality(UnityEngine.Object, UnityEngine.Object)
            163 brfalse System.Reflection.Emit.Label
            164 ldarg.0
            165 ldfld UnityEngine.GameObject m_placementGhost
            166 ldc.i4.0
            167 callvirt Void SetActive(Boolean)
            168 ldarg.0
            169 ldc.i4.1
            170 stfld Player+PlacementStatus m_placementStatus
            171 ret
            172 ldloc.1
            173 ldfld System.Boolean m_groundOnly
            174 brfalse System.Reflection.Emit.Label
            175 ldloc.s Heightmap (5)
            176 ldnull
            177 call Boolean op_Equality(UnityEngine.Object, UnityEngine.Object)
            178 brfalse System.Reflection.Emit.Label
            */
            codes[162] = CodeInstruction.Call(typeof(Player_Patches), "OverrideNullEqualityInBasement");
            codes[177]= CodeInstruction.Call(typeof(Player_Patches), "OverrideNullEqualityInBasement");
            return codes.AsEnumerable();
        }

        static bool OverrideNullEqualityInBasement(UnityEngine.Object a, UnityEngine.Object b)
        {
            if (EnvMan.instance.GetCurrentEnvironment().m_name == "Basement")
            {
                return false;
            }
            return a == b;
        }
    }
}
