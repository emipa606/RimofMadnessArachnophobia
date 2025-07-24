using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberTookDamage))]
public static class Faction_Notify_MemberTookDamage
{
    // RimWorld.Faction
    public static void Postfix(Faction __instance, DamageInfo dinfo)
    {
        if (dinfo.Instigator is Pawn { CurJob: not null } p && p.CurJob.def == ROMADefOf.ROMA_SpinPrey)
        {
            //Log.Message("Spiders GOOO");
            AccessTools.Method(typeof(Faction), "TookDamageFromPredator").Invoke(__instance, [p]);
        }
    }
}