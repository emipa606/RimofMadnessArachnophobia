using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Arachnophobia;

public class MapComponent_CocoonTracker : MapComponent
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

    public MapComponent_CocoonTracker(Map map) : base(map)
    {
    }

    public HashSet<Thing> AllCocoons => new HashSet<Thing>(WildCocoons?.Concat(DomesticCocoons)?.ToList() ?? null);

    public HashSet<Thing> WildCocoons
    {
        get
        {
            if (wildCocoons == null)
            {
                wildCocoons = new HashSet<Thing>(map?.listerThings?.AllThings?.FindAll(x =>
                    x is Building_Cocoon y && y.Spawned && (!x.Map?.areaManager?.Home[x.Position] ?? false) &&
                    (!x?.IsInAnyStorage() ?? false)));
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
            domesticCocoons = new HashSet<Thing>(allTemp?.FindAll(x =>
                (x.Map?.areaManager?.Home[x.Position] ?? false) || (x?.IsInAnyStorage() ?? false)));

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