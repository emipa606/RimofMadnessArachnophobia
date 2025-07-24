using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(CompEggLayer), nameof(CompEggLayer.ProduceEgg))]
public static class CompEggLayer_ProduceEgg
{
    // RimWorld.CompEggLayer
    public static void Postfix(CompEggLayer __instance, ref Thing __result)
    {
        var compHatcher = __result.TryGetComp<CompMultiHatcher>();
        if (compHatcher == null)
        {
            return;
        }

        compHatcher.hatcheeFaction = __instance.parent.Faction;
        if (__instance.parent is Pawn pawn)
        {
            compHatcher.hatcheeParent = pawn;
        }

        if (Traverse.Create(__instance).Field("fertilizedBy").GetValue<Pawn>() is { } Fertilizer)
        {
            compHatcher.otherParent = Fertilizer;
        }
    }
}