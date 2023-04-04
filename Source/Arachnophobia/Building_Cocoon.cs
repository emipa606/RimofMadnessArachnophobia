using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia;

public class Building_Cocoon : Building_Casket
{
    private readonly SoundDef sound = SoundDef.Named("HissSmall");
    private Graphic cachedGraphicEmpty;

    // RimWorld.Building_Grave code repurposed for Cocoons
    private Graphic cachedGraphicFull;
    private PawnWebSpinner currentDrinker;

    public int lastEscapeAttempt;

    private IntVec3 nextValidPlacementSpot;
    private bool resolvingCurrently;
    private PawnWebSpinner spinner;

    public PawnWebSpinner CurrentDrinker
    {
        get
        {
            if (currentDrinker == null)
            {
                return currentDrinker;
            }

            if (!currentDrinker.Spawned || currentDrinker.Downed || currentDrinker.Dead ||
                currentDrinker?.CurJob?.def != ROMADefOf.ROMA_ConsumeCocoon)
            {
                currentDrinker = null;
            }

            return currentDrinker;
        }
        set => currentDrinker = value;
    }

    public PawnWebSpinner Spinner
    {
        get => spinner;
        set => spinner = value;
    }

    public Pawn Victim
    {
        get
        {
            Pawn result = null;
            if (innerContainer.Count <= 0)
            {
                return null;
            }

            if (innerContainer[0] is Pawn p)
            {
                result = p;
            }

            if (innerContainer[0] is Corpse y)
            {
                result = y.InnerPawn;
            }

            return result;
        }
    }

    public IntVec3 NextValidPlacementSpot
    {
        get
        {
            if (nextValidPlacementSpot != default && nextValidPlacementSpot != IntVec3.Invalid)
            {
                return nextValidPlacementSpot;
            }

            var cocoonsToUpdate = new HashSet<Building_Cocoon>();
            var cells = GenAdj.CellsAdjacent8Way(new TargetInfo(PositionHeld, MapHeld));
            foreach (var cell in cells)
            {
                if (!cell.Walkable(MapHeld))
                {
                    continue;
                }

                if (cell.GetThingList(Map).FirstOrDefault(x => x is Building_Cocoon) is Building_Cocoon c)
                {
                    cocoonsToUpdate.Add(c);
                }
                else if (cell.GetThingList(Map).FirstOrDefault(x => x is Building_Cocoon) == null)
                {
                    if (!GenConstruct.CanPlaceBlueprintAt(def, cell, Rot4.North, Map).Accepted)
                    {
                        continue;
                    }

                    nextValidPlacementSpot = cell;
                    break;
                }
            }

            foreach (var c in cocoonsToUpdate)
            {
                if (!c.resolvingCurrently)
                {
                    c.ResolvedNeighborPos();
                }
            }

            return nextValidPlacementSpot;
        }
    }

    public override Graphic Graphic
    {
        get
        {
            if (def.building.fullGraveGraphicData == null)
            {
                return base.Graphic;
            }

            if (Victim == null)
            {
                if (cachedGraphicEmpty == null)
                {
                    cachedGraphicEmpty = GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath,
                        ShaderDatabase.Cutout, def.graphicData.drawSize, DrawColor, DrawColorTwo, def.graphicData);
                }

                return cachedGraphicEmpty;
            }

            if (cachedGraphicFull == null)
            {
                cachedGraphicFull = GraphicDatabase.Get<Graphic_Single>(def.building.fullGraveGraphicData.texPath,
                    ShaderDatabase.Cutout, def.building.fullGraveGraphicData.drawSize, DrawColor, DrawColorTwo,
                    def.building.fullGraveGraphicData);
            }

            return cachedGraphicFull;
        }
    }

    public bool isConsumable =>
        Victim != null &&
        //Victim.IngestibleNow &&
        Spawned &&
        !Destroyed &&
        !MapHeld.physicalInteractionReservationManager.IsReserved(this);

    public bool isPathableBy(Pawn p)
    {
        using var pawnPath = p.Map.pathFinder.FindPath(p.Position, Position,
            TraverseParms.For(p, Danger.Deadly, TraverseMode.PassDoors));
        return pawnPath.Found;
    }

    public bool isConsumableBy(Pawn pawn)
    {
        return pawn is PawnWebSpinner webSpinner &&
               Spawned &&
               !this.IsForbidden(pawn) &&
               webSpinner.Spawned &&
               !webSpinner.Dead &&
               !webSpinner.IsBusy &&
               webSpinner.needs?.food?.CurLevelPercentage <= 0.4 &&
               isConsumable &&
               playerFactionExceptions(webSpinner) &&
               isPathableBy(pawn) &&
               pawn.CanReserve(this) &&
               pawn.MapHeld.reservationManager.CanReserve(pawn, this) &&
               !pawn.MapHeld.physicalInteractionReservationManager.IsReserved(this) &&
               CurrentDrinker == null;
    }


    /// <summary>
    ///     Finds a new position for a potential neighboring cocoon.
    /// </summary>
    public void ResolvedNeighborPos()
    {
        resolvingCurrently = true;
        var result = NextValidPlacementSpot;
        if (!result.Walkable(MapHeld))
        {
            nextValidPlacementSpot = default;
            result = NextValidPlacementSpot;
            for (var i = 0; i < 9; i++)
            {
                if (result.GetThingList(Map).FirstOrDefault(x => x is Building_Cocoon) != null)
                {
                    nextValidPlacementSpot = default;
                    result = NextValidPlacementSpot;
                    if (!GenConstruct.CanPlaceBlueprintAt(def, result, Rot4.North, Map).Accepted)
                    {
                        nextValidPlacementSpot = default;
                        result = NextValidPlacementSpot;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        resolvingCurrently = false;
    }

    //Wild spiders || Faction spiders && player spiders must have access to cocoons
    public bool playerFactionExceptions(PawnWebSpinner y)
    {
        return y?.Faction == null ||
               Victim?.Faction != y.Faction && y.Faction == Faction.OfPlayerSilentFail &&
               PositionHeld.InAllowedArea(y);
    }

    /// <summary>
    ///     Adds this cocoon to the lists in the MapComponent
    /// </summary>
    /// <param name="placer"></param>
    public void Notify_Placed(Pawn placer)
    {
        if (Map.GetComponent<MapComponent_CocoonTracker>() is { } cocoons)
        {
            if (Faction == Faction.OfPlayer || placer?.Faction == Faction.OfPlayer)
            {
                cocoons.DomesticCocoons.Add(this);
            }
            else
            {
                cocoons.WildCocoons.Add(this);
            }
        }

        ResolvedNeighborPos();
    }

    /// <summary>
    ///     Removes this cocoon to the lists in the MapComponent
    /// </summary>
    /// <param name="placer"></param>
    public void Notify_Removed(Pawn placer)
    {
        if (Map.GetComponent<MapComponent_CocoonTracker>() is { } cocoons)
        {
            if (Faction == Faction.OfPlayer || placer?.Faction == Faction.OfPlayer)
            {
                if (cocoons.DomesticCocoons.Contains(this))
                {
                    cocoons.DomesticCocoons.Remove(this);
                }
            }
            else
            {
                if (cocoons.WildCocoons.Contains(this))
                {
                    cocoons.WildCocoons.Remove(this);
                }
            }
        }

        nextValidPlacementSpot = default;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Notify_Placed(null);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Notify_Removed(null);
        base.DeSpawn(mode);
    }

    public override string GetInspectString()
    {
        var str = innerContainer.ContentsString;
        var str2 = "None".Translate();
        var compDisappearsStr = GetComp<CompLifespan>()?.CompInspectStringExtra()?.TrimEndNewlines() ?? "";
        var result = new StringBuilder();

        if (Spinner != null)
        {
            str2 = spinner.Faction != Faction.OfPlayerSilentFail
                ? "ROM_Wild".Translate(spinner.Label).RawText
                : spinner.Name.ToStringFull;
            if (spinner.Dead)
            {
                str2 = "DeadLabel".Translate(str2);
            }
        }

        result.AppendLine("ROM_Spinner".Translate() + ": " + str2);

        result.AppendLine("CasketContains".Translate() + ": " + str.CapitalizeFirst());
        result.AppendLine(compDisappearsStr);

        return result.ToString().TrimEndNewlines();
    }

    public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!base.TryAcceptThing(thing, allowSpecialEffects))
        {
            return false;
        }

        if (allowSpecialEffects)
        {
            //sound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
        }

        return true;
    }

    public override void Tick()
    {
        base.Tick();
        if (Find.TickManager.TicksGame % 250 != 0)
        {
            return;
        }

        var nonSpinnerCarrier =
            ParentHolder is Pawn_CarryTracker { pawn: { } cp and not PawnWebSpinner } &&
            cp.Faction != Spinner.Faction
                ? cp
                : null;
        var isSpinnerAvailable = Spinner is { Spawned: true, IsBusy: false } &&
                                 Spinner.Map == nonSpinnerCarrier?.MapHeld;
        if (isSpinnerAvailable)
        {
            var isInSpinnerLOS = nonSpinnerCarrier != null && GenSight.LineOfSight(Spinner.Position,
                nonSpinnerCarrier.Position,
                nonSpinnerCarrier.Map);
            if (nonSpinnerCarrier != null && isInSpinnerLOS)
            {
                var attackJob = new Job(JobDefOf.AttackMelee, nonSpinnerCarrier)
                {
                    count = 1,
                    killIncappedTarget = false
                };
                Spinner.jobs.TryTakeOrderedJob(attackJob);
            }
        }

        if (lastEscapeAttempt == 0)
        {
            lastEscapeAttempt = Find.TickManager.TicksGame;
        }

        if (Victim is not { } p || p.Dead || p.Faction != Faction.OfPlayerSilentFail ||
            lastEscapeAttempt + GenDate.TicksPerHour <= Find.TickManager.TicksGame)
        {
            return;
        }

        lastEscapeAttempt = Find.TickManager.TicksGame;
        if (!(Rand.Value > 0.95f) || Destroyed)
        {
            return;
        }

        Messages.Message("ROM_EscapedFromCocoon".Translate(p), MessageTypeDefOf.NeutralEvent);
        EjectContents();
    }

    // RimWorld.Building_Casket
    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
        {
            if (mode != DestroyMode.Deconstruct)
            {
                var list = new List<Pawn>();
                foreach (var current in innerContainer)
                {
                    if (current is Pawn pawn)
                    {
                        list.Add(pawn);
                    }
                }

                foreach (var current2 in list)
                {
                    HealthUtility.DamageUntilDowned(current2);
                }
            }

            EjectContents();
        }

        innerContainer.ClearAndDestroyContents();
        if (!Destroyed)
        {
            base.Destroy(mode);
        }
    }


    public override void EjectContents()
    {
        var filthCobwebs = ROMADefOf.ROM_FilthCobwebs;
        foreach (var current in innerContainer)
        {
            if (current is not Pawn pawn)
            {
                continue;
            }

            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            pawn.filth.GainFilth(filthCobwebs);
            //pawn.health.AddHediff(HediffDefOf.ToxicBuildup, null, null);
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, 0.3f);
        }

        if (MapHeld != null)
        {
            innerContainer.TryDropAll(PositionHeld, MapHeld, ThingPlaceMode.Near);
        }

        contentsKnown = true;
        if (!Destroyed)
        {
            Destroy(DestroyMode.KillFinalize);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref spinner, "spinner");
        Scribe_References.Look(ref currentDrinker, "currentDrinker");
        Scribe_Values.Look(ref nextValidPlacementSpot, "nextValidPlacementSpot");
    }
}