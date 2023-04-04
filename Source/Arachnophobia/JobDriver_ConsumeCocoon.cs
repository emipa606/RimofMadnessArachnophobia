using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Arachnophobia;

public class JobDriver_ConsumeCocoon : JobDriver
{
    public const TargetIndex PreyInd = TargetIndex.A;

    private const TargetIndex CorpseInd = TargetIndex.A;

    private bool notifiedPlayer;

    public string report = "";

    private int ticksLeft;

    public Building_Cocoon Cocoon
    {
        get
        {
            Building_Cocoon result = null;
            if (TargetA.Thing is Building_Cocoon cocoon)
            {
                result = cocoon;
            }

            return result;
        }
    }

    public Pawn Victim
    {
        get
        {
            Pawn result = null;
            if (Cocoon?.Victim is { } p)
            {
                result = p;
            }

            return result;
        }
    }

    public override string GetReport()
    {
        if (report == "")
        {
            report = base.ReportStringProcessed(JobDefOf.Ingest.reportString);
        }

        return report;
    }


    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        //Toil definitions

        AddFinishAction(delegate
        {
            if (Cocoon != null)
            {
                Cocoon.CurrentDrinker = null;
            }
        });

        var gotoBody = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        gotoBody.AddPreInitAction(delegate
        {
            Cocoon.CurrentDrinker = pawn as PawnWebSpinner;

            if (Victim is not null && (Victim.Faction != Faction.OfPlayerSilentFail || Victim.Dead || notifiedPlayer))
            {
                return;
            }

            notifiedPlayer = true;
            var sound = Victim?.Dead ?? false ? MessageTypeDefOf.NeutralEvent : MessageTypeDefOf.ThreatBig;
            Messages.Message("ROM_SpiderEatingColonist".Translate(pawn.Label, Victim?.Label),
                sound);
        });

        //Toil executions
        yield return gotoBody;
        yield return Liquify();
        var durationMultiplier = 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
        yield return DrinkCorpse(durationMultiplier);
        yield return Toils_Ingest.FinalizeIngest(pawn, TargetIndex.B);
        yield return Toils_Jump.JumpIf(gotoBody, () => pawn.needs.food.CurLevelPercentage < 0.9f)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
    }

    public Toil Liquify()
    {
        //LIQUIFY - Burn all the victim's innards
        var liquify = new Toil
        {
            initAction = delegate
            {
                ticksLeft = Rand.Range(300, 900);
                pawn.rotationTracker.FaceCell(Cocoon.Position);
                job.SetTarget(TargetIndex.B, Victim.Corpse);
                notifiedPlayer = false;
                report = "ROM_ConsumeJob1".Translate();
            },
            tickAction = delegate
            {
                if (ticksLeft % 150 == 149)
                {
                    if (!Victim.Dead)
                    {
                        FilthMaker.TryMakeFilth(pawn.CurJob.targetA.Cell, Map, ThingDefOf.Filth_Slime,
                            pawn.LabelIndefinite());
                        var damageInt = (int)(2.5f * pawn.RaceProps.baseBodySize);
                        var damageToxic = (int)(25f * pawn.RaceProps.baseBodySize);
                        for (var i = 0; i < 2; i++)
                        {
                            var randomInternalOrgan = Victim.health?.hediffSet?.GetNotMissingParts()
                                .InRandomOrder().FirstOrDefault(x => x.depth == BodyPartDepth.Inside);
                            if (!Victim.Destroyed || !Victim.Dead)
                            {
                                Victim.TakeDamage(new DamageInfo(DamageDefOf.Burn,
                                    Rand.Range(damageInt, damageInt * 2), 1f, -1, pawn, randomInternalOrgan));
                            }

                            if (!Victim.Destroyed || !Victim.Dead)
                            {
                                Victim.TakeDamage(new DamageInfo(ROMADefOf.ToxicBite,
                                    Rand.Range(damageToxic, damageToxic * 2), 1f, -1, pawn, randomInternalOrgan));
                            }
                        }
                    }
                    else
                    {
                        report = "ROM_ConsumeJob2".Translate();
                    }
                }

                ticksLeft--;
                if (ticksLeft > 0)
                {
                    return;
                }

                if (Victim.Dead || Victim.RaceProps.IsMechanoid)
                {
                    if (Victim.Dead)
                    {
                        job.SetTarget(TargetIndex.B, Victim.Corpse);
                    }

                    ReadyForNextToil();
                }
                else
                {
                    ticksLeft = Rand.Range(300, 900);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never
        };
        liquify.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
        liquify.PlaySustainerOrSound(() => ROMADefOf.ROM_MeltingHiss);
        liquify.WithProgressBar(TargetIndex.A, delegate
        {
            var thing = pawn?.CurJob?.GetTarget(TargetIndex.B).Thing;
            if (thing == null)
            {
                return 1f;
            }

            return 1f - (pawn.jobs.curDriver.ticksLeftThisToil /
                         Mathf.Round(thing.def.ingestible.baseIngestTicks));
        });
        AddIngestionEffects(liquify, pawn, TargetIndex.B, TargetIndex.A);
        return liquify;
    }

    public void AddIngestionEffects(Toil toil, Pawn chewer, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd)
    {
        //Log.Message("3");
        if (Victim == null)
        {
            EndJobWith(JobCondition.Incompletable);
        }

        toil.WithEffect(delegate
        {
            var target = toil.actor.CurJob.GetTarget(ingestibleInd);
            if (!target.HasThing)
            {
                return null;
            }

            var result = target.Thing.def.ingestible.ingestEffect;
            if (chewer.RaceProps.intelligence < Intelligence.ToolUser &&
                target.Thing.def.ingestible.ingestEffectEat != null)
            {
                result = target.Thing?.def?.ingestible?.ingestEffectEat;
            }

            return result;
        }, delegate
        {
            if (!toil.actor.CurJob.GetTarget(ingestibleInd).HasThing)
            {
                return null;
            }

            var thing = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
            if (chewer != toil.actor)
            {
                return chewer;
            }

            if (eatSurfaceInd != TargetIndex.None && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
            {
                return (LocalTargetInfo)toil.actor?.CurJob?.GetTarget(eatSurfaceInd);
            }

            return thing;
        });
        toil.PlaySustainerOrSound(delegate
        {
            if (!chewer.RaceProps.Humanlike)
            {
                return null;
            }

            var target = toil.actor.CurJob.GetTarget(ingestibleInd);
            return !target.HasThing ? null : target.Thing.def.ingestible.ingestSound;
        });
    }

    public Toil DrinkCorpse(float durationMultiplier)
    {
        report = "ROM_ConsumeJob2".Translate();
        var drinkCorpse = new Toil
        {
            initAction = delegate
            {
                Thing thing = Victim.Corpse;

                pawn.rotationTracker.FaceCell(TargetA.Thing.Position);

                if (!thing.IngestibleNow)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                pawn.jobs.curDriver.ticksLeftThisToil =
                    Mathf.RoundToInt(thing.def.ingestible.baseIngestTicks * durationMultiplier);
            },
            tickAction = delegate { pawn.GainComfortFromCellIfPossible(); }
        };
        drinkCorpse.WithProgressBar(TargetIndex.A, delegate
        {
            var thing = pawn?.CurJob?.GetTarget(TargetIndex.B).Thing;
            if (thing == null)
            {
                return 1f;
            }

            return 1f - (pawn.jobs.curDriver.ticksLeftThisToil /
                         Mathf.Round(thing.def.ingestible.baseIngestTicks * durationMultiplier));
        });
        drinkCorpse.defaultCompleteMode = ToilCompleteMode.Delay;
        drinkCorpse.WithEffect(EffecterDefOf.EatMeat, TargetIndex.A);
        drinkCorpse.FailOnDestroyedOrNull(TargetIndex.B);
        drinkCorpse.PlaySustainerOrSound(() => SoundDefOf.RawMeat_Eat);
        drinkCorpse.AddFinishAction(delegate
        {
            //Log.Message("11");

            if (pawn?.CurJob == null)
            {
                return;
            }

            if (Cocoon == null)
            {
                return;
            }

            var thing = job.GetTarget(TargetIndex.B).Thing;
            if (thing == null)
            {
                return;
            }

            pawn.ClearAllReservations();
        });
        return drinkCorpse;
    }

    public override bool TryMakePreToilReservations(bool showResult)
    {
        return pawn.Reserve(Cocoon, job);
    }
}