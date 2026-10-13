using Verse;

namespace Genes40k;

/// <summary>
/// On a rank: damage this pawn deals to a target marked for excision is multiplied by these factors.
/// </summary>
public class DefModExtension_MarkedTargetDamage : DefModExtension
{
    public float damageFactor = 1.5f;
    public float psykerOrEntityDamageFactor = 2f;
}

/// <summary>
/// On a rank: positive goodwill changes with other factions are multiplied while a colonist holds this rank.
/// </summary>
public class DefModExtension_GoodwillGainFactor : DefModExtension
{
    public float factor = 1.25f;
}

/// <summary>
/// On a hediff carrying a HediffComp_Link: this share of incoming violent damage is redirected to the linked pawn.
/// </summary>
public class DefModExtension_RedirectDamageToLink : DefModExtension
{
    public float redirectedFraction = 0.5f;
}

/// <summary>
/// On a hediff: incoming ranged and explosive damage is multiplied by this factor.
/// </summary>
public class DefModExtension_IncomingRangedDamageFactor : DefModExtension
{
    public float factor = 0.8f;
}
