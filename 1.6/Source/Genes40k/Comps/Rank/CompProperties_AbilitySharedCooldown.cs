using RimWorld;

namespace Genes40k;

public class CompProperties_AbilitySharedCooldown : CompProperties_AbilityEffect
{
    public CompProperties_AbilitySharedCooldown()
    {
        compClass = typeof(CompAbilityEffect_SharedCooldown);
    }
}
