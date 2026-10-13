using Verse;

namespace Genes40k;

public class HediffCompProperties_Unyielding : HediffCompProperties_ConditionalSeverity
{
    public int ticksStillToActivate = 120;

    public HediffCompProperties_Unyielding()
    {
        compClass = typeof(HediffComp_Unyielding);
    }
}

/// <summary>
/// Active once the pawn has stood still for a short while.
/// </summary>
public class HediffComp_Unyielding : HediffComp_ConditionalSeverity
{
    private int lastMovedTick = -99999;

    private HediffCompProperties_Unyielding UnyieldingProps => (HediffCompProperties_Unyielding)props;

    protected override bool ConditionMet()
    {
        var pawn = Pawn;
        if (!pawn.Spawned || pawn.Downed)
        {
            return false;
        }

        var ticksGame = Find.TickManager.TicksGame;
        if (pawn.pather is { Moving: true })
        {
            lastMovedTick = ticksGame;
            return false;
        }

        return ticksGame - lastMovedTick >= UnyieldingProps.ticksStillToActivate;
    }
}
