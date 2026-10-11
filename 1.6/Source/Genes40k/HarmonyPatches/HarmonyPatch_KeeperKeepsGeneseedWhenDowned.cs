using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Genes40k;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
public static class KeeperKeepsGeneseedWhenDowned
{
    public static void Prefix(Pawn ___pawn, out List<Thing> __state)
    {
        __state = null;

        if (___pawn?.inventory == null || ___pawn.Dead || !___pawn.IsGeneseedKeeper())
        {
            return;
        }

        var container = ___pawn.inventory.innerContainer;

        for (var i = container.Count - 1; i >= 0; i--)
        {
            if (container[i] is not GeneseedVial vial)
            {
                continue;
            }

            container.Remove(vial);
            (__state ??= []).Add(vial);
        }
    }

    public static void Postfix(Pawn ___pawn, List<Thing> __state)
    {
        if (__state == null)
        {
            return;
        }

        foreach (var vial in __state)
        {
            if (___pawn.inventory.innerContainer.TryAdd(vial, false))
            {
                continue;
            }

            if (___pawn.MapHeld != null)
            {
                GenPlace.TryPlaceThing(vial, ___pawn.PositionHeld, ___pawn.MapHeld, ThingPlaceMode.Near);
            }
        }
    }
}
