// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using Verse;
// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// RimWorld specific functions are found here (like 'Building_Battery')
// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// <summary>
// Utility File for use between Cthulhu mods.
// Last Update: 5/5/2017
// </summary>
namespace Arachnophobia;

public static class CthulhuUtility
{
    private const string SanityLossDef = "ROM_SanityLoss";
    private const string AltSanityLossDef = "Cults_SanityLoss";

    private static bool modCheck;
    private static bool loadedCosmicHorrors;
    private static bool loadedIndustrialAge;
    private static bool loadedCults;


    /// <summary>
    ///     This method handles the application of Sanity Loss in multiple mods.
    ///     It returns true and false depending on if it applies successfully.
    /// </summary>
    /// <param name="pawn"></param>
    public static void RemoveSanityLoss(Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }

        var sanityLossDef = !IsCosmicHorrorsLoaded() ? AltSanityLossDef : SanityLossDef;

        var pawnSanityHediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail(sanityLossDef));
        if (pawnSanityHediff != null)
        {
            pawn.health.RemoveHediff(pawnSanityHediff);
        }
    }


    private static bool IsCosmicHorrorsLoaded()
    {
        if (!modCheck)
        {
            ModCheck();
        }

        return loadedCosmicHorrors;
    }


    private static void ModCheck()
    {
        loadedCosmicHorrors = false;
        loadedIndustrialAge = false;
        foreach (var ResolvedMod in LoadedModManager.RunningMods)
        {
            if (loadedCosmicHorrors && loadedIndustrialAge && loadedCults)
            {
                break; //Save some loading
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Cosmic Horrors"))
            {
                //DebugReport("Loaded - Call of Cthulhu - Cosmic Horrors");
                loadedCosmicHorrors = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Industrial Age"))
            {
                //DebugReport("Loaded - Call of Cthulhu - Industrial Age");
                loadedIndustrialAge = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Cults"))
            {
                //DebugReport("Loaded - Call of Cthulhu - Cults");
                loadedCults = true;
            }

            if (ResolvedMod.Name.Contains("Call of Cthulhu - Factions"))
            {
                //DebugReport("Loaded - Call of Cthulhu - Factions");
            }
        }

        modCheck = true;
    }
}