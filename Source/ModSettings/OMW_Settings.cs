using System.IO;
using System.Text;
using RimWorld;
using UnityEngine;
using System.Xml.Linq; // Added for XML generation
using System.Linq; // Added for LINQ operations
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

        public float multSample = 0.5f;
        public float multCompress = 0.1f;
        public float multHarrow = 1.5f;
        public float multRetune = 0.5f;
        public float multCrosstalk = 0.5f;
        public float multScrub = 0.5f;
        public float multAttenuate = 1.0f;

        public float gainFlatten = 3.0f;
        public float gainMute = 20.0f;
        public float gainScrub = 1.5f;

        public float resonanceMax = 200f;

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

            Scribe_Values.Look(ref multSample, "multSample", 0.5f);
            Scribe_Values.Look(ref multCompress, "multCompress", 0.1f);
            Scribe_Values.Look(ref multHarrow, "multHarrow", 1.5f);
            Scribe_Values.Look(ref multRetune, "multRetune", 0.5f);
            Scribe_Values.Look(ref multCrosstalk, "multCrosstalk", 0.5f);
            Scribe_Values.Look(ref multScrub, "multScrub", 0.5f);
            Scribe_Values.Look(ref multAttenuate, "multAttenuate", 1.0f);

            Scribe_Values.Look(ref gainFlatten, "gainFlatten", 3.0f);
            Scribe_Values.Look(ref gainMute, "gainMute", 20.0f);
            Scribe_Values.Look(ref gainScrub, "gainScrub", 1.5f);

            Scribe_Values.Look(ref resonanceMax, "resonanceMax", 200f);
        }
    }

    public class OMW_Mod : Mod
    {
        public static OMW_Settings settings;

        public string Prefix = "[SAMHAPHAGE-SETTINGS]";

        private Vector2 scrollPosition = Vector2.zero;

        public OMW_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<OMW_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // Define a view rectangle that is taller than the window to enable scrolling.
            // The width is slightly reduced to prevent the horizontal scrollbar from appearing.
            Rect viewRect = new Rect(0f, 0f, inRect.width - 30f, 1050f);

            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.Label("Resonance Calculations".Colorize(Color.yellow));
            if (listing.ButtonText("Export CSV for debugging resonance calculations"))
            {
                ExportGeneReports();
            }
            listing.Gap();

            listing.Label("Regenerate the gene blacklist after tweaking with other mods (e.g.: Tweaks Galore, Gene Blacklist)".Colorize(Color.yellow));
            if (listing.ButtonText("Regenerate Gene Blacklist"))
            {
                OMW_BlacklistGenes.RebuildBlacklist();
                Messages.Message($"{Prefix} Gene blacklist regenerated, debugging CSV exported, blacklist xenotype for Genetic Drift exported.", MessageTypeDefOf.TaskCompletion, false);
            }
            listing.Gap();

            listing.Label("Ability Resonance Multipliers".Colorize(Color.yellow));
            listing.Label("Adjust how expensive or rewarding specific abilities are.");
            
            settings.multRetune = DrawMultiplierSlider(listing, "Retune (Debit)", settings.multRetune);
            settings.multCompress = DrawMultiplierSlider(listing, "Compress (Debit)", settings.multCompress);
            settings.multHarrow = DrawMultiplierSlider(listing, "Harrow (Debit)", settings.multHarrow);
            settings.multCrosstalk = DrawMultiplierSlider(listing, "Crosstalk (Debit)", settings.multCrosstalk);
            settings.multSample = DrawMultiplierSlider(listing, "Sample (Credit)", settings.multSample);
            settings.multScrub = DrawMultiplierSlider(listing, "Scrub (Credit)", settings.multScrub);
            settings.multAttenuate = DrawMultiplierSlider(listing, "Attenuate (Credit)", settings.multAttenuate);
            listing.Gap();

            listing.Label("Ability Resonance Gains".Colorize(Color.yellow));
            listing.Label("Adjust the amount of resonance harvested from specific actions.");

            settings.resonanceMax = DrawValueSlider(listing, "Maximum Resonance Capacity", settings.resonanceMax, 50f, 1000f);
            listing.GapLine();
            settings.gainFlatten = DrawValueSlider(listing, "Flatten Gain", settings.gainFlatten, 0f, 20f);
            settings.gainMute = DrawValueSlider(listing, "Mute Gain (per level)", settings.gainMute, 0f, 100f);
            settings.gainScrub = DrawValueSlider(listing, "Scrub Gain (per carcinoma)", settings.gainScrub, 0f, 10f);
            listing.Gap();

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
            Widgets.EndScrollView();

            base.DoSettingsWindowContents(inRect);
        }

        private float DrawMultiplierSlider(Listing_Standard listing, string label, float value)
        {
            listing.Label($"{label}: {value:F2}");
            return listing.Slider(value, 0f, 10f);
        }

        private float DrawValueSlider(Listing_Standard listing, string label, float value, float min, float max)
        {
            listing.Label($"{label}: {value:F2}");
            return listing.Slider(value, min, max);
        }

        private void ExportGeneReports()
        {
            try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "OMW_Samhaphage_Report_Resonance.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine("DEFNAME,LABEL,COMPLEXITY,METABOLISM,ARCHITE,CATEGORY,CATEGORYDEF,RESONANCEVALUE,RESONANCEARCHITE,RESONANCECOMPLEXITY,RESONANCEMETABOLISM,CANGENERATEINGENESET,ABILITIES,DESCRIPTION");

                foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
                {
                    string label = gene.label?.Replace("\"", "\"\"") ?? "";
                    string cat = gene.displayCategory?.label?.Replace("\"", "\"\"") ?? "None";
                    string catDef = gene.displayCategory?.defName ?? "None";
                    
                    float totalResonanceValue = ResonanceUtility.GeneResonanceValue(gene);
                    float resonanceArchite = ResonanceUtility.GeneResonanceValueArchite(gene);
                    float resonanceComplexity = ResonanceUtility.GeneResonanceValueComplexity(gene);
                    float resonanceMetabolism = ResonanceUtility.GeneResonanceValueMetabolism(gene);
                    string abilities = (gene.abilities != null && gene.abilities.Count > 0) 
                        ? string.Join("|", gene.abilities.ConvertAll(a => a.defName)) 
                        : "";
                    string desc = gene.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ?? "";

                    sb.AppendLine($"\"{gene.defName}\",\"{label}\",{gene.biostatCpx},{gene.biostatMet},{gene.biostatArc},\"{cat}\",\"{catDef}\",{totalResonanceValue},{resonanceArchite},{resonanceComplexity},{resonanceMetabolism},{gene.canGenerateInGeneSet},\"{abilities}\",\"{desc}\"");
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported resonance report to {path}");
                Messages.Message($"{Prefix} Exported resonance report to {path}", MessageTypeDefOf.TaskCompletion, false);
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export genes: " + ex.Message);
            }
        }

        public override string SettingsCategory() => "The Samhaphage";
    }
}