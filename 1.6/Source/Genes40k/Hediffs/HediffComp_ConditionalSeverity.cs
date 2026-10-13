using UnityEngine;
using Verse;

namespace Genes40k;

public class HediffCompProperties_ConditionalSeverity : HediffCompProperties
{
    public int checkIntervalTicks = 250;
    public float activeSeverity = 1f;
    public float inactiveSeverity = 0.5f;
}

/// <summary>
/// Switches the parent hediff between an active and an inactive severity, and so between two stages, based on a condition checked on an interval.
/// </summary>
public abstract class HediffComp_ConditionalSeverity : HediffComp
{
    public HediffCompProperties_ConditionalSeverity Props => (HediffCompProperties_ConditionalSeverity)props;

    protected abstract bool ConditionMet();

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        UpdateSeverity();
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        base.CompPostTickInterval(ref severityAdjustment, delta);
        if (!Pawn.IsHashIntervalTick(Props.checkIntervalTicks, delta))
        {
            return;
        }

        UpdateSeverity();
    }

    protected void UpdateSeverity()
    {
        var targetSeverity = ConditionMet() ? Props.activeSeverity : Props.inactiveSeverity;
        if (!Mathf.Approximately(parent.Severity, targetSeverity))
        {
            parent.Severity = targetSeverity;
        }
    }
}
