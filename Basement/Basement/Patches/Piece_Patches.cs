using HarmonyLib;

namespace Basement
{
    [Harmony]
    class Piece_Patches
    {
        [HarmonyPatch(typeof(Piece), "CanBeRemoved")]
        [HarmonyPostfix]
        public static void Piece_CanBeRemoved(Piece __instance, ref bool __result)
        {
            var basement = __instance.GetComponent<Basement>();
            if (basement)
            {
                if (!basement.CanBeRemoved())
                    __result = false;
            }
        }
    }
}
