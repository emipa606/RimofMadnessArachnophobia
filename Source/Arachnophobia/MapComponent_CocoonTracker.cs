using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Arachnophobia;

public class MapComponent_CocoonTracker(Map map) : MapComponent(map)
{
    public bool isGiantSpiderPair;
    public bool isSpiderPair;

    public HashSet<Thing> AllCocoons => [.. WildCocoons.Concat(DomesticCocoons).ToList()];

    public HashSet<Thing> WildCocoons
    {
        get
        {
            field ??=
            [
                .. map?.listerThings?.AllThings?.FindAll(x =>
                    x is Building_Cocoon { Spawned: true } && (!x.Map?.areaManager?.Home[x.Position] ?? false) &&
                    !x.IsInAnyStorage())!
            ];

            return field;
        }
    }

    public HashSet<Thing> DomesticCocoons
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var allTemp = map?.listerThings?.AllThings?.FindAll(x => x is Building_Cocoon { Spawned: true });
            field =
            [
                .. allTemp?.FindAll(x =>
                    (x.Map?.areaManager?.Home[x.Position] ?? false) || x.IsInAnyStorage())!
            ];

            return field;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isSpiderPair, "isSpiderPair");
        Scribe_Values.Look(ref isGiantSpiderPair, "isGiantSpiderPair");
    }
}