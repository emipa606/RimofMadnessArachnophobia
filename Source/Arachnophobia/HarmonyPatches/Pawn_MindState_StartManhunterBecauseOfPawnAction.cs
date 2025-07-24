using HarmonyLib;
using Verse.AI;

namespace Arachnophobia;

[HarmonyPatch(typeof(Pawn_MindState), "StartManhunterBecauseOfPawnAction")]
public static class Pawn_MindState_StartManhunterBecauseOfPawnAction
{
    // Verse.AI.Pawn_MindState
    public static bool Prefix(Pawn_MindState __instance)
    {
        return __instance.pawn is not PawnWebSpinner p || p.CurJob?.def != ROMADefOf.ROMA_SpinPrey;
    }
}