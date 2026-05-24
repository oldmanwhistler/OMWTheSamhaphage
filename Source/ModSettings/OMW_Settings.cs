using System;
using System.Collections.Generic;
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
        public bool logMutation = false;

        public bool disableGeneBlacklist = false;
        public float multSample = 0.5f;
        public float multCompress = 0.1f;
        public float multHarrow = 1.5f;
        public float multRetune = 0.5f;
        public float multCrosstalk = 0.5f;
        public float multScrub = 0.5f;
        public float multAttenuate = 1.0f;
        public float multBootleg = 10.0f;

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
            Scribe_Values.Look(ref logMutation, "logMutation", false);

            Scribe_Values.Look(ref disableGeneBlacklist, "disableGeneBlacklist", false);

            Scribe_Values.Look(ref multSample, "multSample", 0.5f);
            Scribe_Values.Look(ref multCompress, "multCompress", 0.1f);
            Scribe_Values.Look(ref multHarrow, "multHarrow", 1.5f);
            Scribe_Values.Look(ref multRetune, "multRetune", 0.5f);
            Scribe_Values.Look(ref multCrosstalk, "multCrosstalk", 0.5f);
            Scribe_Values.Look(ref multScrub, "multScrub", 0.5f);
            Scribe_Values.Look(ref multAttenuate, "multAttenuate", 1.0f);
            Scribe_Values.Look(ref multBootleg, "multBootleg", 10.0f);

            Scribe_Values.Look(ref gainFlatten, "gainFlatten", 3.0f);
            Scribe_Values.Look(ref gainMute, "gainMute", 20.0f);
            Scribe_Values.Look(ref gainScrub, "gainScrub", 1.5f);

            Scribe_Values.Look(ref resonanceMax, "resonanceMax", 200f);
            Scribe_Values.Look(ref NullThrumUtility.descMode, "descMode", NullThrumDescriptionMode.DescriptionSimple);
        }

        public void Reset()
        {
            logAbilities = false;
            logAnomaly = false;
            logCompAbilityEffect = false;
            logGenes = false;
            logResonance = false;
            logHediffs = false;
            logJobs = false;
            logUI = false;
            logMutation = false;

            disableGeneBlacklist = false;
            multSample = 0.5f;
            multCompress = 0.1f;
            multHarrow = 1.5f;
            multRetune = 0.5f;
            multCrosstalk = 0.5f;
            multScrub = 0.5f;
            multAttenuate = 1.0f;
            multBootleg = 10.0f;

            gainFlatten = 3.0f;
            gainMute = 20.0f;
            gainScrub = 1.5f;

            resonanceMax = 200f;
            NullThrumUtility.descMode = NullThrumDescriptionMode.DescriptionSimple;
        }
    }

    [StaticConstructorOnStartup]
    public class OMW_Mod : Mod
    {
        private enum SettingsTab
        {
            Main,
            GameBalance,
            Debugging
        }

        public static OMW_Settings settings;

        public string Prefix = "[SAMHAPHAGE-SETTINGS]";

        private Vector2 scrollPosition = Vector2.zero;
        private SettingsTab selectedTab = SettingsTab.Main;

        public OMW_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<OMW_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // Tab headers
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("Main", () => { selectedTab = SettingsTab.Main; scrollPosition = Vector2.zero; }, selectedTab == SettingsTab.Main),
                new TabRecord("Game Balance", () => { selectedTab = SettingsTab.GameBalance; scrollPosition = Vector2.zero; }, selectedTab == SettingsTab.GameBalance),
                new TabRecord("Debugging", () => { selectedTab = SettingsTab.Debugging; scrollPosition = Vector2.zero; }, selectedTab == SettingsTab.Debugging)
            };

            Rect tabRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            tabRect.yMin += 40f; // Make space for tab labels
            TabDrawer.DrawTabs(tabRect, tabs);

            // Define a view rectangle that is taller than the window to enable scrolling.
            float viewHeight = selectedTab == SettingsTab.GameBalance ? 1000f : 600f;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 30f, viewHeight);

            Widgets.BeginScrollView(tabRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            switch (selectedTab)
            {
                case SettingsTab.Main:
                    listing.Gap();
                    listing.Label(
                        "Gene Blacklist"
                            .Colorize(Color.yellow));
                   
                    listing.CheckboxLabeled("Disable Gene Blacklist", ref settings.disableGeneBlacklist,
                        "If checkmarked, no genes will be blacklisted.  You have to click 'Regenerate Gene Blacklist' after changing this setting.");                    
                    listing.GapLine();
                    
                    listing.Label("Regenerate the gene blacklist after tweaking with other mods or changing the enable/disable (e.g.: Tweaks Galore, Gene Blacklist)");
                    if (listing.ButtonText("Regenerate Gene Blacklist"))
                    {
                        OMW_BlacklistGenes.RebuildBlacklist();
                        Messages.Message($"{Prefix} Gene blacklist regenerated, debugging CSV exported, blacklist xenotype for Genetic Drift exported.", MessageTypeDefOf.TaskCompletion, false);
                    }
                    listing.GapLine();

                    listing.Label("Narrative Experience".Colorize(Color.yellow));
                    string currentModeLabel = NullThrumUtility.descMode.ToString().Replace("Description", "");
                    if (listing.ButtonTextLabeled("Ability Description Mode", currentModeLabel))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (NullThrumDescriptionMode mode in Enum.GetValues(typeof(NullThrumDescriptionMode)))
                        {
                            string label = mode.ToString().Replace("Description", "");
                            options.Add(new FloatMenuOption(label, () => 
                            {
                                NullThrumUtility.descMode = mode;
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    listing.Label("<color=gray><size=10>Intro: Switches from Simple to Lore after 400 uses.\nSimple: Mechanical/Technical descriptions.\nLore: Flavor/In-universe descriptions.</size></color>");
                    listing.GapLine();

                    listing.Gap(20f);
                    if (listing.ButtonText("Reset to Defaults"))
                    {
                        settings.Reset();
                        OMW_BlacklistGenes.RebuildBlacklist(); // Rebuild blacklist as its setting might have changed
                        Messages.Message($"{Prefix} All settings reset to default values.", MessageTypeDefOf.TaskCompletion, false);
                    }
                    break;

                case SettingsTab.GameBalance:
                    listing.Gap();
                    listing.Label($"Maximum Resonance Capacity: {settings.resonanceMax:F0}");
                    settings.resonanceMax = listing.Slider(settings.resonanceMax, 50f, 1000f);
                    listing.GapLine();
                    listing.Label("Ability Resonance Gene Multipliers".Colorize(Color.yellow));
                    listing.Label("Adjust the resonance spend multiplier for specific abilities. It multiplies the base gene value.");           
                    settings.multRetune = DrawMultiplierSlider(listing, NullThrumAbilityType.Retune, settings.multRetune);
                    settings.multCompress = DrawMultiplierSlider(listing, NullThrumAbilityType.Compress, settings.multCompress);
                    settings.multHarrow = DrawMultiplierSlider(listing, NullThrumAbilityType.Harrow, settings.multHarrow);
                    settings.multCrosstalk = DrawMultiplierSlider(listing, NullThrumAbilityType.Crosstalk, settings.multCrosstalk);
                    settings.multSample = DrawMultiplierSlider(listing, NullThrumAbilityType.Sample, settings.multSample);
                    listing.Label("Adjust how resonance gained multiplier for specific abilities. It multiplies the base gene value.");
                    settings.multScrub = DrawMultiplierSlider(listing, NullThrumAbilityType.Scrub, settings.multScrub);
                    settings.multAttenuate =
                        DrawMultiplierSlider(listing, NullThrumAbilityType.Attenuate, settings.multAttenuate);
                    listing.Gap();

                    listing.Label("Ability Resonance Flat Rate".Colorize(Color.yellow));
                    listing.Label("Adjust the resonance spent from specific abilities. Flat rate.");
                    settings.multBootleg =
                        DrawMultiplierSlider(listing, NullThrumAbilityType.Bootleg, settings.multBootleg);
                    listing.Label("Adjust the resonance gained from specific abilities. Flat rate.");
                    settings.gainFlatten = DrawValueSlider(listing, NullThrumAbilityType.Flatten, settings.gainFlatten, 0f, 20f);
                    settings.gainMute = DrawValueSlider(listing, NullThrumAbilityType.Mute, settings.gainMute, 0f, 100f);
                    settings.gainScrub = DrawValueSlider(listing, NullThrumAbilityType.Scrub, settings.gainScrub, 0f, 10f);
                    break;

                case SettingsTab.Debugging:
                    listing.Gap();
                    listing.Label("Resonance Calculations".Colorize(Color.yellow));
                    if (listing.ButtonText("Export CSV for debugging resonance calculations"))
                    {
                        ExportGeneReport();
                    }

                    listing.Label("Gene Blacklist".Colorize(Color.yellow));
                    if (listing.ButtonText("Export CSV for debugging gene blacklists"))
                    {
                        OMW_BlacklistGenes.ExportBlacklistReport();
                    }

                    listing.Gap();                    
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
                    listing.CheckboxLabeled("Log Mutation", ref settings.logMutation, "Traces for genes getting added by the Random Mutation genes.");
                    break;
            }
            listing.End();
            Widgets.EndScrollView();
        }

        private float DrawMultiplierSlider(Listing_Standard listing, NullThrumAbilityType abilityType, float value)
        {
            string label = NullThrumUtility.ToString(abilityType);
            string desc = NullThrumUtility.DescriptionSimple(abilityType);
            listing.Label($"{label}: {value:F2}");
            listing.Label($"<size=10>    {desc}</size>");
            return listing.Slider(value, 0f, 10f);
        }

        private float DrawValueSlider(Listing_Standard listing, NullThrumAbilityType abilityType, float value, float min, float max)
        {
            string label = NullThrumUtility.ToString(abilityType);
            string desc = NullThrumUtility.DescriptionSimple(abilityType);
            listing.Label($"{label}: {value:F2}");
            listing.Label($"<size=10>    {desc}</size>");
            return listing.Slider(value, min, max);
        }

        private void ExportGeneReport()
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