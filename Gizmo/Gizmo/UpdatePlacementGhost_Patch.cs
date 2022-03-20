using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Gizmo
{
    [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
    public static class UpdatePlacementGhost_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var placementAnglePatched = false;
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if(!placementAnglePatched)
                if (codes[i + 0].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 1].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 2].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 3].opcode == OpCodes.Ldfld &&
                    codes[i + 4].opcode == OpCodes.Conv_R4 &&
                    codes[i + 5].opcode == OpCodes.Mul &&
                    codes[i + 6].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 7].opcode == OpCodes.Call
                    )
                {
                    codes[i + 7] = CodeInstruction.Call(typeof(Plugin), "GetPlacementAngle");
                    placementAnglePatched = true;
                }

            }
            return codes.AsEnumerable();
        }
    }
}
