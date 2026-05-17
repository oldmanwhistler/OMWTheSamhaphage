using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public class OMW_Settings : ModSettings
    {
        public bool logAbilities = false;
        public bool logAnomaly = false;
        public bool logCompAbilityEffect = false;
        public bool logGenes = false;
        public bool logResonance = false;
        public bool logHediffs = false;
        public bool logJobs = false;
        public bool logUI = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref logAbilities, "logAbilities", false);
            Scribe_Values.Look(ref logAnomaly, "logAnomaly", false);
            Scribe_Values.Look(ref logCompAbilityEffect, "logCompAbilityEffect", false);
            Scribe_Values.Look(ref logGenes, "logGenes", false);
            Scribe_Values.Look(ref logResonance, "logResonance", false);
            Scribe_Values.Look(ref logHediffs, "logHediffs", false);
            Scribe_Values.Look(ref logJobs, "logJobs", false);
            Scribe_Values.Look(ref logUI, "logUI", false);
        }
    }

    public class OMW_Mod : Mod
    {
        public static OMW_Settings settings;

        public OMW_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<OMW_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Debug Logging Categories".Colorize(Color.yellow));
            listing.Label("Enable these to see detailed technical information in the console.");
            listing.Gap();

            listing.CheckboxLabeled("Log Abilities", ref settings.logAbilities, "Detailed traces for ability application and logic.");
            listing.CheckboxLabeled("Log Anomaly", ref settings.logAnomaly, "Traces for the anomaly logic for creating shamblers.");
            listing.CheckboxLabeled("Log CompAbilityEffect", ref settings.logCompAbilityEffect, "Traces for menu generation and target selection.");
            listing.CheckboxLabeled("Log Genes", ref settings.logGenes, "Traces for gene addition, removal, and complexity calculation.");
            listing.CheckboxLabeled("Log Resonance", ref settings.logResonance, "Traces for resonance consumption and gains.");
            listing.CheckboxLabeled("Log Hediffs", ref settings.logHediffs, "Traces for technical Hediff components (e.g. ZeroWill).");
            listing.CheckboxLabeled("Log Jobs", ref settings.logJobs, "Traces for the custom 'Approach and Interact' jobs.");
            listing.CheckboxLabeled("Log UI", ref settings.logUI, "Traces for the UIs added by the mod.");

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "The Samhaphage";
    }
}