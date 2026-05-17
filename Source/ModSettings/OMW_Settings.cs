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

        public string Prefix = "[SAMHAPHAGE-SETTINGS]";

        public OMW_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<OMW_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

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