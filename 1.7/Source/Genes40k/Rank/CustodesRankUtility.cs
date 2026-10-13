using Core40k;
using RimWorld;
using Verse;

namespace Genes40k;

public static class CustodesRankUtility
{
    private const int GoodwillFactorCacheTicks = 250;

    private static bool redirectingDamage;

    private static int goodwillFactorCachedTick = -1;
    private static float cachedGoodwillFactor = 1f;

    /// <summary>
    /// Applies the Custodes rank damage effects (excision marks, Vexilla wards and Aquilan Shield interposition) to damage a pawn is about to take.
    /// </summary>
    public static void ModifyIncomingDamage(Pawn victim, ref DamageInfo dinfo)
    {
        var hediffs = victim.health?.hediffSet?.hediffs;
        if (hediffs.NullOrEmpty())
        {
            return;
        }

        Hediff interposeHediff = null;
        for (var i = 0; i < hediffs.Count; i++)
        {
            var hediff = hediffs[i];
            if (hediff.def == Genes40kDefOf.BEWH_DreadHostMark)
            {
                if (dinfo.Instigator is Pawn instigator)
                {
                    var factor = MarkedTargetDamageFactor(instigator, victim);
                    if (factor != 1f)
                    {
                        dinfo.SetAmount(dinfo.Amount * factor);
                    }
                }
                continue;
            }

            if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                var rangedFactor = hediff.def.GetModExtension<DefModExtension_IncomingRangedDamageFactor>();
                if (rangedFactor != null)
                {
                    dinfo.SetAmount(dinfo.Amount * rangedFactor.factor);
                    continue;
                }
            }

            if (interposeHediff == null && hediff.def.HasModExtension<DefModExtension_RedirectDamageToLink>())
            {
                interposeHediff = hediff;
            }
        }

        if (interposeHediff != null)
        {
            TryRedirectDamage(victim, interposeHediff, ref dinfo);
        }
    }

    private static float MarkedTargetDamageFactor(Pawn instigator, Pawn victim)
    {
        var rankComp = instigator.GetComp<CompRankInfo>();
        if (rankComp == null)
        {
            return 1f;
        }

        DefModExtension_MarkedTargetDamage best = null;
        foreach (var rank in rankComp.UnlockedRanks)
        {
            var extension = rank.GetModExtension<DefModExtension_MarkedTargetDamage>();
            if (extension != null && (best == null || extension.damageFactor > best.damageFactor))
            {
                best = extension;
            }
        }

        if (best == null)
        {
            return 1f;
        }

        return IsPsykerOrEntity(victim) ? best.psykerOrEntityDamageFactor : best.damageFactor;
    }

    private static bool IsPsykerOrEntity(Pawn pawn)
    {
        if (pawn.RaceProps.IsAnomalyEntity)
        {
            return true;
        }

        if (pawn.GetPsylinkLevel() > 0)
        {
            return true;
        }

        return pawn.genes != null && pawn.IsPsyker();
    }

    private static void TryRedirectDamage(Pawn victim, Hediff hediff, ref DamageInfo dinfo)
    {
        if (redirectingDamage || !dinfo.Def.ExternalViolenceFor(victim))
        {
            return;
        }

        var guardian = hediff.TryGetComp<HediffComp_Link>()?.other as Pawn;
        if (guardian == null || guardian == victim || guardian.Dead || guardian.Downed || !guardian.Spawned || guardian.Map != victim.MapHeld)
        {
            return;
        }

        var share = dinfo.Amount * hediff.def.GetModExtension<DefModExtension_RedirectDamageToLink>().redirectedFraction;
        if (share <= 0f)
        {
            return;
        }

        dinfo.SetAmount(dinfo.Amount - share);

        var redirected = new DamageInfo(dinfo.Def, share, dinfo.ArmorPenetrationInt, dinfo.Angle, dinfo.Instigator, null, dinfo.Weapon, dinfo.Category, guardian, dinfo.InstigatorGuilty);
        redirectingDamage = true;
        try
        {
            guardian.TakeDamage(redirected);
        }
        finally
        {
            redirectingDamage = false;
        }
    }

    /// <summary>
    /// The highest goodwill gain factor granted by a rank held by any free colonist, cached for a few ticks since goodwill previews call this from the UI.
    /// </summary>
    public static float ColonyGoodwillGainFactor()
    {
        if (Current.Game == null)
        {
            return 1f;
        }

        var ticksGame = Find.TickManager.TicksGame;
        if (goodwillFactorCachedTick >= 0 && ticksGame - goodwillFactorCachedTick < GoodwillFactorCacheTicks && ticksGame >= goodwillFactorCachedTick)
        {
            return cachedGoodwillFactor;
        }

        goodwillFactorCachedTick = ticksGame;
        cachedGoodwillFactor = 1f;
        foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep)
        {
            var rankComp = pawn.GetComp<CompRankInfo>();
            if (rankComp == null)
            {
                continue;
            }

            foreach (var rank in rankComp.UnlockedRanks)
            {
                var extension = rank.GetModExtension<DefModExtension_GoodwillGainFactor>();
                if (extension != null && extension.factor > cachedGoodwillFactor)
                {
                    cachedGoodwillFactor = extension.factor;
                }
            }
        }

        return cachedGoodwillFactor;
    }
}
