using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arachnophobia;

public class PawnWebSpinner : Pawn
{
    private bool firstTick;

    private Thing web;
    private int webPeriod = -1;
    private int websMade;

    private int WebPeriod
    {
        get
        {
            if (webPeriod == -1)
            {
                webPeriod = (int)Mathf.Lerp(3000f, 5000f, Rand.ValueSeeded(thingIDNumber ^ 74374237));
            }

            return webPeriod;
        }
    }

    public Thing Web
    {
        get => web;
        set => web = value;
    }

    public int WebsMade
    {
        get => websMade;
        set => websMade = value;
    }

    public bool IsBusy =>
        CurJob?.def == JobDefOf.PredatorHunt ||
        CurJob?.def == JobDefOf.Ingest ||
        CurJob?.def == JobDefOf.LayDown ||
        CurJob?.def == JobDefOf.Mate ||
        CurJob?.def == JobDefOf.LayEgg ||
        CurJob?.def == JobDefOf.AttackMelee ||
        CurJob?.def == ROMADefOf.ROMA_SpinPrey ||
        CurJob?.def == ROMADefOf.ROMA_ConsumeCocoon;

    public bool IsMakingCocoon => CurJob?.def == ROMADefOf.ROMA_SpinPrey;
    private bool IsMakingWeb => CurJob?.def == ROMADefOf.ROMA_SpinWeb;

    private bool CanMakeWeb => !IsMakingWeb && !IsBusy && ageTracker.CurLifeStageIndex > 1;

    private void MakeWeb()
    {
        if (web != null || !Spawned || Downed || Dead || !CanMakeWeb)
        {
            return;
        }

        var cell = RCellFinder.RandomWanderDestFor(this, PositionHeld, 5f, null, Danger.Some);
        jobs.StartJob(new Job(ROMADefOf.ROMA_SpinWeb, cell), JobCondition.InterruptForced);
    }

    public void Notify_WebTouched(Pawn toucher)
    {
        if (web == null || !Spawned || Dead || Downed)
        {
            return;
        }

        //Our webspinners will attack prey under a few conditions.
        var hungryNow = needs?.food?.CurCategory <= HungerCategory.Hungry;
        var canPreyUpon = (toucher?.RaceProps?.canBePredatorPrey ?? false) &&
                          (float)toucher.RaceProps?.baseBodySize <= (RaceProps?.maxPreyBodySize ?? 0);
        var attackAnyway = Rand.Value > 0.95; //5% chance to attack regardless
        var friendly = toucher?.RaceProps != null && Faction != null && (toucher.Faction == Faction ||
                                                                         !toucher.RaceProps.Animal &&
                                                                         !toucher.Faction.HostileTo(Faction) &&
                                                                         !toucher.IsPrisonerOfColony);

        if (friendly || (!hungryNow || !canPreyUpon) && !attackAnyway)
        {
            return;
        }

        var spinPrey = new Job(ROMADefOf.ROMA_SpinPrey, toucher)
        {
            count = 1
        };
        jobs.StartJob(spinPrey, JobCondition.InterruptForced);
    }

    #region Overrides

    protected override void Tick()
    {
        base.Tick();
        if (Find.TickManager.TicksGame % 60 == 0 && !firstTick)
        {
            firstTick = true;
            //Are we a player animal? Great.
            if (Faction == Faction.OfPlayerSilentFail)
            {
                return;
            }

            //Are there more of us? Great.
            if (MapHeld.mapPawns.AllPawnsSpawned.FirstOrDefault(x => x != this && x.def == def) != null)
            {
                return;
            }

            //Only one? Let's fix that.
            if (def == ROMADefOf.ROMA_SpiderRace)
            {
                if (MapHeld.GetComponent<MapComponent_CocoonTracker>() is { isSpiderPair: false } tracker)
                {
                    tracker.isSpiderPair = true;
                    var newThing = PawnGenerator.GeneratePawn(kindDef, Faction);
                    var newSpinner =
                        (PawnWebSpinner)GenSpawn.Spawn((PawnWebSpinner)newThing, PositionHeld, MapHeld);
                    newSpinner.gender = gender == Gender.Male ? Gender.Female : Gender.Male;
                }
            }

            if (def == ROMADefOf.ROMA_SpiderRaceGiant)
            {
                if (MapHeld.GetComponent<MapComponent_CocoonTracker>() is { isGiantSpiderPair: false } tracker)
                {
                    tracker.isGiantSpiderPair = true;
                    var newThing = PawnGenerator.GeneratePawn(kindDef, Faction);
                    var newSpinner =
                        (PawnWebSpinner)GenSpawn.Spawn((PawnWebSpinner)newThing, PositionHeld, MapHeld);
                    newSpinner.gender = gender == Gender.Male ? Gender.Female : Gender.Male;
                }
            }
        }

        if (Find.TickManager.TicksGame % WebPeriod == 0)
        {
            //Have a spinneret? Make some web.
            if (health?.hediffSet?.GetNotMissingParts()
                    ?.FirstOrDefault(x => x.def.defName == "ROMA_Spinneret") != null)
            {
                MakeWeb();
            }
        }

        if (Find.TickManager.TicksGame % 1000 == 0)
        {
            CthulhuUtility.RemoveSanityLoss(this);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref web, "web");
        Scribe_Values.Look(ref firstTick, "firstTick");
        Scribe_Values.Look(ref websMade, "websMade");
    }

    #endregion Overrides
}