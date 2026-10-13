using HarmonyLib;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
public class CustodesIncomingDamage
{
    public static void Postfix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
    {
        if (absorbed || dinfo.Amount <= 0f)
        {
            return;
        }

        CustodesRankUtility.ModifyIncomingDamage(__instance, ref dinfo);
    }
}
