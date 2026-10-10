using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Genes40k;

/// <summary>
/// Gives each affected pawn a few untended injuries on random outer body parts, never enough to down or kill them.
/// </summary>
public class ScenPart_CrashInjuries : ScenPart_PawnModifier
{
    private IntRange injuryCount = new(1, 3);
    private FloatRange injurySeverity = new(3f, 9f);
    private List<HediffDef> injuries = [];

    private const float SliderHeight = 31f;

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        var scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f + SliderHeight * 2f);
        var countRect = new Rect(scenPartRect.x, scenPartRect.y, scenPartRect.width, SliderHeight);
        var severityRect = new Rect(scenPartRect.x, countRect.yMax, scenPartRect.width, SliderHeight);

        Widgets.IntRange(countRect, listing.CurHeight.GetHashCode(), ref injuryCount, 0, 6, "BEWH.MankindsFinest.Scenario.InjuryCount");
        Widgets.FloatRange(severityRect, listing.CurHeight.GetHashCode() ^ 0x5A17, ref injurySeverity, 1f, 20f, "BEWH.MankindsFinest.Scenario.InjurySeverity", ToStringStyle.FloatOne);
        DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
    }

    public override string Summary(Scenario scen)
    {
        return "BEWH.MankindsFinest.Scenario.CrashInjuriesSummary".Translate(context.ToStringHuman(), injuryCount.min, injuryCount.max).CapitalizeFirst();
    }

    protected override void ModifyPawnPostGenerate(Pawn pawn, bool redressed)
    {
        if (redressed)
        {
            return;
        }

        AddInjuries(pawn);
    }

    /// <summary>
    /// Adds the rolled injuries one by one, skipping any that would down or kill the pawn or destroy the part.
    /// </summary>
    private void AddInjuries(Pawn pawn)
    {
        if (pawn?.health?.hediffSet == null || injuries.NullOrEmpty())
        {
            return;
        }

        var count = injuryCount.RandomInRange;

        for (var i = 0; i < count; i++)
        {
            if (!pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside).TryRandomElementByWeight(part => part.coverageAbs, out var part))
            {
                return;
            }

            var severity = Mathf.Min(injurySeverity.RandomInRange, pawn.health.hediffSet.GetPartHealth(part) - 1f);

            if (severity < 1f)
            {
                continue;
            }

            var injury = HediffMaker.MakeHediff(injuries.RandomElement(), pawn, part);
            injury.Severity = severity;

            if (pawn.health.WouldDieAfterAddingHediff(injury) || pawn.health.WouldBeDownedAfterAddingHediff(injury))
            {
                continue;
            }

            pawn.health.AddHediff(injury, part);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref injuryCount, "injuryCount", new IntRange(1, 3));
        Scribe_Values.Look(ref injurySeverity, "injurySeverity", new FloatRange(3f, 9f));
        Scribe_Collections.Look(ref injuries, "injuries", LookMode.Def);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            injuries ??= [];
        }
    }

    public override bool HasNullDefs()
    {
        return base.HasNullDefs() || injuries.Any(def => def == null);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ injuryCount.GetHashCode() ^ injurySeverity.GetHashCode();
    }
}
