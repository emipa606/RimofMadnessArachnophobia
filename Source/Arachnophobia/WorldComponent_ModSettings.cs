using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Arachnophobia;

public class WorldComponent_ModSettings : WorldComponent
{
    public WorldComponent_ModSettings(World world) : base(world)
    {
    }

    public bool SpiderDefsModified { get; set; }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        ResolveSpiderDefSettings();
    }

    public void ResolveSpiderDefSettings()
    {
        if (SpiderDefsModified)
        {
            return;
        }

        Log.Message($"Arachnophobia :: Spider Biome Settings Adjusted :: Current Factor: {ModInfo.romSpiderFactor}");
        SpiderDefsModified = true;

        var spiders =
            new List<ThingDef>
            {
                ROMADefOf.ROMA_SpiderRace,
                ROMADefOf.ROMA_SpiderRaceGiant
            };

        var spiderKinds =
            new List<PawnKindDef>
            {
                ROMADefOf.ROMA_SpiderKind,
                ROMADefOf.ROMA_SpiderKindGiant
            };

        foreach (var def in spiders)
        {
            var wildBomes = def.race.wildBiomes;
            if (wildBomes == null)
            {
                continue;
            }

            foreach (var record in def.race.wildBiomes)
            {
                record.commonality *= ModInfo.romSpiderFactor;
            }
        }

        foreach (var kind in spiderKinds)
        {
            kind.ecoSystemWeight *= ModInfo.romSpiderFactor;
        }
    }
}