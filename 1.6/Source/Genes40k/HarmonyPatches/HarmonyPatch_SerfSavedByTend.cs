using HarmonyLib;
using RimWorld;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(TendUtility), "DoTend")]
public class SerfSavedByTend
{
    public static void Postfix(Pawn doctor, Pawn patient)
    {
        if (patient == null || !patient.Downed)
        {
            return;
        }

        SerfUtility.Notify_SavedBy(patient, doctor);
    }
}
