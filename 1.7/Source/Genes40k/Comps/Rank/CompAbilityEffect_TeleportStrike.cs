using RimWorld;
using Verse;

namespace Genes40k;

/// <summary>
/// Teleports the caster to the targeted cell, or beside the nearest beacon on the map when a beacon def is set, and stuns adjacent enemies on arrival.
/// </summary>
public class CompAbilityEffect_TeleportStrike : CompAbilityEffect
{
    public new CompProperties_AbilityTeleportStrike Props => (CompProperties_AbilityTeleportStrike)props;

    private bool UsesBeacon => Props.beaconDef != null;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);

        var pawn = parent.pawn;
        var map = pawn.Map;
        if (map == null)
        {
            return;
        }

        IntVec3 cell;
        if (UsesBeacon)
        {
            if (!TryFindBeaconCell(map, out cell))
            {
                return;
            }
        }
        else
        {
            cell = target.Cell;
            if (!CanLandAt(cell, map))
            {
                return;
            }
        }

        var origin = pawn.Position;
        parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(origin, map), origin, 60);
        parent.AddEffecterToMaintain(EffecterDefOf.Skip_Exit.Spawn(cell, map), cell, 60);

        pawn.Position = cell;
        pawn.Notify_Teleported();

        StunEnemiesAround(pawn, map, cell);
    }

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        if (!UsesBeacon && !CanLandAt(target.Cell, parent.pawn.Map))
        {
            if (throwMessages)
            {
                Messages.Message("BEWH.MankindsFinest.Custodes.CannotTeleportThere".Translate(), MessageTypeDefOf.RejectInput, false);
            }
            return false;
        }

        return base.Valid(target, throwMessages);
    }

    public override bool GizmoDisabled(out string reason)
    {
        if (UsesBeacon && parent.pawn.Map != null && !TryFindBeaconCell(parent.pawn.Map, out _))
        {
            reason = "BEWH.MankindsFinest.Custodes.NoBeaconOnMap".Translate(Props.beaconDef.label);
            return true;
        }

        return base.GizmoDisabled(out reason);
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        return false;
    }

    private static bool CanLandAt(IntVec3 cell, Map map)
    {
        return map != null && cell.IsValid && cell.InBounds(map) && cell.Standable(map) && !cell.Fogged(map);
    }

    private bool TryFindBeaconCell(Map map, out IntVec3 cell)
    {
        cell = IntVec3.Invalid;

        Thing nearest = null;
        var bestDistance = float.MaxValue;
        foreach (var beacon in map.listerThings.ThingsOfDef(Props.beaconDef))
        {
            var distance = (beacon.Position - parent.pawn.Position).LengthHorizontalSquared;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = beacon;
            }
        }

        if (nearest == null)
        {
            return false;
        }

        return CellFinder.TryFindRandomCellNear(nearest.Position, map, Props.beaconSearchRadius, c => CanLandAt(c, map) && c.GetFirstPawn(map) == null, out cell);
    }

    private void StunEnemiesAround(Pawn pawn, Map map, IntVec3 cell)
    {
        if (Props.stunTicks <= 0 || Props.stunRadius <= 0f)
        {
            return;
        }

        foreach (var thing in GenRadial.RadialDistinctThingsAround(cell, map, Props.stunRadius, true))
        {
            if (thing is not Pawn other || other == pawn || other.Downed || !other.HostileTo(pawn))
            {
                continue;
            }

            other.stances?.stunner?.StunFor(Props.stunTicks, pawn, addBattleLog: false);
        }
    }
}
