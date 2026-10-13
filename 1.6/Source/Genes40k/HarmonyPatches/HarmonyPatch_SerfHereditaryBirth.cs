using HarmonyLib;
using RimWorld;
using Verse;

namespace Genes40k;

[StaticConstructorOnStartup]
public static class SerfHereditaryBirth
{
    static SerfHereditaryBirth()
    {
        Genes40kMod.harmony.Patch(AccessTools.Method(typeof(PregnancyUtility), "ApplyBirthOutcome"), null, new HarmonyMethod(AccessTools.Method(typeof(SerfHereditaryBirth), "Postfix")));
    }

    /// <summary>
    /// A child whose genetic mother and father are both serfs is born a serf.
    /// </summary>
    public static void Postfix(Thing __result, Pawn geneticMother, Pawn father)
    {
        if (__result is not Pawn pawn || pawn.Dead)
        {
            return;
        }

        if (!SerfUtility.IsSerf(geneticMother) || !SerfUtility.IsSerf(father))
        {
            return;
        }

        SerfUtility.MakeSerf(pawn);
    }
}
