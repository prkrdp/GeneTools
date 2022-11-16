using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;

namespace GeneTools
{
    [HarmonyPatch(typeof(PawnGenerator), "GetBodyTypeFor")] /* Set body type to any forced by genes */
    public static class AddPawnBody
    {
        [HarmonyPostfix]
        public static void PostFix(Pawn pawn, ref BodyTypeDef __result)
        {
            List<Gene> genesListForReading = pawn.genes.GenesListForReading;
            foreach (Gene gene in genesListForReading)
            {
                if (gene.def.HasModExtension<GeneToolsGeneDef>())
                {
                    if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null && pawn.DevelopmentalStage == DevelopmentalStage.Baby)
                    {
                        __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby[Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby.Count)];
                    }
                    else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null && pawn.DevelopmentalStage == DevelopmentalStage.Child)
                    {
                        __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild[Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild.Count)];
                    }
                    else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null && pawn.gender == Gender.Female && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                    {

                        __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale[Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale.Count)];
                    }
                    else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                    {
                        __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes[Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes.Count)];
                    }

                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")] /* Update body if genes are changed */
    public static class CheckForGeneChange
    {
        [HarmonyPostfix]
        public static void Postfix(ref Pawn ___pawn, GeneDef addedOrRemovedGene)
        {
            if (addedOrRemovedGene != null && addedOrRemovedGene.HasModExtension<GeneToolsGeneDef>())
            {
                if (addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null)
                    ___pawn.story.bodyType = Verse.PawnGenerator.GetBodyTypeFor(___pawn);
                ___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
            }
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")] /* Apply shader changes to body and head */
    public static class BodyColorPatch
    {
        [HarmonyPostfix]
        public static void ResolveAllGraphics(PawnGraphicSet __instance)
        {
            
            if (__instance.pawn.RaceProps.Humanlike)
            {
                Color skinColor = __instance.pawn.story.SkinColor;
                Color hairColor = __instance.pawn.story.HairColor;
                if (__instance.pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && __instance.pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().useShader == true)
                {
                    __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                }
                else if (__instance.pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && __instance.pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().colorBody == false)
                {
                    __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, Color.white);
                }

                if (__instance.pawn.story.headType.HasModExtension<GeneToolsHeadTypeDef>() && __instance.pawn.story.headType.GetModExtension<GeneToolsHeadTypeDef>().useShader == true)
                {
                    __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.headType.graphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                }
                else if (__instance.pawn.story.headType.HasModExtension<GeneToolsHeadTypeDef>() && __instance.pawn.story.headType.GetModExtension<GeneToolsHeadTypeDef>().colorHead == false)
                {
                    __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.headType.graphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, Color.white);
                }
            }
        }
    }

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new System.Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class ApparelEquipRestriction /* Prevent player from making pawn equip apparel that doesn't fit the body */
    {
        public static void Postfix(Pawn pawn, Thing thing, out string cantReason, ref bool __result)
        {
            cantReason = (string)null;
            if (thing.def.IsApparel && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
            {
                List<BodyTypeDef> forcedBodies = thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                if (!forcedBodies.Contains(pawn.story.bodyType))
                {
                    cantReason = (string)"does not fit " + pawn.story.bodyType + " body";
                    __result = false;
                }
            }
            if (thing.def.IsApparel && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
            {
                List<BodyTypeDef> allowedBodies = thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (!allowedBodies.Contains(pawn.story.bodyType))
                {
                    cantReason = (string)"does not fit " + pawn.story.bodyType + " body";
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ApparelRequirement), "AllowedForPawn")]
    public static class ApparelAllowedRestriction /* Check if apparel can be used for a pawn kind */
    {
        public static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
        {
            if (apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
            {
                List<BodyTypeDef> forcedBodies = apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                if (!forcedBodies.Contains(p.story.bodyType))
                {
                    __result = false;
                }
            }
            if (apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
            {
                List<BodyTypeDef> allowedBodies = apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (!allowedBodies.Contains(p.story.bodyType))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")] /* Prevent pawns from equiping clothes themselves that don't fit */
    internal static class ApparelScoreRestriction 
    {
        public static void Postfix(ref Pawn pawn, ref Apparel ap, ref List<float> wornScoresCache, ref float __result)
        {
            if (ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
            {
                List<BodyTypeDef> forcedBodies = ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                if (!forcedBodies.Contains(pawn.story.bodyType))
                {
                    __result = -1000f;
                }
            }
            if (ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
            {
                List<BodyTypeDef> allowedBodies = ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (!allowedBodies.Contains(pawn.story.bodyType))
                {
                    __result = -1000f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnApparelGenerator), "CanUsePair")] /* Prevent pawns from spawning in with apparel that doesn't fit */
    internal static class StartingApparelRestriction
    {
        public static void Postfix(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed, ref bool __result)
        {
            if (pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
            {
                List<BodyTypeDef> allowedBodies = pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (!allowedBodies.Contains(pawn.story.bodyType))
                {
                    pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                    __result = false;
                }
                else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
            }

            if (pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
            {
                List<BodyTypeDef> forcedBodies = pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                if (!forcedBodies.Contains(pawn.story.bodyType))
                {
                    pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                    __result = false;
                }
                else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
            }
        }

    }

}