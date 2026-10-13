using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Genes40k;

/// <summary>
/// Lets gene-seed keepers take a gene-seed vial into their inventory where vanilla would not allow picking it up.
/// </summary>
public class FloatMenuOptionProvider_CarryGeneseedVial : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    protected override bool AppliesInt(FloatMenuContext context)
    {
        return context.FirstSelectedPawn.IsGeneseedKeeper();
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        var pawn = context.FirstSelectedPawn;

        if (clickedThing is not GeneseedVial vial || pawn.inventory == null || PawnUtility.CanPickUp(pawn, vial.def))
        {
            yield break;
        }

        var label = "BEWH.MankindsFinest.GeneseedKeeper.CarryVial".Translate(vial.Label);

        if (!pawn.CanReach(vial, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            yield return new FloatMenuOption(label + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            yield break;
        }

        if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, vial, vial.stackCount))
        {
            yield return new FloatMenuOption(label + ": " + "TooHeavy".Translate(), null);
            yield break;
        }

        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
        {
            vial.SetForbidden(false, false);
            var job = JobMaker.MakeJob(JobDefOf.TakeInventory, vial);
            job.count = vial.stackCount;
            job.checkEncumbrance = true;
            job.takeInventoryDelay = 120;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }, MenuOptionPriority.High), pawn, vial);
    }
}
