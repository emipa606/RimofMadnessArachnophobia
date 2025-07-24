using HarmonyLib;
using Verse;
using Verse.AI;

namespace Arachnophobia;

[HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.StartFleeingBecauseOfPawnAction))]
public static class Pawn_MindState_StartFleeingBecauseOfPawnAction
{
    //MindState
    public static bool Prefix(Thing instigator)
    {
        return instigator is not Pawn { ParentHolder: Building_Cocoon };
    }
}