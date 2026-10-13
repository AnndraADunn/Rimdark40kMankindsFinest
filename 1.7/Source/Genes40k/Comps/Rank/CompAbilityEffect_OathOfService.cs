using System;
using RimWorld;
using Verse;

namespace Genes40k;

public class CompAbilityEffect_OathOfService : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);

        var pawn = target.Pawn;

        if (!SerfUtility.CanBecomeSerf(pawn))
        {
            return;
        }

        SerfUtility.MakeSerf(pawn);
        Messages.Message("BEWH.MankindsFinest.Serf.OathTaken".Translate(pawn.Named("PAWN"), parent.pawn.Named("CHAPLAIN")), pawn, MessageTypeDefOf.PositiveEvent);
    }

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        if (!base.Valid(target, throwMessages))
        {
            return false;
        }

        var reason = InvalidReason(target.Pawn);

        if (reason == null)
        {
            return true;
        }

        if (throwMessages)
        {
            Messages.Message(reason, MessageTypeDefOf.RejectInput, false);
        }

        return false;
    }

    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        return target.Pawn == null ? null : InvalidReason(target.Pawn);
    }

    public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
    {
        return Dialog_MessageBox.CreateConfirmation("BEWH.MankindsFinest.Serf.OathConfirm".Translate(target.Pawn.Named("PAWN")), confirmAction, true);
    }

    private static string InvalidReason(Pawn pawn)
    {
        if (pawn == null)
        {
            return "BEWH.MankindsFinest.Ability.MustBePawn".Translate();
        }

        if (!pawn.IsFreeNonSlaveColonist)
        {
            return "BEWH.MankindsFinest.Serf.MustBeColonist".Translate(pawn.Named("PAWN"));
        }

        if (!pawn.DevelopmentalStage.Adult())
        {
            return "BEWH.MankindsFinest.Serf.MustBeAdult".Translate(pawn.Named("PAWN"));
        }

        if (pawn.story?.traits != null && pawn.story.traits.HasTrait(Genes40kDefOf.BEWH_Serf))
        {
            return "BEWH.MankindsFinest.Serf.AlreadySerf".Translate(pawn.Named("PAWN"));
        }

        if (!SerfUtility.CanBecomeSerf(pawn))
        {
            return "BEWH.MankindsFinest.Serf.CannotBecomeSerf".Translate(pawn.Named("PAWN"));
        }

        return null;
    }
}
