using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Arachnophobia
{
    public class Building_Web : Building
    {
        private readonly HashSet<Pawn> touchingPawns = new HashSet<Pawn>();
        private PawnWebSpinner spinner;

        public PawnWebSpinner Spinner
        {
            get => spinner;
            set => spinner = value;
        }

        public void WebEffect(Pawn p)
        {
            try
            {
                if (p == null)
                {
                    return;
                }

                if (!WebShouldAffect(p))
                {
                    return;
                }

                var num = 20f;
                var num2 = Mathf.Lerp(0.85f, 1.15f, p.thingIDNumber ^ 74374237);
                num *= num2;
                p.TakeDamage(new DamageInfo(DamageDefOf.Stun, (int) num, 1f, -1, this));
                if (!Destroyed)
                {
                    var leavingsRect = new CellRect(this.OccupiedRect().minX, this.OccupiedRect().minZ,
                        this.OccupiedRect().Width, this.OccupiedRect().Height);
                    if (Rand.Value > 0.9)
                    {
                        Destroy(DestroyMode.KillFinalize);
                    }
                    else
                    {
                        for (var i = leavingsRect.minZ; i <= leavingsRect.maxZ; i++)
                        {
                            for (var j = leavingsRect.minX; j <= leavingsRect.maxX; j++)
                            {
                                var c = new IntVec3(j, 0, i);
                                if (Rand.Value > 0.5f)
                                {
                                    FilthMaker.TryMakeFilth(c, Map, def.filthLeaving,
                                        Rand.RangeInclusive(1, 3));
                                }
                            }
                        }

                        Destroy();
                    }
                }

                spinner?.Notify_WebTouched(p);

                if (p.Faction == Faction.OfPlayerSilentFail)
                {
                    Messages.Message("ROM_SpiderWebsCrossed".Translate(p.LabelShort), p,
                        MessageTypeDefOf.NeutralEvent);
                }

                if (spinner != null)
                {
                    spinner.Web = null;
                }
            }
            catch
            {
                /* Log.Message(e.ToString()); */
            }
        }

        private bool WebShouldAffect(Pawn p)
        {
            var isViableAnimal = p?.RaceProps?.Animal == true && p.Faction != Faction.OfPlayer &&
                                 p.HostFaction?.IsPlayer == false;
            var isViablePrey = p?.Faction?.HostileTo(Spinner.Faction) == true || isViableAnimal;
            var isPlayerSpinner = Spinner?.Faction == Faction.OfPlayer && isViablePrey;
            return isPlayerSpinner || Spinner?.Faction != Faction.OfPlayer;
        }

        public override void Tick()
        {
            var thingList = new HashSet<Thing>(Position.GetThingList(Map));
            foreach (var t in thingList)
            {
                if (t is not Pawn pawn || pawn is PawnWebSpinner || touchingPawns.Contains(pawn))
                {
                    continue;
                }

                touchingPawns.Add(pawn);
                WebEffect(pawn);
            }

            var temp = new HashSet<Pawn>(touchingPawns);
            foreach (var p in temp)
            {
                if (!p.Spawned || p.Position != Position)
                {
                    touchingPawns.Remove(p);
                }
            }

            base.Tick();
            //try
            //{
            //    if (!this.Destroyed && this.Spawned && Find.TickManager.TicksGame % 20 == 0)
            //    {
            //        var mapHeld = this.MapHeld;
            //        if (mapHeld != null)
            //        {
            //            var cells = new List<IntVec3>(this?.OccupiedRect().Cells?.ToList() ?? null);
            //            var cellCount = (!cells.NullOrEmpty()) ? cells.Count : 0;
            //            for (int j = 0; j < cellCount; j++)
            //            {
            //                var thingList = (!cells.NullOrEmpty()) ? cells[j].GetThingList(mapHeld) : null;
            //                var thingCount = (!thingList.NullOrEmpty()) ? thingList.Count : 0;
            //                for (int i = 0; i < thingCount; i++)
            //                {
            //                    if (thingList[i] is Pawn p && !(thingList[i] is PawnWebSpinner))
            //                    {
            //                        WebEffect(p);
            //                    }
            //                }
            //            }
            //            cells = null;
            //        }
            //    }
            //}
            //catch (Exception) { }
        }

        public override string GetInspectString()
        {
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
            result.AppendLine(compDisappearsStr);

            return result.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spinner, "spinner");
        }
    }
}