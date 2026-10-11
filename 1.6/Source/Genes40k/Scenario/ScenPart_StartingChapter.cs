using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Genes40k;

/// <summary>
/// Places the starting Space Marines, and the gene-seed vials they carry, in one of the legion chapters. Either fixed in the
/// scenario or picked by the player when the game starts.
/// </summary>
public class ScenPart_StartingChapter : ScenPart
{
    private GeneDef chapterGene;

    private static IEnumerable<GeneDef> ChapterGenes => ChapterChoiceUtility.ChapterMaterials
        .Select(material => material.GetModExtension<DefModExtension_GeneFromMaterial>().addedGene);

    private static string ChapterLabel(GeneDef gene)
    {
        var label = CustomChapterGeneUtility.ChapterLabelOf(gene);
        return label.NullOrEmpty() ? gene.LabelCap.ToString() : label.CapitalizeFirst();
    }

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        var buttonLabel = chapterGene == null ? "BEWH.MankindsFinest.Scenario.ChooseAtStart".Translate().ToString() : ChapterLabel(chapterGene);

        if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), buttonLabel))
        {
            return;
        }

        var options = new List<FloatMenuOption>
        {
            new("BEWH.MankindsFinest.Scenario.ChooseAtStart".Translate(), () => chapterGene = null)
        };

        foreach (var gene in ChapterGenes)
        {
            var localGene = gene;
            options.Add(new FloatMenuOption(ChapterLabel(localGene), () => chapterGene = localGene));
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    public override string Summary(Scenario scen)
    {
        return chapterGene == null
            ? "BEWH.MankindsFinest.Scenario.StartingChapterAsk".Translate()
            : "BEWH.MankindsFinest.Scenario.StartingChapterFixed".Translate(ChapterLabel(chapterGene));
    }

    public override void PostGameStart()
    {
        if (chapterGene != null)
        {
            ApplyChapter(chapterGene);
            return;
        }

        var choices = ChapterChoiceUtility.ChapterMaterials.Select(material => new ChapterChoice
        {
            chapterGene = material.GetModExtension<DefModExtension_GeneFromMaterial>().addedGene,
            chapterMaterial = material
        }).ToList();

        if (choices.NullOrEmpty())
        {
            return;
        }

        Find.WindowStack.Add(new Dialog_SelectChapter(
            choices,
            "BEWH.MankindsFinest.Scenario.SelectStartingChapter".Translate(),
            "BEWH.MankindsFinest.Scenario.SelectStartingChapterDesc".Translate(),
            choice => ApplyChapter(choice.chapterGene),
            "BEWH.MankindsFinest.Scenario.RemainChapterless".Translate()));
    }

    /// <summary>
    /// Gives every chapterless Firstborn of the player the chapter gene and the chapter's armour colours, marks the chapter's
    /// legion material as known and assigns the chapter to the unassigned gene-seed vials those marines carry.
    /// </summary>
    public static void ApplyChapter(GeneDef gene)
    {
        if (gene == null)
        {
            return;
        }

        var material = ChapterChoiceUtility.ChapterMaterials.FirstOrDefault(def => def.GetModExtension<DefModExtension_GeneFromMaterial>().addedGene == gene);
        var unlockedMaterials = GameComponent_UnlockedMaterials.Instance;
        var chapterColours = Genes40kUtils.ChapterColourFor(gene);

        if (material != null && unlockedMaterials != null && !unlockedMaterials.HasMaterial(material))
        {
            unlockedMaterials.UnlockMaterial(material);
        }

        foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction.ToList())
        {
            if (pawn.genes == null || !pawn.IsFirstborn())
            {
                continue;
            }

            if (pawn.genes.GenesListForReading.Any(existing => existing.def.HasModExtension<DefModExtension_ChapterGene>()))
            {
                continue;
            }

            CustomChapterGeneUtility.AddChapterGene(pawn, gene, true, false);
            Genes40kUtils.ApplyChapterColoursToApparel(pawn, chapterColours);

            if (pawn.inventory?.innerContainer == null)
            {
                continue;
            }

            foreach (var vial in pawn.inventory.innerContainer.OfType<GeneseedVial>())
            {
                vial.extraGeneFromMaterial ??= gene;
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref chapterGene, "chapterGene");
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ (chapterGene?.GetHashCode() ?? 0);
    }
}
