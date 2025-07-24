using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberDied))]
public static class Faction_Notify_MemberDied
{
    // RimWorld.Faction
    public static bool Prefix(DamageInfo? dinfo)
    {
        return dinfo?.Instigator is not PawnWebSpinner;
    }
}