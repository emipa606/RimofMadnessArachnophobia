using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia
{
    public class JobDriver_SpinPrey : JobDriver
    {
        private string currentActivity = "";

        private bool firstHit = true;
        private bool notifiedPlayer;

        public ThingDef CocoonDef
        {
            get
            {
                var result = ROMADefOf.ROMA_Cocoon;
                if ((Prey?.RaceProps?.baseBodySize ?? 0) > 0.99)
                {
                    result = ROMADefOf.ROMA_CocoonGiant;
                }

                return result;
            }
        }

        public Pawn Prey
        {
            get
            {
                var corpse = Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }

                return (Pawn) job.GetTarget(TargetIndex.A).Thing;
            }
        }

        private Corpse Corpse => job.GetTarget(TargetIndex.A).Thing as Corpse;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref firstHit, "firstHit");
        }

        public override string GetReport()
        {
            if (currentActivity == "")
            {
                currentActivity = base.ReportStringProcessed(JobDefOf.Hunt.reportString);
            }

            return currentActivity;
        }

        public IntVec3 CocoonPlace(Building_Cocoon exception = null)
        {
            var newPosition = TargetB.Cell;
            var localCocoons = Utility.CocoonsFor(pawn.Map, pawn);
            if (exception != null)
            {
                localCocoons.Remove(exception);
            }

            //Log.Message("1");
            if (localCocoons == null || localCocoons.Count <= 0)
            {
                return newPosition;
            }
            //Log.Message("2");

            var tempCocoons = new HashSet<Thing>(localCocoons.InRandomOrder());
            foreach (var thing in tempCocoons)
            {
                var cocoon = (Building_Cocoon) thing;
                //Log.Message("3");
                if (cocoon.NextValidPlacementSpot is { } vec && vec != default)
                {
                    newPosition = vec;
                }
            }

            return newPosition;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            /*
             * 
             *  Toil Configurations
             *
             */
            var prepareToSpin = new Toil
            {
                initAction = delegate
                {
                    if (Prey == null && Corpse == null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    if (Prey != null && Prey.Dead)
                    {
                        pawn.CurJob.SetTarget(TargetIndex.A, Prey.Corpse);
                    }

                    if (!pawn.CanReserveAndReach(TargetA, PathEndMode.Touch, Danger.None))
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
            };

            var gotoBody = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            gotoBody.AddPreInitAction(delegate
            {
                pawn.ClearAllReservations();
                pawn.Reserve(TargetA, job);
                //this.Map.physicalInteractionReservationManager.ReleaseAllForTarget(TargetA);
                //this.Map.physicalInteractionReservationManager.Reserve(this.GetActor(), TargetA);
                currentActivity = "ROM_SpinPreyJob1".Translate(TargetA.Thing?.LabelShort ?? "");
            });

            var spinDelay = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 500,
                initAction = delegate { currentActivity = "ROM_SpinPreyJob2".Translate(); }
            };
            spinDelay.WithProgressBarToilDelay(TargetIndex.B);

            var spinBody = new Toil
            {
                initAction = delegate
                {
                    //Log.Message("5");
                    if (GetActor() is not PawnWebSpinner spinner)
                    {
                        return;
                    }

                    Thing toLoad;
                    IntVec3 newPosition;
                    if (Prey.Dead)
                    {
                        toLoad = Prey.Corpse;
                        newPosition = Prey.Corpse.Position;
                    }
                    else
                    {
                        toLoad = Prey;
                        newPosition = Prey.Position;

                        //Tend prey's wounds by luck
                        if (!Prey.health.HasHediffsNeedingTend())
                        {
                            return;
                        }

                        var unused = ThingDefOf.MedicineHerbal;
                        var quality = 0.5f; //Not a doctor or attentive to human needs
                        var hediffsToTend = new List<Hediff>();
                        var hediffs = Prey.health.hediffSet.hediffs;
                        foreach (var hediff in hediffs)
                        {
                            if (hediff.TendableNow())
                            {
                                hediffsToTend.Add(hediff);
                            }
                        }

                        for (var i = 0; i < hediffsToTend.Count; i++)
                        {
                            hediffsToTend[i].Tended(quality, i);
                        }
                    }

                    if (!toLoad.Spawned)
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }

                    toLoad.DeSpawn();
                    toLoad.holdingOwner = null;
                    if (!GenConstruct.CanPlaceBlueprintAt(CocoonDef, newPosition, Rot4.North, pawn.Map).Accepted)
                    {
                        var cells = GenAdj.CellsAdjacent8Way(new TargetInfo(newPosition, pawn.Map));
                        foreach (var cell in cells)
                        {
                            if (!GenConstruct.CanPlaceBlueprintAt(CocoonDef, cell, Rot4.North, Map).Accepted)
                            {
                                continue;
                            }

                            newPosition = cell;
                            break;
                        }
                    }

                    var newCocoon = (Building_Cocoon) GenSpawn.Spawn(CocoonDef, newPosition, spinner.Map);
                    newCocoon.Spinner = spinner;
                    if (spinner.Faction == Faction.OfPlayerSilentFail)
                    {
                        newCocoon.SetFactionDirect(Faction.OfPlayerSilentFail);
                    }

                    //Log.Message("New Spinner: " + newCocoon.Spinner.Label);
                    newCocoon.TryGetInnerInteractableThingOwner().TryAdd(toLoad);
                    pawn?.CurJob?.SetTarget(TargetIndex.B, newCocoon);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            var pickupCocoon = Toils_Haul.StartCarryThing(TargetIndex.B);
            pickupCocoon.AddPreInitAction(delegate
            {
                //this.TargetB.Thing.DeSpawn();
                pawn.CurJob.SetTarget(TargetIndex.C, CocoonPlace((Building_Cocoon) TargetB.Thing));
                pawn.Reserve(TargetC, job);
                //this.pawn.Map.physicalInteractionReservationManager.Reserve(this.pawn, TargetC);
            });
            var relocateCocoon = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            var dropCocoon = Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, relocateCocoon, false).FailOn(() =>
                !GenConstruct.CanPlaceBlueprintAt(CocoonDef, TargetC.Cell, Rot4.North, Map).Accepted);
            AddFinishAction(delegate { pawn.Map.physicalInteractionReservationManager.ReleaseAllClaimedBy(pawn); });


            /*
             * 
             *  Toil Execution
             *
             */

            yield return new Toil
            {
                initAction = delegate { Map.attackTargetsCache.UpdateTarget(pawn); },
                atomicWithPrevious = true,
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            void onHitAction()
            {
                var prey = Prey;
                var surpriseAttack = firstHit && !prey.IsColonist;
                if (pawn.meleeVerbs.TryMeleeAttack(prey, job.verbToUse, surpriseAttack))
                {
                    if (!notifiedPlayer && PawnUtility.ShouldSendNotificationAbout(prey))
                    {
                        notifiedPlayer = true;
                        Messages.Message(
                            "MessageAttackedByPredator".Translate(prey.LabelShort, pawn.LabelIndefinite(),
                                    prey.Named("PREY"), pawn.Named("PREDATOR"))
                                .CapitalizeFirst(), prey, MessageTypeDefOf.ThreatBig); // MessageSound.SeriousAlert);
                    }

                    Map.attackTargetsCache.UpdateTarget(pawn);
                }

                firstHit = false;
            }

            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, onHitAction)
                .JumpIf(() => Prey.Downed || Prey.Dead, prepareToSpin).FailOn(() =>
                    Find.TickManager.TicksGame > startTick + 5000 &&
                    (float) (job.GetTarget(TargetIndex.A).Cell - pawn.Position).LengthHorizontalSquared > 4f);
            yield return prepareToSpin.FailOn(() => Prey == null);
            yield return gotoBody.FailOn(() => Prey == null);
            yield return spinDelay.FailOn(() => Prey == null);
            yield return spinBody.FailOn(() => Prey == null);
            yield return pickupCocoon;
            yield return relocateCocoon;
            yield return dropCocoon;

            //float durationMultiplier = 1f / this.pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            //yield return Toils_Ingest.ChewIngestible(this.pawn, durationMultiplier, TargetIndex.A, TargetIndex.None).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            //yield return Toils_Ingest.FinalizeIngest(this.pawn, TargetIndex.A);
            //yield return Toils_Jump.JumpIf(gotoCorpse, () => this.pawn.needs.food.CurLevelPercentage < 0.9f);
        }

        public override bool TryMakePreToilReservations(bool showResult)
        {
            return true;
        }
    }
}