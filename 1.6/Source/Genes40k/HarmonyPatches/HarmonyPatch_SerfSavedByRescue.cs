using HarmonyLib;
using RimWorld;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(Pawn_RelationsTracker), "Notify_RescuedBy")]
public class SerfSavedByRescue
{
    public static void Postfix(Pawn ___pawn, Pawn rescuer)
    {
        SerfUtility.Notify_SavedBy(___pawn, rescuer);
    }
}
