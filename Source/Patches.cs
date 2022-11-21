using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

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
            if (thing.def.IsApparel)
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

    [HarmonyPatch(typeof(ApparelRequirement), "AllowedForPawn")]
    public static class ApparelAllowedRestriction /* Check if apparel can be used for a pawn kind */
    {
        public static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
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

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")] /* Prevent pawns from equiping clothes themselves that don't fit */
    internal static class ApparelScoreRestriction 
    {
        public static void Postfix(ref Pawn pawn, ref Apparel ap, ref List<float> wornScoresCache, ref float __result)
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

    [HarmonyPatch(typeof(PawnApparelGenerator), "CanUsePair")] /* Prevent pawns from spawning in with apparel that doesn't fit */
    internal static class StartingApparelRestriction
    {
        public static void Postfix(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed, ref bool __result)
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

        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]  /* Allow body to use substitute apparel texture if available */
        public static class UseSubstituteBody
        {
            [HarmonyPostfix]
            public static bool Prefix(ref PawnGraphicSet __instance)
            {
                __instance.ClearCache();
                __instance.apparelGraphics.Clear();
                using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
                {
                    bool fixedGraphic = false;
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
                    return !fixedGraphic;
                }
            }
        }

}