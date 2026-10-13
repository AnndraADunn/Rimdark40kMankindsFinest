using System;
using Core40k;
using RimWorld;
using Verse;

namespace Genes40k;

public static class SerfUtility
{
    public const float ImplantSurvivalChance = 0.2f;
    public const float SavedOathChance = 0.15f;
    public const float InductResistanceFactorChaplain = 1.5f;

    public static bool CanBecomeSerf(Pawn pawn)
    {
        if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike || pawn.story?.traits == null || pawn.genes == null)
        {
            return false;
        }

        if (pawn.IsSuperHuman() || pawn.UndergoingPhaseDevelopment())
        {
            return false;
        }

        return !pawn.story.traits.HasTrait(Genes40kDefOf.BEWH_Serf);
    }

    public static void MakeSerf(Pawn pawn)
    {
        if (!CanBecomeSerf(pawn))
        {
            return;
        }

        pawn.story.traits.GainTrait(new Trait(Genes40kDefOf.BEWH_Serf));
    }

    public static bool IsChaplain(Pawn pawn)
    {
        var rankComp = pawn?.GetComp<CompRankInfo>();

        return rankComp != null && (rankComp.HasRank(Genes40kDefOf.BEWH_AstartesRankChaplain) || rankComp.HasRank(Genes40kDefOf.BEWH_AstartesRankMasterOfSanctity));
    }

    /// <summary>
    /// Rolls whether a pawn whose gene-seed implantation failed lives through it. On success the player is asked
    /// whether the pawn takes up serfdom, and the caller must not kill the pawn.
    /// </summary>
    public static bool TrySurviveFailedImplantation(Pawn pawn)
    {
        if (!CanBecomeSerf(pawn) || !Rand.Chance(ImplantSurvivalChance))
        {
            return false;
        }

        Find.WindowStack.Add(new Dialog_MessageBox(
            "BEWH.MankindsFinest.Serf.FailedImplantText".Translate(pawn.Named("PAWN")),
            "BEWH.MankindsFinest.Serf.FailedImplantAccept".Translate(),
            delegate
            {
                MakeSerf(pawn);
                GainMemory(pawn, Genes40kDefOf.BEWH_SerfStillServe);
            },
            "BEWH.MankindsFinest.Serf.FailedImplantRefuse".Translate(),
            delegate
            {
                GainMemory(pawn, Genes40kDefOf.BEWH_FailedImplantation);
            },
            "BEWH.MankindsFinest.Serf.FailedImplantTitle".Translate(pawn.Named("PAWN"))));

        return true;
    }

    /// <summary>
    /// Called when a downed pawn was tended or rescued. A mortal colonist saved by a superhuman one gets a memory,
    /// and may ask to swear themselves to the chapter.
    /// </summary>
    public static void Notify_SavedBy(Pawn saved, Pawn saviour)
    {
        if (saved == null || saviour == null || saved == saviour)
        {
            return;
        }

        if (!saved.RaceProps.Humanlike || !saved.IsFreeNonSlaveColonist || saviour.Faction != saved.Faction)
        {
            return;
        }

        if (saved.genes == null || saved.IsSuperHuman() || saviour.genes == null || !saviour.IsSuperHuman())
        {
            return;
        }

        var memories = saved.needs?.mood?.thoughts?.memories;

        if (memories == null || memories.GetFirstMemoryOfDef(Genes40kDefOf.BEWH_SavedByAngels) != null)
        {
            return;
        }

        memories.TryGainMemory(Genes40kDefOf.BEWH_SavedByAngels, saviour);

        if (!CanBecomeSerf(saved) || !saved.DevelopmentalStage.Adult())
        {
            return;
        }

        var utils = Current.Game?.GetComponent<GameComponent_MankindFinestUtils>();

        if (utils == null || utils.HasDeclinedSerfOath(saved) || !Rand.Chance(SavedOathChance))
        {
            return;
        }

        Find.WindowStack.Add(new Dialog_MessageBox(
            "BEWH.MankindsFinest.Serf.SavedOathText".Translate(saved.Named("PAWN"), saviour.Named("SAVIOUR")),
            "BEWH.MankindsFinest.Serf.SavedOathAccept".Translate(),
            delegate
            {
                MakeSerf(saved);
            },
            "BEWH.MankindsFinest.Serf.SavedOathRefuse".Translate(),
            delegate
            {
                utils.Notify_SerfOathDeclined(saved);
            },
            "BEWH.MankindsFinest.Serf.SavedOathTitle".Translate(saved.Named("PAWN"))));
    }

    /// <summary>
    /// Rank abilities are only handed out when a rank is unlocked, so marines who were already Chaplains get the oath ability here.
    /// </summary>
    [Obsolete]
    public static void GrantOathAbilityToChaplains()
    {
        var oath = Genes40kDefOf.BEWH_ChaplainOathOfService;

        foreach (var pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive)
        {
            if (pawn.abilities == null || pawn.abilities.GetAbility(oath) != null)
            {
                continue;
            }

            var rankComp = pawn.GetComp<CompRankInfo>();

            if (rankComp != null && rankComp.HasRank(Genes40kDefOf.BEWH_AstartesRankChaplain))
            {
                pawn.abilities.GainAbility(oath);
            }
        }
    }

    private static void GainMemory(Pawn pawn, ThoughtDef thought)
    {
        if (pawn.Dead)
        {
            return;
        }

        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
    }
}
