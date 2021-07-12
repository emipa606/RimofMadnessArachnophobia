using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Arachnophobia
{
    public class CompMultiHatcher : ThingComp
    {
        private float gestateProgress;

        public Faction hatcheeFaction;

        public Pawn hatcheeParent;

        public Pawn otherParent;

        public CompProperties_MultiHatcher Props => (CompProperties_MultiHatcher) props;

        private CompTemperatureRuinable FreezerComp => parent.GetComp<CompTemperatureRuinable>();

        protected bool TemperatureDamaged
        {
            get
            {
                var freezerComp = FreezerComp;
                return freezerComp != null && FreezerComp.Ruined;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gestateProgress, "gestateProgress");
            Scribe_References.Look(ref hatcheeParent, "hatcheeParent");
            Scribe_References.Look(ref otherParent, "otherParent");
            Scribe_References.Look(ref hatcheeFaction, "hatcheeFaction");
        }

        public override void CompTick()
        {
            //if (!this.TemperatureDamaged)
            //{
            var num = 1f / (Props.hatcherDaystoHatch * 60000f);
            gestateProgress += num;
            if (gestateProgress > 1f)
            {
                Hatch();
            }

            //}
        }

        public void Hatch()
        {
            var request = new PawnGenerationRequest(Props.hatcherPawn, hatcheeFaction, PawnGenerationContext.NonPlayer,
                -1, false, true, false, false, true, false, 1f, false, true, true, false);
            for (var i = 0; i < parent.stackCount; i++)
            {
                var range = Props.hatcherNumber.RandomInRange;
                for (var o = 0; o < range; o++)
                {
                    var pawn = PawnGenerator.GeneratePawn(request);
                    if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
                    {
                        if (pawn != null)
                        {
                            if (hatcheeParent != null)
                            {
                                if (hatcheeParent.Faction != null)
                                {
                                    pawn.SetFaction(hatcheeParent.Faction);
                                }

                                if (pawn.playerSettings != null && hatcheeParent.playerSettings != null &&
                                    hatcheeParent.Faction == hatcheeFaction)
                                {
                                    pawn.playerSettings.AreaRestriction = hatcheeParent.playerSettings.AreaRestriction;
                                }

                                if (pawn.RaceProps.IsFlesh)
                                {
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, hatcheeParent);
                                }
                            }

                            if (otherParent != null &&
                                (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) &&
                                pawn.RaceProps.IsFlesh)
                            {
                                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, otherParent);
                            }
                        }

                        if (parent.Spawned)
                        {
                            FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_AmnioticFluid);
                        }
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    }
                }
            }

            parent.Destroy();
        }


        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            var t = count / (float) (parent.stackCount + count);
            var comp = ((ThingWithComps) otherStack).GetComp<CompMultiHatcher>();
            var b = comp.gestateProgress;
            gestateProgress = Mathf.Lerp(gestateProgress, b, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            var comp = ((ThingWithComps) piece).GetComp<CompMultiHatcher>();
            comp.gestateProgress = gestateProgress;
            comp.hatcheeParent = hatcheeParent;
            comp.otherParent = otherParent;
            comp.hatcheeFaction = hatcheeFaction;
        }

        public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PrePreTraded(action, playerNegotiator, trader);
            if (action == TradeAction.PlayerBuys)
            {
                hatcheeFaction = Faction.OfPlayer;
            }
            else if (action == TradeAction.PlayerSells)
            {
                hatcheeFaction = trader.Faction;
            }
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
            hatcheeFaction = forFaction;
        }

        public override string CompInspectStringExtra()
        {
            if (!TemperatureDamaged)
            {
                return "EggProgress".Translate() + ": " + gestateProgress.ToStringPercent();
            }

            return null;
        }
    }
}