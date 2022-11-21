using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GeneTools
{
    public class GeneToolsGeneDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> forcedBodyTypesFemale;
        public List<BodyTypeDef> forcedBodyTypesBaby;
        public List<BodyTypeDef> forcedBodyTypesChild;

        public List<HeadTypeDef> forcedHeadTypes;
        public List<HeadTypeDef> forcedHeadTypesFemale;
        public List<HeadTypeDef> forcedHeadTypesBaby;
        public List<HeadTypeDef> forcedHeadTypesChild;
    }

    public class GeneToolsApparelDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> allowedBodyTypes;
        public List<HeadTypeDef> forcedHeadTypes;
        public List<HeadTypeDef> allowedHeadTypes;

    }

    public class GeneToolsBodyTypeDef : DefModExtension
    {
        public bool colorBody = true;
        public BodyTypeDef substituteBody;
        public bool useShader;
    }
    public class GeneToolsHeadTypeDef : DefModExtension
    {
        public bool colorHead = true;
        public bool useShader;
    }
}
