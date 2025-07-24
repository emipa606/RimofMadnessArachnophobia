using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.IsFighting))]
public static class PawnUtility_IsFighting
{
// RimWorld.PawnUtility
    public static void Postfix(ref bool __result, Pawn pawn)
    {
        __result = pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.AttackMelee ||
                                           pawn.CurJob.def == JobDefOf.AttackStatic ||
                                           pawn.CurJob.def == JobDefOf.Wait_Combat ||
                                           pawn.CurJob.def == JobDefOf.PredatorHunt ||
                                           pawn.CurJob.def == ROMADefOf.ROMA_SpinPrey);
    }
}