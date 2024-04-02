using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmony = new Harmony("rimworld.jecrell.arachnophobia");
        harmony.Patch(AccessTools.Method(typeof(Pawn_MindState), "StartManhunterBecauseOfPawnAction"),
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(StartManhunterBecauseOfPawnAction_PreFix))));
        harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.Notify_MemberDied)),
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Notify_MemberDied_Prefix))));
        harmony.Patch(AccessTools.Method(typeof(CompEggLayer), nameof(CompEggLayer.ProduceEgg)), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(ProduceEgg_PostFix))));
        harmony.Patch(AccessTools.Method(typeof(JobGiver_ReactToCloseMeleeThreat), "IsHunting"), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(IsHunting_PostFix))));
        harmony.Patch(AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.IsFighting)), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(IsFighting_PostFix))));
        harmony.Patch(AccessTools.Method(typeof(GenHostility), "GetPreyOfMyFaction"), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(GetPreyOfMyFaction_PostFix))));
        harmony.Patch(AccessTools.Method(typeof(Faction), nameof(Faction.Notify_MemberTookDamage)), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Notify_MemberTookDamage_PostFix))));
        harmony.Patch(AccessTools.Method(typeof(Pawn_MindState), "CanStartFleeingBecauseOfPawnAction"), null,
            new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CanStartFleeingBecauseOfPawnAction))));
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    //MindState
    public static void CanStartFleeingBecauseOfPawnAction(Pawn p, ref bool __result)
    {
        if (p.ParentHolder is Building_Cocoon)
        {
            __result = false;
        }
    }

    // RimWorld.Faction
    public static void Notify_MemberTookDamage_PostFix(Faction __instance, Pawn member, DamageInfo dinfo)
    {
        if (dinfo.Instigator is Pawn { CurJob: not null } p && p.CurJob.def == ROMADefOf.ROMA_SpinPrey)
        {
            //Log.Message("Spiders GOOO");
            AccessTools.Method(typeof(Faction), "TookDamageFromPredator").Invoke(__instance, [p]);
        }
    }


    // RimWorld.GenHostility
    public static void GetPreyOfMyFaction_PostFix(ref Pawn __result, Pawn predator, Faction myFaction)
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


    // RimWorld.PawnUtility
    public static void IsFighting_PostFix(ref bool __result, Pawn pawn)
    {
        __result = pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.AttackMelee ||
                                           pawn.CurJob.def == JobDefOf.AttackStatic ||
                                           pawn.CurJob.def == JobDefOf.Wait_Combat ||
                                           pawn.CurJob.def == JobDefOf.PredatorHunt ||
                                           pawn.CurJob.def == ROMADefOf.ROMA_SpinPrey);
    }


    // RimWorld.JobGiver_ReactToCloseMeleeThreat
    public static void IsHunting_PostFix(ref bool __result, Pawn pawn, Pawn prey)
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


    // RimWorld.CompEggLayer
    public static void ProduceEgg_PostFix(CompEggLayer __instance, ref Thing __result)
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


    // RimWorld.Faction
    public static bool Notify_MemberDied_Prefix(Faction __instance, Pawn member, DamageInfo? dinfo,
        bool wasWorldPawn)
    {
        return dinfo?.Instigator is not PawnWebSpinner;
    }

    // Verse.AI.Pawn_MindState
    public static bool StartManhunterBecauseOfPawnAction_PreFix(Pawn_MindState __instance)
    {
        return __instance.pawn is not PawnWebSpinner p || p.CurJob?.def != ROMADefOf.ROMA_SpinPrey;
    }
}