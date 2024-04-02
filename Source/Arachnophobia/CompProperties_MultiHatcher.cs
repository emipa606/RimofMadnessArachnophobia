using Verse;

namespace Arachnophobia;

public class CompProperties_MultiHatcher : CompProperties
{
    public readonly float hatcherDaystoHatch = 1f;

    public IntRange hatcherNumber = new IntRange(2, 3);

    public PawnKindDef hatcherPawn;

    public CompProperties_MultiHatcher()
    {
        compClass = typeof(CompMultiHatcher);
    }
}