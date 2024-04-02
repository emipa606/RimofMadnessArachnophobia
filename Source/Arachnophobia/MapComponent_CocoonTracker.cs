using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Arachnophobia;

public class MapComponent_CocoonTracker(Map map) : MapComponent(map)
{
    private HashSet<Thing> domesticCocoons;
    public bool isGiantSpiderPair;
    public bool isSpiderPair;

    private HashSet<Thing> wildCocoons;
    //Wild spiders should go for non-home located cocoons and cocoons that are not in storage areas.
    //var wildCocoons = new List<Thing>(


    //    //Domestic spiders should go for home located cocoons or cocoons in storage areas.
    //    var domesticCocoons = new List<Thing>(
    //        allCocoons?.FindAll(x => (x.Map?.areaManager?.Home[x.Position] ?? false) || (x?.IsInAnyStorage() ?? false))
    //        );
    //    if (domesticCocoons != null && domesticCocoons.Count > 0 && t.Faction == Faction.OfPlayerSilentFail) return domesticCocoons;

    //    //Other cases should not exist.
    //    //("Arachophobia :: No cocoons exist");
    //    return allCocoons;

    public HashSet<Thing> AllCocoons => [..WildCocoons.Concat(DomesticCocoons).ToList()];

    public HashSet<Thing> WildCocoons
    {
        get
        {
            if (wildCocoons == null)
            {
                wildCocoons =
                [
                    ..map?.listerThings?.AllThings?.FindAll(x =>
                        x is Building_Cocoon y && y.Spawned && (!x.Map?.areaManager?.Home[x.Position] ?? false) &&
                        !x.IsInAnyStorage())
                ];
            }

            return wildCocoons;
        }
    }

    public HashSet<Thing> DomesticCocoons
    {
        get
        {
            if (domesticCocoons != null)
            {
                return domesticCocoons;
            }

            var allTemp = map?.listerThings?.AllThings?.FindAll(x => x is Building_Cocoon { Spawned: true });
            domesticCocoons =
            [
                ..allTemp?.FindAll(x =>
                    (x.Map?.areaManager?.Home[x.Position] ?? false) || x.IsInAnyStorage())
            ];

            return domesticCocoons;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isSpiderPair, "isSpiderPair");
        Scribe_Values.Look(ref isGiantSpiderPair, "isGiantSpiderPair");
    }
}