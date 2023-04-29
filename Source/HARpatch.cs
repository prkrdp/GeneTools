using AlienRace;
using AlienRace.ApparelGraphics;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace GeneTools
{
    internal class ResolveAllGraphicsHARpatch
    {
        /*Overwrites HAR's ResolveAllGraphics prepatch with the original method if the pawn is a human with genes made for this mod
         This is more potentially destructive to other mods than I would like*/
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool Prefix(PawnGraphicSet __instance)
        {
         
            bool isGtHuman = false;
            bool HARfix = GeneToolsSettings.HARfix;
            bool result = true;

            bool isHuman = false;
            if (__instance.pawn.RaceProps.Humanlike)
            { 
                isHuman = !AlienRace.Utilities.DifferentRace(__instance.pawn.def, ThingDefOf.Human); 
            }

            if (isHuman)
            {
                Color SwaddleColor()
                {
                    Rand.PushState(__instance.pawn.thingIDNumber);
                    float num1 = Rand.Range(0.6f, 0.89f);
                    float num2 = Rand.Range(-0.1f, 0.1f);
                    float num3 = Rand.Range(-0.1f, 0.1f);
                    float num4 = Rand.Range(-0.1f, 0.1f);
                    Rand.PopState();
                    return new Color(num1 + num2, num1 + num3, num1 + num4);
                }

                if (isHuman)
                {
                    List<Gene> genesListForReading = __instance.pawn.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.def.HasModExtension<GeneToolsGeneDef>())
                        {
                            isGtHuman = true;
                            break;
                        }
                    }
                }

                if (isGtHuman)
                {
                    Color color = __instance.pawn.story.SkinColorOverriden ? PawnGraphicSet.RottingColorDefault * __instance.pawn.story.SkinColor : PawnGraphicSet.RottingColorDefault;
                    __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, __instance.pawn.story.SkinColor);
                    __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, color);
                    __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
                    if (ModLister.BiotechInstalled)
                        __instance.furCoveredGraphic = __instance.pawn.story.furDef == null ? (Graphic)null : GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.furDef.GetFurBodyGraphicPath(__instance.pawn), ShaderDatabase.CutoutSkinOverlay, Vector2.one, __instance.pawn.story.HairColor);
                    if (ModsConfig.BiotechActive)
                        __instance.swaddledBabyGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Apparel/SwaddledBaby/Swaddled_Child", ShaderDatabase.Cutout, Vector2.one, SwaddleColor());
                    if (__instance.pawn.style != null && ModsConfig.IdeologyActive && (!ModLister.BiotechInstalled || __instance.pawn.genes == null || !__instance.pawn.genes.GenesListForReading.Any<Gene>((Predicate<Gene>)(x => x.def.graphicData != null && !x.def.graphicData.tattoosVisible && x.Active))))
                    {
                        Color skinColor = __instance.pawn.story.SkinColor;
                        skinColor.a *= 0.8f;
                        __instance.faceTattooGraphic = __instance.pawn.style.FaceTattoo == null || __instance.pawn.style.FaceTattoo == TattooDefOf.NoTattoo_Face ? (Graphic)null : GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.style.FaceTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, (GraphicData)null, __instance.pawn.story.headType.graphicPath);
                        __instance.bodyTattooGraphic = __instance.pawn.style.BodyTattoo == null || __instance.pawn.style.BodyTattoo == TattooDefOf.NoTattoo_Body ? (Graphic)null : GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.style.BodyTattoo.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, (GraphicData)null, __instance.pawn.story.bodyType.bodyNakedGraphicPath);
                    }
                    __instance.headGraphic = (Graphic)__instance.pawn.story.headType.GetGraphic(__instance.pawn.story.SkinColor, skinColorOverriden: __instance.pawn.story.SkinColorOverriden);
                    __instance.desiccatedHeadGraphic = (Graphic)__instance.pawn.story.headType.GetGraphic(color, true, __instance.pawn.story.SkinColorOverriden);
                    __instance.skullGraphic = (Graphic)HeadTypeDefOf.Skull.GetGraphic(Color.white, true);
                    __instance.headStumpGraphic = (Graphic)HeadTypeDefOf.Stump.GetGraphic(__instance.pawn.story.SkinColor, skinColorOverriden: __instance.pawn.story.SkinColorOverriden);
                    __instance.desiccatedHeadStumpGraphic = (Graphic)HeadTypeDefOf.Stump.GetGraphic(color, true, __instance.pawn.story.SkinColorOverriden);
                    __instance.CalculateHairMats();
                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();
                    result = false;
                }
            }
            return result;
        }
    }

    internal class GtResolveAllGraphicsHARpatch
    {
        [HarmonyPostfix]
        public static void Postfix(PawnGraphicSet __instance)
        {
            bool isHuman = false;
            if (__instance.pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(__instance.pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
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

    public static class GtGetBodyTypeForHARpatch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref BodyTypeDef __result)
        {
            bool isHuman = false;
            if (pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                foreach (Gene gene in genesListForReading)
                {
                    if (gene.def.HasModExtension<GeneToolsGeneDef>())
                    {
                        if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null && pawn.DevelopmentalStage == DevelopmentalStage.Baby)
                        {
                            __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null && pawn.DevelopmentalStage == DevelopmentalStage.Child)
                        {
                            __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null && pawn.gender == Gender.Female && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                        {

                            __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                        {
                            __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes.Count)];
                        }

                        if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby != null && pawn.DevelopmentalStage == DevelopmentalStage.Baby)
                        {
                            pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild != null && pawn.DevelopmentalStage == DevelopmentalStage.Child)
                        {
                            pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale != null && pawn.gender == Gender.Female && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                        {
                            pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale.Count)];
                        }
                        else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                        {
                            pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes.Count)];
                        }
                    }
                }
            }
        }
    }

    public static class GtNotify_GenesChangedHARpatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Pawn ___pawn, GeneDef addedOrRemovedGene)
        {
            bool isHuman = false;
            if (___pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(___pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                if (addedOrRemovedGene != null && addedOrRemovedGene.HasModExtension<GeneToolsGeneDef>())
                {
                    if (addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null)
                        ___pawn.story.bodyType = Verse.PawnGenerator.GetBodyTypeFor(___pawn);
                    ___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                }
            }
        }
    }

    
    public static class GtCanEquipHARPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Pawn pawn, ref Thing thing, out string cantReason, ref bool __result)
        {
            cantReason = (string)null;
            bool apparelChecking = GeneToolsSettings.forcedApparelChecking;
            bool isHuman = false;
            if (pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                if (thing.def.IsApparel && apparelChecking)
                {
                    BodyTypeDef bodyType = pawn.story.bodyType;
                    bool useSubstitute = thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool useSubstituteForced = thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null && !thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool isHat = thing.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
                    bool isInvisible = thing.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || thing.def.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || thing.def.apparel.wornGraphicPath == "" ? true : false;

                    if (!isInvisible && !isHat && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
                    {
                        List<BodyTypeDef> forcedBodies = thing.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                        if (!forcedBodies.Contains(pawn.story.bodyType) && !useSubstituteForced)
                        {
                            cantReason = (string)"does not fit " + pawn.story.bodyType + " body";
                            __result = false;
                        }
                    }
                    if (!isInvisible && !isHat && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
                    {
                        List<BodyTypeDef> allowedBodies = thing.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                        if (!allowedBodies.Contains(pawn.story.bodyType) && !useSubstitute)
                        {
                            cantReason = (string)"does not fit " + pawn.story.bodyType + " body";
                            __result = false;
                        }
                    }
                    if (!isInvisible && isHat && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null)
                    {
                        List<HeadTypeDef> forcedHeads = thing.def.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes;
                        if (!forcedHeads.Contains(pawn.story.headType))
                        {
                            cantReason = (string)"does not fit " + pawn.story.headType + " head";
                            __result = false;
                        }
                    }
                    if (!isInvisible && isHat && thing.def.HasModExtension<GeneToolsApparelDef>() && thing.def.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes != null)
                    {
                        List<HeadTypeDef> allowedHeads = thing.def.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes;
                        if (!allowedHeads.Contains(pawn.story.headType))
                        {
                            cantReason = (string)"does not fit " + pawn.story.headType + " head";
                            __result = false;
                        }
                    }
                }                                                                               
            } 
        }
    }

    public static class GtAllowedForPawnHARPatch /* Check if apparel can be used for a pawn kind */
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
        {
            bool isHuman = false;
            if (p.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(p.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                bool apparelChecking = GeneToolsSettings.spawnApparelChecking;
                if (apparelChecking)
                {
                    BodyTypeDef bodyType = p.story.bodyType;
                    bool useSubstitute = apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool useSubstituteForced = apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null && !apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool isHat = apparel.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
                    bool isInvisible = apparel.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || apparel.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || apparel.apparel.wornGraphicPath == "" ? true : false;
                    if (!isInvisible && !isHat && apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
                    {
                        List<BodyTypeDef> forcedBodies = apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                        if (!forcedBodies.Contains(p.story.bodyType) && !useSubstituteForced)
                        {
                            __result = false;
                        }
                    }
                    if (!isInvisible && !isHat && apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
                    {
                        List<BodyTypeDef> allowedBodies = apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                        if (!allowedBodies.Contains(p.story.bodyType) && !useSubstitute)
                        {
                            __result = false;
                        }
                    }
                    if (!isInvisible && isHat && apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null)
                    {
                        List<HeadTypeDef> forcedHeads = apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes;
                        if (!forcedHeads.Contains(p.story.headType))
                        {
                            __result = false;
                        }
                    }
                    if (!isInvisible && isHat && apparel.IsApparel && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes != null)
                    {
                        List<HeadTypeDef> allowedHeads = apparel.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes;
                        if (!allowedHeads.Contains(p.story.headType))
                        {
                            __result = false;
                        }
                    }
                }
            }
        }
    }

    internal static class GtApparelScoreGainHARPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Pawn pawn, ref Apparel ap, ref List<float> wornScoresCache, ref float __result)
        {
            bool isHuman = false;
            if (pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                bool apparelChecking = GeneToolsSettings.autoApparelChecking;
                if (apparelChecking)
                {
                    BodyTypeDef bodyType = pawn.story.bodyType;
                    bool useSubstitute = ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool useSubstituteForced = ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null && !ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool isHat = ap.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || ap.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || ap.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || ap.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
                    bool isInvisible = ap.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || ap.def.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || ap.def.apparel.wornGraphicPath == "" ? true : false;
                    if (!isInvisible && !isHat && ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
                    {
                        List<BodyTypeDef> forcedBodies = ap.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                        if (!forcedBodies.Contains(pawn.story.bodyType) && !useSubstituteForced)
                        {
                            __result = -1000f;
                        }
                    }
                    if (!isInvisible && !isHat && ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
                    {
                        List<BodyTypeDef> allowedBodies = ap.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                        if (!allowedBodies.Contains(pawn.story.bodyType) && !useSubstitute)
                        {
                            __result = -1000f;
                        }
                    }
                    if (!isInvisible && isHat && ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null)
                    {
                        List<HeadTypeDef> forcedHeads = ap.def.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes;
                        if (!forcedHeads.Contains(pawn.story.headType))
                        {
                            __result = -1000f;
                        }
                    }
                    if (!isInvisible && isHat && ap.def.IsApparel && ap.def.HasModExtension<GeneToolsApparelDef>() && ap.def.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes != null)
                    {
                        List<HeadTypeDef> allowedHeads = ap.def.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes;
                        if (!allowedHeads.Contains(pawn.story.headType))
                        {
                            __result = -1000f;
                        }
                    }
                }
            }
        }
    }

    internal static class GtCanUsePairHARPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed, ref bool __result)
        {
            bool isHuman = false;
            if (pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                bool apparelChecking = GeneToolsSettings.spawnApparelChecking;
                if (apparelChecking)
                {
                    BodyTypeDef bodyType = pawn.story.bodyType;
                    bool useSubstitute = pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool useSubstituteForced = pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null && !pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                    bool isHat = pair.thing.apparel.LastLayer == ApparelLayerDefOf.Overhead || pair.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || pair.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || pair.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
                    bool isInvisible = pair.thing.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || pair.thing.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || pair.thing.apparel.wornGraphicPath == "" ? true : false;
                    if (!isInvisible && !isHat && pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
                    {
                        List<BodyTypeDef> allowedBodies = pair.thing.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                        if (!allowedBodies.Contains(pawn.story.bodyType) && !useSubstitute)
                        {
                            pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                            __result = false;
                        }
                        else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
                    }

                    if (!isInvisible && !isHat && pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
                    {
                        List<BodyTypeDef> forcedBodies = pair.thing.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                        if (!forcedBodies.Contains(pawn.story.bodyType) && !useSubstituteForced)
                        {
                            pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                            __result = false;
                        }
                        else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
                    }

                    if (!isInvisible && isHat && pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes != null)
                    {
                        List<HeadTypeDef> allowedHeads = pair.thing.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes;
                        if (!allowedHeads.Contains(pawn.story.headType))
                        {
                            pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                            __result = false;
                        }
                        else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
                    }

                    if (!isInvisible && isHat && pair.thing.HasModExtension<GeneToolsApparelDef>() && pair.thing.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null)
                    {
                        List<HeadTypeDef> forcedHeads = pair.thing.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes;
                        if (!forcedHeads.Contains(pawn.story.headType))
                        {
                            pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                            __result = false;
                        }
                        else { pair.thing.apparel.canBeGeneratedToSatisfyWarmth = true; }
                    }
                }
            }
        }
    }

    public static class GtResolveApparelGraphicsHARPatch
    {
        public static bool Prefix(ref PawnGraphicSet __instance)
        {
            bool fixedGraphic = false;
            bool isHuman = false;
            if (__instance.pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(__instance.pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {       
                __instance.ClearCache();
                __instance.apparelGraphics.Clear();
                using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
                {

                    while (enumerator.MoveNext())
                    {
                        ApparelGraphicRecord item;
                        Apparel apparel = enumerator.Current;
                        BodyTypeDef bodyType = __instance.pawn.story.bodyType;
                        bool useSubstitute = apparel.def.HasModExtension<GeneToolsApparelDef>() && apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
                        if (useSubstitute && ApparelGraphicRecordGetter.TryGetGraphicApparel(enumerator.Current, bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody, out item))
                        {
                            __instance.apparelGraphics.Add(item);
                            fixedGraphic = true;
                        }
                        else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(enumerator.Current, __instance.pawn.story.bodyType, out item))
                        {
                            __instance.apparelGraphics.Add(item);
                        }
                    }
                }
            }
            return !fixedGraphic;
        }
    }

    public static class GtTryGetBodyTypeFallbackHARPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Pawn pawn, out BodyTypeDef def, ref bool __result)
        {
            def = (BodyTypeDef)null;
            if (pawn == null)
                __result = false;
            bool isHuman = false;
            if (pawn.RaceProps.Humanlike)
            {
                isHuman = !AlienRace.Utilities.DifferentRace(pawn.def, ThingDefOf.Human);
            }

            if (isHuman)
            {
                if (pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null)
                {
                    def = pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody;
                    __result = true;
                }
            }
            else
            {
                __result = false;
            }
            
        }
    }

}
