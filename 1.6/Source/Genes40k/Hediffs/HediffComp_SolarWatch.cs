namespace Genes40k;

public class HediffCompProperties_SolarWatch : HediffCompProperties_ConditionalSeverity
{
    public HediffCompProperties_SolarWatch()
    {
        compClass = typeof(HediffComp_SolarWatch);
    }
}

/// <summary>
/// Active while the pawn is on one of the player's home maps.
/// </summary>
public class HediffComp_SolarWatch : HediffComp_ConditionalSeverity
{
    protected override bool ConditionMet()
    {
        return Pawn.Map is { IsPlayerHome: true };
    }
}
