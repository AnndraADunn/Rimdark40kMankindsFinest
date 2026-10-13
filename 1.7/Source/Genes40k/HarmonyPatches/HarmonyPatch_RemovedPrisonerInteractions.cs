using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Genes40k;

[StaticConstructorOnStartup]
public static class RemovedPrisonerInteractions
{
    private static readonly FieldInfo EnabledNonExclusiveInteractions = AccessTools.Field(typeof(Pawn_GuestTracker), "enabledNonExclusiveInteractions");

    /// <summary>
    /// Lets saves made with the removed induct-as-serf prisoner checkbox load without errors or null entries.
    /// </summary>
    static RemovedPrisonerInteractions()
    {
        if (AccessTools.Field(typeof(BackCompatibility), "RemovedDefs")?.GetValue(null) is List<Tuple<string, Type>> removedDefs)
        {
            removedDefs.Add(new Tuple<string, Type>("BEWH_InductAsSerf", typeof(PrisonerInteractionModeDef)));
        }

        if (EnabledNonExclusiveInteractions != null)
        {
            Genes40kMod.harmony.Patch(AccessTools.Method(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.ExposeData)), null, new HarmonyMethod(AccessTools.Method(typeof(RemovedPrisonerInteractions), "Postfix")));
        }
    }

    public static void Postfix(Pawn_GuestTracker __instance)
    {
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        (EnabledNonExclusiveInteractions.GetValue(__instance) as List<PrisonerInteractionModeDef>)?.RemoveAll(def => def == null);
    }
}
