using RimWorld;
using Verse;
using Verse.AI;

namespace Genes40k;

/// <summary>
/// Gene-seed keepers carrying vials put them into a protective storage once one has room.
/// </summary>
public class WorkGiver_DepositGeneseed : WorkGiver
{
    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        if (pawn.Map == null || !pawn.Map.IsPlayerHome || pawn.inventory == null || !pawn.IsGeneseedKeeper())
        {
            return true;
        }

        var container = pawn.inventory.innerContainer;
        for (var i = 0; i < container.Count; i++)
        {
            if (container[i] is GeneseedVial)
            {
                return false;
            }
        }

        return true;
    }

    public override Job NonScanJob(Pawn pawn)
    {
        var container = pawn.inventory.innerContainer;

        for (var i = 0; i < container.Count; i++)
        {
            if (container[i] is not GeneseedVial vial || !GeneseedKeeperUtility.TryFindProtectedStoreCell(vial, pawn, out var cell))
            {
                continue;
            }

            var job = JobMaker.MakeJob(Genes40kDefOf.BEWH_DepositGeneseedVial, vial, cell);
            job.count = vial.stackCount;
            return job;
        }

        return null;
    }
}
