using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Genes40k;

/// <summary>
/// Gene-seed keepers rescue deteriorating vials: into a protective storage when one has room, otherwise into their inventory.
/// </summary>
public class WorkGiver_SecureGeneseed : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return pawn.Map == null || !pawn.Map.IsPlayerHome || pawn.inventory == null || !pawn.IsGeneseedKeeper();
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (var def in GeneseedKeeperUtility.VialDefs)
        {
            foreach (var thing in pawn.Map.listerThings.ThingsOfDef(def))
            {
                yield return thing;
            }
        }
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not GeneseedVial vial || !vial.Spawned || vial.IsForbidden(pawn) || !GeneseedKeeperUtility.IsDeteriorating(vial))
        {
            return false;
        }

        if (!pawn.CanReserve(vial, 1, -1, null, forced))
        {
            return false;
        }

        if (GeneseedKeeperUtility.TryFindProtectedStoreCell(vial, pawn, out _))
        {
            return true;
        }

        return !MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, vial, vial.stackCount);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (GeneseedKeeperUtility.TryFindProtectedStoreCell(t, pawn, out var cell))
        {
            return HaulAIUtility.HaulToCellStorageJob(pawn, t, cell, false);
        }

        var job = JobMaker.MakeJob(JobDefOf.TakeInventory, t);
        job.count = t.stackCount;
        job.checkEncumbrance = true;
        return job;
    }
}
