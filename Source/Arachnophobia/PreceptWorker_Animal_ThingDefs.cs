using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Arachnophobia;

[HarmonyPatch(typeof(PreceptWorker_Animal), nameof(PreceptWorker_Animal.ThingDefs), MethodType.Getter)]
public static class PreceptWorker_Animal_ThingDefs
{
    public static IEnumerable<PreceptThingChance> Postfix(IEnumerable<PreceptThingChance> values)
    {
        foreach (var preceptThingChance in values)
        {
            yield return preceptThingChance;
        }

        foreach (var def in from x in DefDatabase<ThingDef>.AllDefs
                 where x.race?.Animal == true && x.defName.StartsWith("ROMA_Spider")
                 select x)
        {
            yield return new PreceptThingChance
            {
                def = def,
                chance = 1f
            };
        }
    }
}