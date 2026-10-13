using RimWorld;
using Verse;

namespace Genes40k;

/// <summary>
/// Puts every other free colonist's copy of this ability on cooldown when it is cast.
/// </summary>
public class CompAbilityEffect_SharedCooldown : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);

        var cooldownTicks = parent.def.cooldownTicksRange.max;
        if (cooldownTicks <= 0)
        {
            return;
        }

        foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
        {
            if (pawn == parent.pawn)
            {
                continue;
            }

            var ability = pawn.abilities?.GetAbility(parent.def);
            if (ability == null || ability.CooldownTicksRemaining >= cooldownTicks)
            {
                continue;
            }

            ability.StartCooldown(cooldownTicks);
        }
    }
}
