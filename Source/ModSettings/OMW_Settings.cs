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
        public bool logAbilities;
        public bool logAnomaly;
        public bool logCompAbilityEffect;
        public bool logGenes;
        public bool logResonance;
        public bool logHediffs;
        public bool logJobs;
        public bool logUI;
        public bool logMutation;
        public bool logSelection;
        public bool disableDissonance;

        public bool disableGeneBlacklist;
        public bool disableTraitBlacklist;
        
        public float resonanceMax = DefaultResonanceMax;
        private const float DefaultResonanceMax = 1000f;

        public NullThrumAbilities abilityValue = new NullThrumAbilities();

        public float complexityMultiplierHallowbound = 1.5f;
        public int complexityHallowbound => Mathf.RoundToInt(complexityMultiplierHallowbound * OMWGenes.CalculateComplexity(OMW_XenotypeDefOf.omw_hallowbound));
        public float complexityMultiplierSamhaphage = 1.5f;
        public int complexitySamhaphage =>
            Mathf.RoundToInt(complexityMultiplierSamhaphage * OMWGenes.CalculateComplexity(OMW_XenotypeDefOf.omw_samhaphage));

        

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
            Scribe_Values.Look(ref logMutation, "logSelection", false);
            Scribe_Values.Look(ref disableDissonance, "disableDissonance", false);
            Scribe_Values.Look(ref disableGeneBlacklist, "disableGeneBlacklist", false);
            Scribe_Values.Look(ref disableTraitBlacklist, "disableTraitBlacklist", false);            
            Scribe_Values.Look(ref resonanceMax, "ResonanceMax", DefaultResonanceMax);
            Scribe_Values.Look(ref complexityMultiplierHallowbound, "complexityMultiplierHallowbound", 1.5f);
            Scribe_Values.Look(ref complexityMultiplierSamhaphage, "complexityMultiplierSamhaphage", 1.5f);

            // Use a fresh instance as the default reference for Scribe
            NullThrumAbilities defaults = new NullThrumAbilities();
            Scribe_Values.Look(ref abilityValue.flatten.value, "Flatten", defaults.flatten.value);
            Scribe_Values.Look(ref abilityValue.scrub.value, "Scrub", defaults.scrub.value);
            Scribe_Values.Look(ref abilityValue.scrubCarcinoma.value, "Scrub Carcinoma", defaults.scrubCarcinoma.value);
            Scribe_Values.Look(ref abilityValue.retune.value, "Retune", defaults.retune.value);
            Scribe_Values.Look(ref abilityValue.compress.value, "Compress", defaults.compress.value);
            Scribe_Values.Look(ref abilityValue.harrow.value, "Harrow", defaults.harrow.value);
            Scribe_Values.Look(ref abilityValue.transpose.value, "Transpose", defaults.transpose.value);
            Scribe_Values.Look(ref abilityValue.infest.value, "Infest", defaults.infest.value);
            Scribe_Values.Look(ref abilityValue.enwomb.value, "Enwomb", defaults.enwomb.value);
            Scribe_Values.Look(ref abilityValue.unmute.value, "Unmute", defaults.unmute.value);
            Scribe_Values.Look(ref abilityValue.mute.value, "Mute", defaults.mute.value);
            Scribe_Values.Look(ref abilityValue.attenuate.value, "Attenuate", defaults.attenuate.value);
            Scribe_Values.Look(ref abilityValue.sample.value, "Sample", defaults.sample.value);
            Scribe_Values.Look(ref abilityValue.bootleg.value, "Bootleg", defaults.bootleg.value);
            Scribe_Values.Look(ref abilityValue.crosstalk.value, "Crosstalk", defaults.crosstalk.value);
            Scribe_Values.Look(ref abilityValue.resurrect.value, "Resurrect", defaults.resurrect.value);
            Scribe_Values.Look(ref abilityValue.stun.value, "Stun", defaults.stun.value);
            Scribe_Values.Look(ref abilityValue.hallowbound.value, "Hallowbound", defaults.hallowbound.value);
            
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
            logSelection = false;
            disableDissonance = false;
            disableGeneBlacklist = false;
            disableTraitBlacklist = false;
            abilityValue = new NullThrumAbilities();
            resonanceMax = DefaultResonanceMax;
            complexityMultiplierHallowbound = 1.5f;
            complexityMultiplierSamhaphage = 1.5f;
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
                        "Blacklists"
                            .Colorize(Color.yellow));
                   
                    listing.CheckboxLabeled("Disable Gene Blacklist", ref settings.disableGeneBlacklist,
                        "If checkmarked, no genes will be blacklisted.  You have to click 'Regenerate Genes Blacklist' after changing this setting.");
                    listing.CheckboxLabeled("Disable Trait Blacklist", ref settings.disableTraitBlacklist,
                        "If checkmarked, no traits will be blacklisted.  You have to click 'Regenerate Traits Blacklist' after changing this setting.");
                    listing.GapLine();
                    
                    listing.Label("Regenerate the gene/trait blacklist after tweaking with other mods or changing the enable/disable (e.g.: Tweaks Galore, Gene Blacklist)");
                    if (listing.ButtonText("Regenerate Gene/Trait Blacklists"))
                    {
                        OMW_BlacklistGenes.RebuildBlacklist();
                        OMW_BlacklistTraits.RebuildBlacklist();
                        OMW_BlacklistGenes.ExportBlacklistGeneReport();                        
                        OMW_BlacklistTraits.ExportBlacklistTraitReport();
                        Messages.Message($"{Prefix} Genes/Traits blacklist regenerated, debugging CSV exported, blacklist xenotype for Genetic Drift exported.", MessageTypeDefOf.TaskCompletion, false);
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
                    listing.Label($"Dissonance");
                    listing.CheckboxLabeled("Disable Dissonance", ref settings.disableDissonance,
                        "Dissonance is a hediff used like 'genes regrowing'");
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
                        ExportReport.ExportReportsResonance();
                    }

                    listing.Label("Blacklists".Colorize(Color.yellow));
                    if (listing.ButtonText("Export CSV for debugging gene blacklists"))
                    {
                        OMW_BlacklistGenes.ExportBlacklistGeneReport();
                        OMW_BlacklistTraits.ExportBlacklistTraitReport();
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
                    listing.CheckboxLabeled("Log Selection", ref settings.logSelection, "Traces for genes selector.");
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

        public override string SettingsCategory() => "The Samhaphage";
    }
}