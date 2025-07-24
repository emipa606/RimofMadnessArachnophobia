using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(JobGiver_ReactToCloseMeleeThreat), "IsHunting")]
public static class JobGiver_ReactToCloseMeleeThreat_IsHunting
{
    // RimWorld.JobGiver_ReactToCloseMeleeThreat
    public static void Postfix(ref bool __result, Pawn pawn, Pawn prey)
    {
        if (pawn?.CurJob == null)
        {
            __result = false;
            return;
        }

        if (pawn.jobs?.curDriver is JobDriver_PredatorHunt)
        {
            return;
        }

        if (pawn.jobs?.curDriver is JobDriver_SpinPrey jobDriver_Hunt)
        {
            //Log.Message("Spiders GOOOO");
            __result = jobDriver_Hunt.Prey == prey;
        }
    }
}