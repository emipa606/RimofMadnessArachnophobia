using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Arachnophobia;

public static class Utility
{
    public static HashSet<Thing> CocoonsFor(Map map, Thing t)
    {
        //All cocoons in the allowed area for Thing t.
        //var allCocoons = new List<Thing>(map.GetComponent<MapComponent_CocoonTracker>().AllCocoons);

        //Wild spiders should go for non-home located cocoons and cocoons that are not in storage areas.
        var wildCocoons = map.GetComponent<MapComponent_CocoonTracker>().WildCocoons;
        if (wildCocoons != null && t.Faction != Faction.OfPlayerSilentFail)
        {
            return wildCocoons;
        }

        //Domestic spiders should go for home located cocoons or cocoons in storage areas.
        var domesticCocoons = map.GetComponent<MapComponent_CocoonTracker>().DomesticCocoons;
        if (domesticCocoons != null && t.Faction == Faction.OfPlayerSilentFail)
        {
            return new HashSet<Thing>(domesticCocoons.Where(x => x.PositionHeld.InAllowedArea(t as Pawn)));
        }

        //Other cases should not exist.
        //("Arachophobia :: No cocoons exist");
        return null;
    }

    public static Thing DetermineBestCocoon(List<Thing> cocoons, PawnWebSpinner spinner)
    {
        Thing result = null;
        if (cocoons != null && cocoons.Count > 0)
        {
            result = GenClosest.ClosestThingReachable(spinner.Position, spinner.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch,
                TraverseParms.For(spinner), 9999,
                x => x is Building_Cocoon y && cocoons.Contains(y) && y.isConsumableBy(spinner));
        }

        return result;
    }
}