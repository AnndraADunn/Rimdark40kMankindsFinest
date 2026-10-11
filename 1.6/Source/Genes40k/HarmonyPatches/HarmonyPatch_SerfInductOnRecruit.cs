using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "Interacted")]
public class SerfInductOnRecruit
{
    public static void Prefix(Pawn recipient, out float __state)
    {
        __state = -1f;

        if (recipient?.guest == null || !recipient.IsPrisonerOfColony)
        {
            return;
        }

        if (!recipient.guest.IsInteractionEnabled(Genes40kDefOf.BEWH_InductAsSerf) || !SerfUtility.CanBecomeSerf(recipient))
        {
            return;
        }

        __state = Mathf.Max(0f, recipient.guest.resistance);
    }

    public static void Postfix(Pawn initiator, Pawn recipient, float __state)
    {
        if (__state < 0f)
        {
            return;
        }

        if (recipient.Faction == Faction.OfPlayer && !recipient.IsPrisoner)
        {
            SerfUtility.MakeSerf(recipient);
            Messages.Message("BEWH.MankindsFinest.Serf.Inducted".Translate(recipient.Named("PAWN")), recipient, MessageTypeDefOf.PositiveEvent);
            return;
        }

        var reduced = __state - recipient.guest.resistance;

        if (reduced <= 0f || recipient.guest.resistance <= 0f || !SerfUtility.IsChaplain(initiator))
        {
            return;
        }

        recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - reduced * (SerfUtility.InductResistanceFactorChaplain - 1f));
    }
}
