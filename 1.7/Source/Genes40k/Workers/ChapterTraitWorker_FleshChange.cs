using System.Collections.Generic;
using Verse;

namespace Genes40k;

/// <summary>
/// Drives the Flesh Change hediff applied by the trait: severity climbs while the marine carries psychic heat and
/// decays slowly when he does not, so a chapter that leans on its psykers pays for it in mutation.
/// </summary>
public class ChapterTraitWorker_FleshChange : ChapterTraitWorker
{
    public HediffDef hediff;

    /// <summary>Fraction of the pawn's entropy limit above which the mutation stirs.</summary>
    public float entropyThreshold = 0.15f;

    public float severityRise = 0.006f;
    public float severityFall = 0.0015f;
    public float minSeverity = 0.01f;
    public float maxSeverity = 1f;

    public override void PostAdd(Gene_CustomChapter gene, Pawn pawn)
    {
        EnsureHediff(gene, pawn);
    }

    public override void PostRemove(Gene_CustomChapter gene, Pawn pawn)
    {
        if (hediff == null || pawn?.health == null)
        {
            return;
        }

        var mutation = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);

        if (mutation != null)
        {
            pawn.health.RemoveHediff(mutation);
        }
    }

    public override void Tick(Gene_CustomChapter gene, Pawn pawn)
    {
        var mutation = EnsureHediff(gene, pawn);

        if (mutation == null)
        {
            return;
        }

        var delta = Stirring(pawn) ? severityRise : -severityFall;
        mutation.Severity = Mathf.Clamp(mutation.Severity + delta, minSeverity, maxSeverity);
    }

    /// <summary>
    /// Returns the pawn's flesh change, adding it first when the trait's gene is active but the hediff is missing.
    /// </summary>
    private Hediff EnsureHediff(Gene_CustomChapter gene, Pawn pawn)
    {
        if (hediff == null || pawn?.health?.hediffSet == null || pawn.Dead)
        {
            return null;
        }

        var mutation = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);

        if (mutation != null || gene is not { Active: true })
        {
            return mutation;
        }

        return pawn.health.AddHediff(hediff);
    }

    private bool Stirring(Pawn pawn)
    {
        if (!ModsConfig.RoyaltyActive)
        {
            return false;
        }

        var entropy = pawn.psychicEntropy;

        if (entropy == null)
        {
            return false;
        }

        var max = entropy.MaxEntropy;
        return max > 0f && entropy.EntropyValue / max > entropyThreshold;
    }

    public override IEnumerable<string> ConfigErrors()
    {
        if (hediff == null)
        {
            yield return "ChapterTraitWorker_FleshChange has no hediff";
        }

        if (tickInterval <= 0)
        {
            yield return "ChapterTraitWorker_FleshChange has no tickInterval, so it will never run";
        }
    }
}
