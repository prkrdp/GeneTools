using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using System.Runtime;
using System.Reflection;
using System.Collections;
using Verse.Noise;

namespace GeneTools
{
    public class GeneToolsSettings : ModSettings
    {
        public static bool HARfix = true;      
        public static bool forcedApparelChecking = true;
        public static bool autoApparelChecking = true;
        public static bool spawnApparelChecking = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref HARfix, "HARfix");
            Scribe_Values.Look(ref forcedApparelChecking, "forcedApparelChecking");
            Scribe_Values.Look(ref autoApparelChecking, "forcedApparelChecking");
            Scribe_Values.Look(ref spawnApparelChecking, "forcedApparelChecking");         
            base.ExposeData();
        }

    }

    public class GeneToolsMod : Mod
    {
        GeneToolsSettings settings;

        public GeneToolsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<GeneToolsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Enable Experimental HAR fix (Requires Restart)", ref GeneToolsSettings.HARfix, "Prevents HAR from affecting the rendering of a human pawn if said pawn has any genes or body types affected by this mod. Prevents this mod from affecting any alien race's rendering or apparel restrictions. Fixes rendering issues and error log spam with HAR but may cause issues with any other mod that affects pawn rendering.");
            listingStandard.CheckboxLabeled("Enable Force Equipped Apparel Checking", ref GeneToolsSettings.forcedApparelChecking, "When checked this will restrict apparel from being manually equipped by player order if the pawn's bodytype is not listed in the apparel's 'allowed bodytypes' or 'forced bodytypes' in xml mod extensions. Can cause issues with some mods that add apparel and bodies if they aren't made with this mod in mind. If unchecked, pawns that are made to wear apparel with no compatible graphic will likely cause pink boxes and error log spam. (Recommended to leave checked)");
            listingStandard.CheckboxLabeled("Enable Autonomously Equipped Apparel Checking", ref GeneToolsSettings.autoApparelChecking, "When checked this will restrict apparel from being sought out and equipped by pawns if the pawn's bodytype is not listed in the apparel's 'allowed bodytypes' or 'forced bodytypes' in xml mod extensions. Can cause issues with some mods that add apparel and bodies if they aren't made with this mod in mind. If unchecked, pawns that equip apparel with no compatible graphic will likely cause pink boxes and error log spam. (Recommended to leave checked)");
            listingStandard.CheckboxLabeled("Enable Pawn Spawn Apparel Checking", ref GeneToolsSettings.spawnApparelChecking, "When checked this will prevent a pawn from spawning with a piece of apparel equipped if the pawn's bodytype is not listed in the apparel's 'allowed bodytypes' or 'forced bodytypes' in xml mod extensions. Can cause issues with some mods that add apparel and bodies if they aren't made with this mod in mind. If unchecked, pawns that spawn with apparel with no compatible graphic will likely cause pink boxes and error log spam. (Recommended to leave checked)");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Gene Tools";
        }
    }
}
