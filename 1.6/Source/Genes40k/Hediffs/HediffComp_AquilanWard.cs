using RimWorld;
using Verse;

namespace Genes40k;

public class HediffCompProperties_AquilanWard : HediffCompProperties_ConditionalSeverity
{
    public float radius = 5f;

    public HediffCompProperties_AquilanWard()
    {
        compClass = typeof(HediffComp_AquilanWard);
    }
}

/// <summary>
/// Active while a titled colonist or the ideoligion leader (other than the pawn itself) is nearby.
/// </summary>
public class HediffComp_AquilanWard : HediffComp_ConditionalSeverity
{
    private HediffCompProperties_AquilanWard WardProps => (HediffCompProperties_AquilanWard)props;

    protected override bool ConditionMet()
    {
        var pawn = Pawn;
        if (!pawn.Spawned)
        {
            return false;
        }

        var radiusSquared = WardProps.radius * WardProps.radius;
        foreach (var other in pawn.Map.mapPawns.FreeColonistsSpawned)
        {
            if (other == pawn || other.Dead)
            {
                continue;
            }

            if ((other.Position - pawn.Position).LengthHorizontalSquared > radiusSquared)
            {
                continue;
            }

            if (IsWardWorthy(other))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsWardWorthy(Pawn pawn)
    {
        if (ModsConfig.RoyaltyActive && pawn.royalty?.MostSeniorTitle != null)
        {
            return true;
        }

        return ModsConfig.IdeologyActive && pawn.Ideo?.GetRole(pawn)?.def.leaderRole == true;
    }
}
