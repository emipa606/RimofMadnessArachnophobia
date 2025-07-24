using System.Reflection;
using HarmonyLib;
using Verse;

namespace Arachnophobia;

[StaticConstructorOnStartup]
internal static class PatchInitiator
{
    static PatchInitiator()
    {
        new Harmony("rimworld.jecrell.arachnophobia").PatchAll(Assembly.GetExecutingAssembly());
    }
}