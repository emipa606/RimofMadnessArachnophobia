using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia;

[HarmonyPatch(typeof(GenHostility), "GetPreyOfMyFaction")]
public static class GenHostility_GetPreyOfMyFaction
{
    // RimWorld.GenHostility
    public static void Postfix(ref Pawn __result, Pawn predator, Faction myFaction)
    {
        var curJob = predator.CurJob;
        if (curJob == null || curJob.def != ROMADefOf.ROMA_SpinPrey || predator.jobs.curDriver.ended)
        {
            return;
        }

        if (curJob.GetTarget(TargetIndex.A).Thing is Pawn pawn && pawn.Faction == myFaction)
        {
            //Log.Message("Spiders GOOOO");
            __result = pawn;
        }
    }
}