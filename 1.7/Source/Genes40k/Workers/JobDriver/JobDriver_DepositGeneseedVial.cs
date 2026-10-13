using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Genes40k;

/// <summary>
/// Takes a gene-seed vial out of the pawn's inventory and places it in the target storage cell.
/// </summary>
public class JobDriver_DepositGeneseedVial : JobDriver
{
    private const TargetIndex VialInd = TargetIndex.A;
    private const TargetIndex CellInd = TargetIndex.B;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.GetTarget(CellInd), job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoCell(CellInd, PathEndMode.Touch);

        var takeOut = ToilMaker.MakeToil("TakeVialFromInventory");
        takeOut.initAction = delegate
        {
            var vial = job.GetTarget(VialInd).Thing;

            if (vial == null || !pawn.inventory.innerContainer.Contains(vial))
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            pawn.inventory.innerContainer.TryTransferToContainer(vial, pawn.carryTracker.innerContainer, job.count, out var carried);

            if (carried == null)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            carried.SetForbidden(false, false);
            job.count = carried.stackCount;
            job.SetTarget(VialInd, carried);
        };
        yield return takeOut;

        var carryToCell = Toils_Haul.CarryHauledThingToCell(CellInd);
        yield return carryToCell;
        yield return Toils_Haul.PlaceHauledThingInCell(CellInd, carryToCell, true);
    }
}
