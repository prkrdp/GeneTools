using HarmonyLib;
using Verse;

namespace GeneTools
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main() => new Harmony("GeneTools").PatchAll();
    }
}
