using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace GeneTools
{
    public class GeneToolsGeneDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> forcedBodyTypesFemale;
        public List<BodyTypeDef> forcedBodyTypesBaby;
        public List<BodyTypeDef> forcedBodyTypesChild;
    }

    public class GeneToolsApparelDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> allowedBodyTypes;
    }

    public class GeneToolsBodyTypeDef : DefModExtension
    {
        public bool colorBody = true;
        public bool useShader;
    }
    public class GeneToolsHeadTypeDef : DefModExtension
    {
        public bool colorHead = true;
        public bool useShader;
    }
}
