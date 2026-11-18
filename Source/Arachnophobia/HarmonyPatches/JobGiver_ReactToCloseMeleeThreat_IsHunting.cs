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

        switch (pawn.jobs?.curDriver)
        {
            case JobDriver_PredatorHunt:
                return;
            case JobDriver_SpinPrey jobDriver_Hunt:
                __result = jobDriver_Hunt.Prey == prey;
                break;
        }
    }
}