using Mlie;
using UnityEngine;
using Verse;

namespace Arachnophobia;

public class ModMain : Mod
{
    private static string currentVersion;
    private readonly Settings settings;

    public ModMain(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
        ModInfo.romSpiderFactor = settings.romSpiderFactor;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.RimofMadnessArachnophobia"));
    }

    public override string SettingsCategory()
    {
        return "Rim of Madness - Arachnophobia";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        string label = settings.romSpiderFactor < 0.25f
            ? "ROM_SettingsSpiderMultiplier_None".Translate()
            : "ROM_SettingsSpiderMultiplier_Num".Translate(settings.romSpiderFactor);

        settings.romSpiderFactor = Widgets.HorizontalSlider(inRect.TopHalf().TopHalf().TopHalf(),
            settings.romSpiderFactor, 0.0f, 10f, false, label, null, null, 0.25f);
        if (currentVersion != null)
        {
            GUI.contentColor = Color.gray;
            Widgets.Label(inRect.TopHalf().TopHalf(), "ROM_SettingsCurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        WriteSettings();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        if (Find.World?.GetComponent<WorldComponent_ModSettings>() is not { } modSettings)
        {
            return;
        }

        ModInfo.romSpiderFactor = settings.romSpiderFactor;
        modSettings.SpiderDefsModified = false;
    }
}