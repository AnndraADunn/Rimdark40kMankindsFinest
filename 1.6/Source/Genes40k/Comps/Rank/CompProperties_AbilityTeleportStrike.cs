using RimWorld;
using Verse;

namespace Genes40k;

public class CompProperties_AbilityTeleportStrike : CompProperties_AbilityEffect
{
    public ThingDef beaconDef;
    public int beaconSearchRadius = 2;
    public float stunRadius = 1.9f;
    public int stunTicks = 90;

    public CompProperties_AbilityTeleportStrike()
    {
        compClass = typeof(CompAbilityEffect_TeleportStrike);
    }
}
