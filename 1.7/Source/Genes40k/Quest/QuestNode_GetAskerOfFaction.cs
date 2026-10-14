using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Genes40k;

/// <summary>
/// Picks a quest asker from one specific faction: either its leader or a titled member,
/// generating a titled member of titledPawnKind when there are few to choose from.
/// </summary>
public class QuestNode_GetAskerOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef;

    public SlateRef<PawnKindDef> titledPawnKind;

    public SlateRef<FloatRange> seniorityRange;

    public SlateRef<bool> allowLeader = true;

    public SlateRef<bool> allowTitled = true;

    public SlateRef<float> titledChance = 0.6f;

    public SlateRef<bool> mustBeNonHostileToPlayer = true;

    public SlateRef<int> maxUsablePawnsToGenerate = 5;

    protected override bool TestRunInt(Slate slate)
    {
        var faction = GetFaction(slate);
        if (faction == null)
        {
            return false;
        }
        if (slate.TryGet<Pawn>(storeAs.GetValue(slate), out var existing) && (IsGoodLeader(existing, faction, slate) || IsGoodTitled(existing, faction, slate)))
        {
            return true;
        }

        var leaderOk = IsGoodLeader(faction.leader, faction, slate);
        var titledOk = CanUseTitled(faction, slate);
        if (!leaderOk && !titledOk)
        {
            return false;
        }

        var titled = titledOk ? ExistingTitled(faction, slate).ToList() : new List<Pawn>();
        if (titled.Any())
        {
            slate.Set(storeAs.GetValue(slate), titled.RandomElement());
        }
        else if (leaderOk)
        {
            slate.Set(storeAs.GetValue(slate), faction.leader);
        }
        return true;
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var faction = GetFaction(slate);
        if (faction == null)
        {
            return;
        }
        if (slate.TryGet<Pawn>(storeAs.GetValue(slate), out var existing) && (IsGoodLeader(existing, faction, slate) || IsGoodTitled(existing, faction, slate)))
        {
            AddInvolvedFaction(existing);
            return;
        }

        var leaderOk = IsGoodLeader(faction.leader, faction, slate);
        var titledOk = CanUseTitled(faction, slate);
        Pawn asker = null;

        if (titledOk && (!leaderOk || Rand.Chance(titledChance.GetValue(slate))))
        {
            var titled = ExistingTitled(faction, slate).ToList();
            var generateChance = 1f - (float)titled.Count / System.Math.Max(1, maxUsablePawnsToGenerate.GetValue(slate));
            asker = titled.Count == 0 || Rand.Chance(generateChance) ? GenerateTitled(faction, slate) : titled.RandomElement();
        }

        if (asker == null && leaderOk)
        {
            asker = faction.leader;
        }
        if (asker == null)
        {
            return;
        }

        slate.Set(storeAs.GetValue(slate), asker);
        AddInvolvedFaction(asker);
    }

    private Faction GetFaction(Slate slate)
    {
        var def = factionDef.GetValue(slate);
        if (def == null)
        {
            return null;
        }
        var faction = Find.FactionManager.FirstFactionOfDef(def);
        if (faction == null || faction.defeated || faction.Hidden || faction.IsPlayer)
        {
            return null;
        }
        if (mustBeNonHostileToPlayer.GetValue(slate) && faction.HostileTo(Faction.OfPlayer))
        {
            return null;
        }
        return faction;
    }

    private bool IsGoodLeader(Pawn pawn, Faction faction, Slate slate)
    {
        if (!allowLeader.GetValue(slate) || pawn == null || pawn.Dead || pawn.Faction != faction || faction.leader != pawn || pawn.IsPrisoner)
        {
            return false;
        }
        if (pawn.IsWorldPawn())
        {
            var situation = Find.WorldPawns.GetSituation(pawn);
            return situation == WorldPawnSituation.Free || situation == WorldPawnSituation.FactionLeader;
        }
        return true;
    }

    private bool IsGoodTitled(Pawn pawn, Faction faction, Slate slate)
    {
        if (!allowTitled.GetValue(slate) || pawn == null || pawn.Dead || pawn.Faction != faction || pawn.IsPrisoner || pawn.royalty == null)
        {
            return false;
        }
        if (!pawn.IsWorldPawn() || Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free)
        {
            return false;
        }
        var title = pawn.royalty.GetCurrentTitle(faction);
        return title != null && InRange(title, slate);
    }

    private bool InRange(RoyalTitleDef title, Slate slate)
    {
        var range = seniorityRange.GetValue(slate);
        return range.max <= 0f || range.IncludesEpsilon(title.seniority);
    }

    private bool CanUseTitled(Faction faction, Slate slate)
    {
        return allowTitled.GetValue(slate) && ModsConfig.RoyaltyActive && titledPawnKind.GetValue(slate) != null && TitlesForGeneration(faction, slate).Any();
    }

    private IEnumerable<Pawn> ExistingTitled(Faction faction, Slate slate)
    {
        return Find.WorldPawns.AllPawnsAlive.Where(p => IsGoodTitled(p, faction, slate));
    }

    private IEnumerable<RoyalTitleDef> TitlesForGeneration(Faction faction, Slate slate)
    {
        var kind = titledPawnKind.GetValue(slate);
        return faction.def.RoyalTitlesAllInSeniorityOrderForReading.Where(t => t.commonality > 0f && InRange(t, slate) && (kind == null || kind.titleSelectOne.NullOrEmpty() || kind.titleSelectOne.Contains(t)));
    }

    private Pawn GenerateTitled(Faction faction, Slate slate)
    {
        if (!TitlesForGeneration(faction, slate).TryRandomElementByWeight(t => t.commonality, out var title))
        {
            return null;
        }
        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(titledPawnKind.GetValue(slate), faction, forceGenerateNewPawn: true, fixedTitle: title));
        Find.WorldPawns.PassToWorld(pawn);

        var hyperlinks = new QuestPart_Hyperlinks();
        hyperlinks.pawns.Add(pawn);
        QuestGen.quest.AddPart(hyperlinks);
        return pawn;
    }

    private static void AddInvolvedFaction(Pawn pawn)
    {
        if (pawn.Faction == null || pawn.Faction.Hidden)
        {
            return;
        }
        var involved = new QuestPart_InvolvedFactions();
        involved.factions.Add(pawn.Faction);
        QuestGen.quest.AddPart(involved);
    }
}
