using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Genes40k;

/// <summary>
/// Like QuestNode_GetNearbySettlement, but only picks visitable surface settlements of one faction.
/// </summary>
public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    public SlateRef<FactionDef> factionDef;

    public SlateRef<bool> allowActiveTradeRequest = true;

    public SlateRef<float> maxTileDistance;

    [NoTranslate]
    public SlateRef<string> storeAs;

    [NoTranslate]
    public SlateRef<string> storeFactionLeaderAs;

    [NoTranslate]
    public SlateRef<string> storeCanCaravanAs;

    protected override bool TestRunInt(Slate slate)
    {
        var map = slate.Get<Map>("map");
        if (map == null)
        {
            return false;
        }
        var settlement = RandomSettlement(map.Tile, slate);
        if (settlement == null)
        {
            return false;
        }
        Store(slate, settlement, map);
        return true;
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        var map = slate.Get<Map>("map");
        var settlement = RandomSettlement(map.Tile, slate);
        if (settlement == null)
        {
            return;
        }
        Store(slate, settlement, map);
    }

    private void Store(Slate slate, Settlement settlement, Map map)
    {
        slate.Set(storeAs.GetValue(slate), settlement);
        if (!storeFactionLeaderAs.GetValue(slate).NullOrEmpty())
        {
            slate.Set(storeFactionLeaderAs.GetValue(slate), settlement.Faction.leader);
        }
        if (!storeCanCaravanAs.GetValue(slate).NullOrEmpty())
        {
            var canCaravan = settlement.Tile.Valid && map.Tile.Valid && settlement.Tile.Layer == map.Tile.Layer && settlement.Tile.LayerDef.SurfaceTiles;
            slate.Set(storeCanCaravanAs.GetValue(slate), canCaravan);
        }
    }

    private Settlement RandomSettlement(PlanetTile originTile, Slate slate)
    {
        var def = factionDef.GetValue(slate);
        if (def == null)
        {
            return null;
        }
        return Find.WorldObjects.SettlementBases.Where(s => IsValid(s, def, originTile, slate)).RandomElementWithFallback();
    }

    private bool IsValid(Settlement settlement, FactionDef def, PlanetTile originTile, Slate slate)
    {
        if (settlement.Faction == null || settlement.Faction.def != def || settlement.Faction.leader == null || !settlement.Visitable)
        {
            return false;
        }
        if (!allowActiveTradeRequest.GetValue(slate) && HasTradeRequest(settlement))
        {
            return false;
        }
        if (GravshipUtility.PlayerHasGravEngine())
        {
            return true;
        }
        return Find.WorldReachability.CanReach(originTile, settlement.Tile) && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate);
    }

    private static bool HasTradeRequest(Settlement settlement)
    {
        var comp = settlement.GetComponent<TradeRequestComp>();
        if (comp != null && comp.ActiveRequest)
        {
            return true;
        }
        foreach (var quest in Find.QuestManager.QuestsListForReading)
        {
            if (quest.Historical)
            {
                continue;
            }
            if (quest.PartsListForReading.Any(p => p is QuestPart_InitiateTradeRequest part && part.settlement == settlement))
            {
                return true;
            }
        }
        return false;
    }
}
