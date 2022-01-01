using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia;

public class JobDriver_SpinWeb : JobDriver
{
    private const TargetIndex LaySpotInd = TargetIndex.A;

    private ThingDef WebDef
    {
        get
        {
            var result = ROMADefOf.ROMA_Web;
            if ((pawn?.RaceProps?.baseBodySize ?? 0) > 2)
            {
                result = ROMADefOf.ROMA_WebGiant;
            }

            return result;
        }
    }

    public override bool TryMakePreToilReservations(bool showResult)
    {
        return true;
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 500,
            initAction = delegate
            {
                var i = 999;
                var breakNow = false;
                while (i > 0)
                {
                    pawn.CurJob.SetTarget(TargetIndex.A,
                        RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some));
                    var cellRect = GenAdj.OccupiedRect(TargetLocA, Rot4.North, WebDef.Size);
                    foreach (var cellRectCell in cellRect.Cells)
                    {
                        if (cellRectCell.Walkable(Map))
                        {
                            breakNow = true;
                        }
                    }

                    if (GenConstruct.CanPlaceBlueprintAt(WebDef, TargetLocA, Rot4.North, pawn.Map).Accepted)
                    {
                        if (pawn?.Faction == null || pawn?.Faction != Faction.OfPlayerSilentFail)
                        {
                            break;
                        }

                        if (pawn?.Faction == Faction.OfPlayerSilentFail && !TargetA.Cell.IsForbidden(pawn))
                        {
                            breakNow = true;
                        }
                    }
                    else
                    {
                        breakNow = false;
                    }

                    if (breakNow)
                    {
                        break;
                    }

                    i--;
                }
            }
        };
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        yield return new Toil
        {
            initAction = delegate
            {
                if (GetActor() is not PawnWebSpinner spinner || spinner.Position.CloseToEdge(spinner.Map, 2))
                {
                    return;
                }

                var web = (Building_Web)GenSpawn.Spawn(WebDef, spinner.Position, spinner.Map);
                spinner.Web = web;
                spinner.WebsMade++;
                web.Spinner = spinner;
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}