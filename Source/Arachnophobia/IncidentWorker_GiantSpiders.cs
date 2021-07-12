using RimWorld;
using Verse;

namespace Arachnophobia
{
    internal class IncidentWorker_GiantSpiders : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var giantSpider = ROMADefOf.ROMA_SpiderKindGiant;
            var giantSpiderQueen = ROMADefOf.ROMA_SpiderKindGiantQueen;
            var queenSpawned = false;
            var desc = "ROM_SpidersArrivedNoQueen";
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var intVec, map, CellFinder.EdgeRoadChance_Animal))
            {
                return false;
            }

            var points = parms.points;
            var num = Rand.RangeInclusive(3, 6);
            IntVec3 loc;
            for (var i = 0; i < num; i++)
            {
                var newThing = PawnGenerator.GeneratePawn(giantSpider);
                loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
                GenSpawn.Spawn(newThing, loc, map);
                points -= giantSpider.combatPower;
            }

            loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10);
            if (points > giantSpiderQueen.combatPower)
            {
                queenSpawned = true;
                var spiderQueen = PawnGenerator.GeneratePawn(giantSpiderQueen);
                GenSpawn.Spawn(spiderQueen, loc, map);
            }

            //ROM_SpidersArrived
            if (queenSpawned)
            {
                desc = "ROM_SpidersArrived";
            }

            Find.LetterStack.ReceiveLetter("ROM_LetterLabelSpidersArrived".Translate(), desc.Translate(),
                LetterDefOf.ThreatSmall, new TargetInfo(intVec, map));
            return true;
        }
    }
}