using System.Collections.Generic;
using System.Linq;
using Core40k;
using RimWorld;
using Verse;

namespace Genes40k;

public static class GeneseedKeeperUtility
{
    private static List<ThingDef> vialDefs;

    /// <summary>
    /// Every thing def that spawns as a gene-seed vial.
    /// </summary>
    public static List<ThingDef> VialDefs => vialDefs ??= DefDatabase<ThingDef>.AllDefsListForReading
        .Where(def => def.thingClass != null && typeof(GeneseedVial).IsAssignableFrom(def.thingClass))
        .ToList();

    /// <summary>
    /// Whether the pawn holds a rank that makes it a keeper of gene-seed (Helix Adept and up the apothecary line).
    /// </summary>
    public static bool IsGeneseedKeeper(this Pawn pawn)
    {
        var comp = pawn?.GetComp<CompRankInfo>();
        return comp != null && comp.UnlockedRanks.Any(rank => rank.HasModExtension<DefModExtension_GeneseedHarvest>());
    }

    public static bool IsDeteriorating(Thing vial)
    {
        return vial.TryGetComp<Comp_DeteriorateOutsideBuilding>()?.ShouldDeteriorate ?? false;
    }

    /// <summary>
    /// Finds the closest cell in a powered storage that keeps the vial from deteriorating and can take it.
    /// </summary>
    public static bool TryFindProtectedStoreCell(Thing vial, Pawn carrier, out IntVec3 cell)
    {
        cell = IntVec3.Invalid;

        var map = carrier?.MapHeld;
        var containers = vial.TryGetComp<Comp_DeteriorateOutsideBuilding>()?.Props.antiDeteriorateContainers;

        if (map == null || containers.NullOrEmpty())
        {
            return false;
        }

        var origin = carrier.PositionHeld;
        var bestDistance = float.MaxValue;

        foreach (var containerDef in containers)
        {
            foreach (var building in map.listerBuildings.AllBuildingsColonistOfDef(containerDef))
            {
                if (building is not Building_Storage storage || !storage.Accepts(vial))
                {
                    continue;
                }

                var power = storage.TryGetComp<CompPowerTrader>();
                if (power is { PowerOn: false })
                {
                    continue;
                }

                foreach (var slot in storage.AllSlotCellsList())
                {
                    if (!StoreUtility.IsGoodStoreCell(slot, map, vial, carrier, carrier.Faction))
                    {
                        continue;
                    }

                    var distance = (slot - origin).LengthHorizontalSquared;
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    cell = slot;
                }
            }
        }

        return cell.IsValid;
    }
}
