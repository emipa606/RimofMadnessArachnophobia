using Verse;

namespace Arachnophobia;

public class Settings : ModSettings
{
    public float romSpiderFactor = 1;
    public string romSpiderFactorBuffer;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref romSpiderFactor, "romSpiderFactor");
    }
}