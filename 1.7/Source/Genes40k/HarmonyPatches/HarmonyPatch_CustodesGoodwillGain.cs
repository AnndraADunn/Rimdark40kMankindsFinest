using HarmonyLib;
using RimWorld;

namespace Genes40k;

[HarmonyPatch(typeof(Faction), nameof(Faction.CalculateAdjustedGoodwillChange))]
public class CustodesGoodwillGain
{
    public static void Postfix(Faction __instance, Faction other, ref int __result)
    {
        if (__result <= 0 || other == null || (!__instance.IsPlayer && !other.IsPlayer))
        {
            return;
        }

        var factor = CustodesRankUtility.ColonyGoodwillGainFactor();
        if (factor > 1f)
        {
            __result = Mathf.CeilToInt(__result * factor);
        }
    }
}
