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
        
        public float resonanceMax = 200f;

        public NullThrumAbilities abilityValue = new NullThrumAbilities();
        private NullThrumAbilities abilityDefault = new NullThrumAbilities();


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
            Scribe_Values.Look(ref resonanceMax, "ResonanceMax", 200f);
            Scribe_Values.Look(ref abilityValue.flatten.value, "Flatten", abilityDefault.flatten.value);
            Scribe_Values.Look(ref abilityValue.scrub.value, "Scrub", abilityDefault.scrub.value);
            Scribe_Values.Look(ref abilityValue.scrubCarcinoma.value, "Scrub Carcinoma", abilityDefault.scrubCarcinoma.value);
            Scribe_Values.Look(ref abilityValue.retune.value, "Retune", abilityDefault.retune.value);
            Scribe_Values.Look(ref abilityValue.compress.value, "Compress", abilityDefault.compress.value);
            Scribe_Values.Look(ref abilityValue.harrow.value, "Harrow", abilityDefault.harrow.value);
            Scribe_Values.Look(ref abilityValue.transpose.value, "Transpose", abilityDefault.transpose.value);
            Scribe_Values.Look(ref abilityValue.infest.value, "Infest", abilityDefault.infest.value);
            Scribe_Values.Look(ref abilityValue.enwomb.value, "Enwomb", abilityDefault.enwomb.value);
            Scribe_Values.Look(ref abilityValue.unmute.value, "Unmute", abilityDefault.unmute.value);
            Scribe_Values.Look(ref abilityValue.mute.value, "Mute", abilityDefault.mute.value);
            Scribe_Values.Look(ref abilityValue.attenuate.value, "Attenuate", abilityDefault.attenuate.value);
            Scribe_Values.Look(ref abilityValue.sample.value, "Sample", abilityDefault.sample.value);
            Scribe_Values.Look(ref abilityValue.bootleg.value, "Bootleg", abilityDefault.bootleg.value);
            Scribe_Values.Look(ref abilityValue.crosstalk.value, "Crosstalk", abilityDefault.crosstalk.value);
            Scribe_Values.Look(ref abilityValue.resurrect.value, "Resurrect", abilityDefault.resurrect.value);
            Scribe_Values.Look(ref abilityValue.stun.value, "Stun", abilityDefault.stun.value);
            Scribe_Values.Look(ref abilityValue.hallowbound.value, "Hallowbound", abilityDefault.hallowbound.value);
            
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
            abilityValue = new NullThrumAbilities();
            abilityDefault = new NullThrumAbilities();            
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
            float viewHeight = selectedTab == SettingsTab.GameBalance ? 1800f : 600f;
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
                    listing.Label("<color=gray><size=10>Simple: Mechanical/Technical descriptions.\nLore: Flavor/In-universe descriptions.</size></color>");
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
                    listing.Label("Resonance Gains (Credits)".Colorize(Color.yellow));
                    listing.Label("Adjust resonance acquired from harvesting or sacrifices.");
                    DrawValueSlider(listing, settings.abilityValue.flatten);
                    DrawValueSlider(listing, settings.abilityValue.scrubCarcinoma);
                    DrawValueSlider(listing, settings.abilityValue.mute);
                    listing.Gap();
                    DrawValueSlider(listing, settings.abilityValue.scrub);
                    DrawValueSlider(listing, settings.abilityValue.attenuate);

                    // infest is free
                    // settings.abilityValue.infest.value = DrawValueSlider(listing, settings.abilityValue.infest);

                    listing.GapLine();
                    listing.Label("Resonance Costs (Debits)".Colorize(Color.yellow));
                    listing.Label("Adjust the resonance spend (offset or multiplier) for specific abilities.");
                    DrawValueSlider(listing, settings.abilityValue.unmute);
                    DrawValueSlider(listing, settings.abilityValue.bootleg);
                    DrawValueSlider(listing, settings.abilityValue.transpose);
                    DrawValueSlider(listing, settings.abilityValue.stun);
                    DrawValueSlider(listing, settings.abilityValue.resurrect);
                    DrawValueSlider(listing, settings.abilityValue.enwomb);
                    DrawValueSlider(listing, settings.abilityValue.hallowbound);
                    listing.Gap();
                    DrawValueSlider(listing, settings.abilityValue.retune);
                    DrawValueSlider(listing, settings.abilityValue.compress);
                    DrawValueSlider(listing, settings.abilityValue.crosstalk);
                    DrawValueSlider(listing, settings.abilityValue.sample);
                    DrawValueSlider(listing, settings.abilityValue.harrow);
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

        private void DrawValueSlider(Listing_Standard listing, NullThrumAbilityProps abilityProps)
        {
            string label = NullThrumUtility.ToString(abilityProps.abilityType);
            string desc = $"{abilityProps.ToString()}. {NullThrumUtility.DescriptionSimple(abilityProps.abilityType)}";
            listing.Label($"{label}: {abilityProps.value:F2}");
            listing.Label($"<size=10>    {desc}</size>");
            abilityProps.value = listing.Slider(abilityProps.value, abilityProps.min, abilityProps.max);
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