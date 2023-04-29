using AlienRace.ApparelGraphics;
using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace GeneTools
{
    [StaticConstructorOnStartup]
    public class Main
    {
        public static bool HARactive;
        static Main() 
        {
            if (LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.Name == "Humanoid Alien Races 2.0")))
                Main.HARactive = true;
            if (LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.Name.Contains("Humanoid Alien Races"))))
                Main.HARactive = true;
            if (LoadedModManager.RunningModsListForReading.Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId == "erdelf.HumanoidAlienRaces")))
                Main.HARactive = true;
            Harmony harmony = new Harmony("GeneTools");
            /*If HAR is loaded and HARfix in the options menu is enabled, load the patches that make this mod only affect humans and prevent HAR from affecting humans affected by this mod*/
            try
            {
                ((Action)(() =>
                {
                    if (Main.HARactive && GeneToolsSettings.HARfix)
                    {
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), prefix: new HarmonyMethod(typeof(ResolveAllGraphicsHARpatch), "Prefix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), postfix: new HarmonyMethod(typeof(GtResolveAllGraphicsHARpatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGenerator), "GetBodyTypeFor"), postfix: new HarmonyMethod(typeof(GtGetBodyTypeForHARpatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"), postfix: new HarmonyMethod(typeof(GtNotify_GenesChangedHARpatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelRequirement), "AllowedForPawn"), postfix: new HarmonyMethod(typeof(GtAllowedForPawnHARPatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain"), postfix: new HarmonyMethod(typeof(GtApparelScoreGainHARPatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnApparelGenerator), "CanUsePair"), postfix: new HarmonyMethod(typeof(GtCanUsePairHARPatch), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"), prefix: new HarmonyMethod(typeof(GtResolveApparelGraphicsHARPatch), "Prefix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelGraphicsOverrides), "TryGetBodyTypeFallback"), postfix: new HarmonyMethod(typeof(GtTryGetBodyTypeFallbackHARPatch), "Postfix"));
                        harmony.Patch(typeof(EquipmentUtility).GetMethod("CanEquip", new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }), postfix: new HarmonyMethod(typeof(GtCanEquipHARPatch), "Postfix"));
                        Log.Message("[GeneTools] HAR Patches Applied");
                    }

                }))();
            }
            catch (TypeLoadException ex)
            {
            }

            try
            {
                ((Action)(() =>
                {

                    if (!Main.HARactive || !GeneToolsSettings.HARfix)
                    {                       
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), postfix: new HarmonyMethod(typeof(GtResolveAllGraphics), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGenerator), "GetBodyTypeFor"), postfix: new HarmonyMethod(typeof(GtGetBodyTypeFor), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"), postfix: new HarmonyMethod(typeof(GtNotify_GenesChanged), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelRequirement), "AllowedForPawn"), postfix: new HarmonyMethod(typeof(GtAllowedForPawn), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain"), postfix: new HarmonyMethod(typeof(GtApparelScoreGain), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnApparelGenerator), "CanUsePair"), postfix: new HarmonyMethod(typeof(GtCanUsePair), "Postfix"));
                        harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"), prefix: new HarmonyMethod(typeof(GtResolveApparelGraphics), "Prefix"));
                        harmony.Patch(typeof(EquipmentUtility).GetMethod("CanEquip", new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }), postfix: new HarmonyMethod(typeof(GtCanEquip), "Postfix"));
                        Log.Message("[GeneTools] HAR not found or HAR fix disabled");
                    }

                }))();
            }
            catch (TypeLoadException ex)
            {
            }
        } 
    }
}
