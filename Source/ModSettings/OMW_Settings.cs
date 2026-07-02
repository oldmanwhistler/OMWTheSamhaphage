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
        public bool logKill;
        public bool logCompAbilityEffect;
        public bool logGenes;
        public bool logResonance;
        public bool logHediffs;
        public bool logJobs;
        public bool logUI;
        public bool logMutation;
        public bool logSelection;
        public bool logHediff;
        public bool disableDissonance;

        public bool disableGeneBlacklist;
        public bool disableTraitBlacklist;

        public NullThrumDifficultyPreset limitDifficulties;
        public NullThrumXenotypeLimitPercentage limitPercentage = new NullThrumXenotypeLimitPercentage(NullThrumDifficultyPreset.DifficultyMedium);
        public NullThrumXenotypeLimitMetabolism limitMetabolism = new NullThrumXenotypeLimitMetabolism(NullThrumDifficultyPreset.DifficultyMedium);
        public NullThrumXenotypeLimitTraits limitTraits = new NullThrumXenotypeLimitTraits(NullThrumDifficultyPreset.DifficultyMedium);
        public NullThrumXenotypeMultiplierComplexity multiplierComplexity = new NullThrumXenotypeMultiplierComplexity(NullThrumDifficultyPreset.DifficultyMedium);
        public float resonanceMax = DefaultResonanceMax;
        private const float DefaultResonanceMax = 1000f;

        public NullThrumAbilities abilityValue = new NullThrumAbilities();

        public float complexityMultiplierHallowbound = 1.5f;

        public int complexityHallowbound => Mathf.RoundToInt(complexityMultiplierHallowbound *
                                                             OMWGenes.CalculateComplexity(OMW_XenotypeDefOf
                                                                 .omw_hallowbound, true));

        public float complexityMultiplierSamhaphage = 1.5f;

        public int complexitySamhaphage =>
            Mathf.RoundToInt(complexityMultiplierSamhaphage *
                             OMWGenes.CalculateComplexity(OMW_XenotypeDefOf.omw_samhaphage, true));

        
        public NullThrumAbilityMenuType abilityMenuType;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref logAbilities, "logAbilities", false);
            Scribe_Values.Look(ref logKill, "logKill", false);
            Scribe_Values.Look(ref logCompAbilityEffect, "logCompAbilityEffect", false);
            Scribe_Values.Look(ref logGenes, "logGenes", false);
            Scribe_Values.Look(ref logResonance, "logResonance", false);
            Scribe_Values.Look(ref logHediffs, "logHediffs", false);
            Scribe_Values.Look(ref logJobs, "logJobs", false);
            Scribe_Values.Look(ref logUI, "logUI", false);
            Scribe_Values.Look(ref logMutation, "logMutation", false);
            Scribe_Values.Look(ref logSelection, "logSelection", false);
            Scribe_Values.Look(ref logHediff, "logHediff", false);
            Scribe_Values.Look(ref disableDissonance, "disableDissonance", false);
            Scribe_Values.Look(ref disableGeneBlacklist, "disableGeneBlacklist", false);
            Scribe_Values.Look(ref disableTraitBlacklist, "disableTraitBlacklist", false);
            Scribe_Values.Look(ref limitDifficulties, "limitDifficulties", NullThrumDifficultyPreset.DifficultyMedium);
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
            Scribe_Values.Look(ref abilityValue.amplify.value, "Amplify", defaults.amplify.value);
            Scribe_Values.Look(ref abilityValue.excise.value, "Excise", defaults.excise.value);
            Scribe_Values.Look(ref abilityValue.render.value, "Render", defaults.render.value);
            Scribe_Values.Look(ref abilityValue.dub.value, "Dub", defaults.dub.value);

            NullThrumXenotypeLimitPercentage defaultsPercentage = new NullThrumXenotypeLimitPercentage(limitDifficulties);
            Scribe_Values.Look(ref limitPercentage.enabled, "LimitPercEnabled", defaultsPercentage.enabled);
            Scribe_Values.Look(ref limitPercentage.fluxspawn, "LimitPercFluxSpawn", defaultsPercentage.fluxspawn);
            Scribe_Values.Look(ref limitPercentage.echovessel, "LimitPercEchoVessel", defaultsPercentage.echovessel);
            Scribe_Values.Look(ref limitPercentage.cradlemold, "LimitPercCradleMold", defaultsPercentage.cradlemold);
            Scribe_Values.Look(ref limitPercentage.hallowbound, "LimitPercHallowbound", defaultsPercentage.hallowbound);
            Scribe_Values.Look(ref limitPercentage.samhaphage, "LimitPercSamhaphage", defaultsPercentage.samhaphage);
            Scribe_Values.Look(ref limitPercentage.sovereign_stillness, "LimitPercSovereignStillness",
                defaultsPercentage.sovereign_stillness);

            NullThrumXenotypeLimitMetabolism defaultsMetabolism = new NullThrumXenotypeLimitMetabolism(limitDifficulties);
            Scribe_Values.Look(ref limitMetabolism.enabled, "LimitMetabolismEnabled", defaultsMetabolism.enabled);
            Scribe_Values.Look(ref limitMetabolism.fluxspawn, "LimitMetabolismFluxSpawn", defaultsMetabolism.fluxspawn);
            Scribe_Values.Look(ref limitMetabolism.echovessel, "LimitMetabolismEchoVessel",
                defaultsMetabolism.echovessel);
            Scribe_Values.Look(ref limitMetabolism.cradlemold, "LimitMetabolismCradleMold",
                defaultsMetabolism.cradlemold);
            Scribe_Values.Look(ref limitMetabolism.hallowbound, "LimitMetabolismHallowbound",
                defaultsMetabolism.hallowbound);
            Scribe_Values.Look(ref limitMetabolism.samhaphage, "LimitMetabolismSamhaphage",
                defaultsMetabolism.samhaphage);
            Scribe_Values.Look(ref limitMetabolism.sovereign_stillness, "LimitMetabolismSovereignStillness",
                defaultsMetabolism.sovereign_stillness);

            NullThrumXenotypeLimitTraits defaultsTraits = new NullThrumXenotypeLimitTraits(limitDifficulties);
            Scribe_Values.Look(ref limitTraits.enabled, "LimitTraitsEnabled", defaultsTraits.enabled);
            Scribe_Values.Look(ref limitTraits.fluxspawn, "LimitTraitsFluxSpawn", defaultsTraits.fluxspawn);
            Scribe_Values.Look(ref limitTraits.echovessel, "LimitTraitsEchoVessel", defaultsTraits.echovessel);
            Scribe_Values.Look(ref limitTraits.cradlemold, "LimitTraitsCradleMold", defaultsTraits.cradlemold);
            Scribe_Values.Look(ref limitTraits.hallowbound, "LimitTraitsHallowbound", defaultsTraits.hallowbound);
            Scribe_Values.Look(ref limitTraits.samhaphage, "LimitTraitsSamhaphage", defaultsTraits.samhaphage);
            Scribe_Values.Look(ref limitTraits.sovereign_stillness, "LimitTraitsSovereignStillness",
                defaultsTraits.sovereign_stillness);

            NullThrumXenotypeMultiplierComplexity defaultsComplexity = new NullThrumXenotypeMultiplierComplexity(limitDifficulties);
            Scribe_Values.Look(ref defaultsComplexity.fluxspawn, "ComplexityFluxSpawn", defaultsComplexity.fluxspawn);
            Scribe_Values.Look(ref defaultsComplexity.echovessel, "ComplexityEchoVessel", defaultsComplexity.echovessel);
            Scribe_Values.Look(ref defaultsComplexity.cradlemold, "ComplexityCradleMold", defaultsComplexity.cradlemold);
            Scribe_Values.Look(ref defaultsComplexity.hallowbound, "ComplexityHallowbound", defaultsComplexity.hallowbound);
            Scribe_Values.Look(ref defaultsComplexity.samhaphage, "ComplexitySamhaphage", defaultsComplexity.samhaphage);
            Scribe_Values.Look(ref defaultsComplexity.sovereign_stillness, "ComplexitySovereignStillness", defaultsComplexity.sovereign_stillness);

            Scribe_Values.Look(ref NullThrumUtility.descMode, "descMode", NullThrumDescriptionMode.DescriptionSimple);
            Scribe_Values.Look(ref abilityMenuType, "abilityMenuType", NullThrumAbilityMenuType.ByXenotype);
        }

        // This needs to set all of the default values
        public void Reset()
        {
            logAbilities = false;
            logKill = false;
            logCompAbilityEffect = false;
            logGenes = false;
            logResonance = false;
            logHediffs = false;
            logJobs = false;
            logUI = false;
            logMutation = false;
            logSelection = false;
            logHediff = false;
            disableDissonance = false;
            disableGeneBlacklist = false;
            disableTraitBlacklist = false;
            limitDifficulties = NullThrumDifficultyPreset.DifficultyMedium;
            limitPercentage.SetLimitDefaults(limitDifficulties);
            limitMetabolism.SetLimitDefaults(limitDifficulties);
            limitTraits.SetLimitDefaults(limitDifficulties);
            multiplierComplexity.SetMultiplierDefaults(limitDifficulties);
            abilityValue = new NullThrumAbilities();
            resonanceMax = DefaultResonanceMax;
            complexityMultiplierHallowbound = 1.5f;
            complexityMultiplierSamhaphage = 1.5f;
            NullThrumUtility.descMode = NullThrumDescriptionMode.DescriptionSimple;
            abilityMenuType = NullThrumAbilityMenuType.ByXenotype;
        }

        public void SetNullThrumDifficultyPreset(NullThrumDifficultyPreset preset)
        {
            limitDifficulties = preset;
            limitPercentage.SetLimitDefaults(preset);
            limitMetabolism.SetLimitDefaults(preset);
            limitTraits.SetLimitDefaults(preset);
            multiplierComplexity.SetMultiplierDefaults(preset);
        }
    }

    [StaticConstructorOnStartup]
    public class OMW_Mod : Mod
    {
        private enum SettingsTab
        {
            Main,
            UI,
            Limits,
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
                new TabRecord("Main", () =>
                {
                    selectedTab = SettingsTab.Main;
                    scrollPosition = Vector2.zero;
                }, selectedTab == SettingsTab.Main),
                new TabRecord("UI", () =>
                {
                    selectedTab = SettingsTab.UI;
                    scrollPosition = Vector2.zero;
                }, selectedTab == SettingsTab.UI),
                new TabRecord("Game Balance", () =>
                {
                    selectedTab = SettingsTab.GameBalance;
                    scrollPosition = Vector2.zero;
                }, selectedTab == SettingsTab.GameBalance),
                new TabRecord("Limits", () =>
                {
                    selectedTab = SettingsTab.Limits;
                    scrollPosition = Vector2.zero;
                }, selectedTab == SettingsTab.Limits),
                new TabRecord("Debugging", () =>
                {
                    selectedTab = SettingsTab.Debugging;
                    scrollPosition = Vector2.zero;
                }, selectedTab == SettingsTab.Debugging)
            };

            Rect tabRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            tabRect.yMin += 40f; // Make space for tab labels
            TabDrawer.DrawTabs(tabRect, tabs);

            // Define a view rectangle that is taller than the window to enable scrolling.
            float viewHeight = selectedTab == SettingsTab.GameBalance
                ? 2000f
                : (selectedTab == SettingsTab.Limits ? 1500f : 600f);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 30f, viewHeight);

            Widgets.BeginScrollView(tabRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            switch (selectedTab)
            {
                case SettingsTab.Main:
                    listing.Gap();
                    listing.Label("Narrative Experience".Colorize(Color.yellow));
                    string currentModeLabel = NullThrumUtility.descMode.ToString().Replace("Description", "");
                    if (listing.ButtonTextLabeled("Ability Description Mode", currentModeLabel))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (NullThrumDescriptionMode mode in Enum.GetValues(typeof(NullThrumDescriptionMode)))
                        {
                            string label = mode.ToString().Replace("Description", "");
                            options.Add(new FloatMenuOption(label, () => { NullThrumUtility.descMode = mode; }));
                        }

                        Find.WindowStack.Add(new FloatMenu(options));
                    }

                    listing.GapLine();

                    listing.Label(
                        "<color=gray><size=10>Simple: Mechanical/Technical descriptions.\nLore: Flavor/In-universe descriptions.</size></color>");
                    listing.Gap();
                    listing.Label(
                        "Blacklists"
                            .Colorize(Color.yellow));

                    listing.CheckboxLabeled("Disable Gene Blacklist", ref settings.disableGeneBlacklist,
                        "If checkmarked, no genes will be blacklisted.  You have to click 'Regenerate Genes Blacklist' after changing this setting.");
                    listing.CheckboxLabeled("Disable Trait Blacklist", ref settings.disableTraitBlacklist,
                        "If checkmarked, no traits will be blacklisted.  You have to click 'Regenerate Traits Blacklist' after changing this setting.");
                    listing.GapLine();

                    listing.Label(
                        "Regenerate the gene/trait blacklist after tweaking with other mods or changing the enable/disable (e.g.: Tweaks Galore, Gene Blacklist)");
                    if (listing.ButtonText("Regenerate Gene/Trait Blacklists"))
                    {
                        OMW_BlacklistGenes.RebuildBlacklist();
                        OMW_BlacklistTraits.RebuildBlacklist();
                        OMW_BlacklistGenes.ExportBlacklistGeneReport();
                        OMW_BlacklistTraits.ExportBlacklistTraitReport();
                        Messages.Message(
                            $"{Prefix} Genes/Traits blacklist regenerated, debugging CSV exported, blacklist xenotype for Genetic Drift exported.",
                            MessageTypeDefOf.TaskCompletion, false);
                    }

                    listing.GapLine();

                    listing.Gap(20f);
                    if (listing.ButtonText("Reset to Defaults"))
                    {
                        settings.Reset();
                        OMW_BlacklistGenes.RebuildBlacklist(); // Rebuild blacklist as its setting might have changed
                        Messages.Message($"{Prefix} All settings reset to default values.",
                            MessageTypeDefOf.TaskCompletion, false);
                    }

                    break;

                case SettingsTab.UI:
                    listing.Gap();
                    if (listing.ButtonTextLabeled($"Ability Menu", $"{settings.abilityMenuType}"))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (NullThrumAbilityMenuType preset in Enum.GetValues(typeof(NullThrumAbilityMenuType)))
                        {
                            string label = preset.ToString();
                            options.Add(new FloatMenuOption(label,
                                () => { settings.abilityMenuType = preset; }));
                        }

                        Find.WindowStack.Add(new FloatMenu(options));
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
                    DrawValueSlider(listing, ref settings.abilityValue.flatten);
                    DrawValueSlider(listing, ref settings.abilityValue.amplify);
                    DrawValueSlider(listing, ref settings.abilityValue.excise);
                    DrawValueSlider(listing, ref settings.abilityValue.render);
                    DrawValueSlider(listing, ref settings.abilityValue.scrubCarcinoma);
                    DrawValueSlider(listing, ref settings.abilityValue.mute);
                    listing.Gap();
                    DrawValueSlider(listing, ref settings.abilityValue.scrub);
                    DrawValueSlider(listing, ref settings.abilityValue.attenuate);

                    // infest is free
                    // settings.abilityValue.infest.value = DrawValueSlider(listing, settings.abilityValue.infest);

                    listing.GapLine();
                    listing.Label("Resonance Costs (Debits)".Colorize(Color.yellow));
                    listing.Label("Adjust the resonance spend (offset or multiplier) for specific abilities.");
                    DrawValueSlider(listing, ref settings.abilityValue.unmute);
                    DrawValueSlider(listing, ref settings.abilityValue.bootleg);
                    DrawValueSlider(listing, ref settings.abilityValue.transpose);
                    DrawValueSlider(listing, ref settings.abilityValue.stun);
                    DrawValueSlider(listing, ref settings.abilityValue.resurrect);
                    DrawValueSlider(listing, ref settings.abilityValue.enwomb);
                    DrawValueSlider(listing, ref settings.abilityValue.hallowbound);
                    listing.Gap();
                    DrawValueSlider(listing, ref settings.abilityValue.retune);
                    DrawValueSlider(listing, ref settings.abilityValue.compress);
                    DrawValueSlider(listing, ref settings.abilityValue.crosstalk);
                    DrawValueSlider(listing, ref settings.abilityValue.sample);
                    DrawValueSlider(listing, ref settings.abilityValue.harrow);
                    DrawValueSlider(listing, ref settings.abilityValue.dub);
                    break;

                case SettingsTab.Limits:                    
                    
                    listing.Gap();
                    listing.Label("Genetic Complexity Threshold For Evolution (Amplify)".Colorize(Color.yellow));
                    listing.Label(
                        $"Required complexity for Hallowbound to Samhaphage to evolve: {settings.complexityMultiplierHallowbound:F2} (Target: {settings.complexityHallowbound})");
                    settings.complexityMultiplierHallowbound =
                        listing.Slider(settings.complexityMultiplierHallowbound, 0.5f, 5.0f);

                    listing.Label(
                        $"Required complexity for Samhaphage to Sovereign Stillness to evolve: {settings.complexityMultiplierSamhaphage:F2} (Target: {settings.complexitySamhaphage})");
                    settings.complexityMultiplierSamhaphage =
                        listing.Slider(settings.complexityMultiplierSamhaphage, 0.5f, 5.0f);

                    listing.GapLine();

                    listing.Label("Limit Presets".Colorize(Color.yellow));
                    string currentLimitDifficulties = settings.limitDifficulties.ToString().Replace("Limit", "");;
                    if (listing.ButtonTextLabeled("Limit Preset", currentLimitDifficulties))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        foreach (NullThrumDifficultyPreset preset in Enum.GetValues(typeof(NullThrumDifficultyPreset)))
                        {
                            string label = preset.ToString().Replace("Limit", "");
                            options.Add(new FloatMenuOption(label, () => { settings.SetNullThrumDifficultyPreset(preset); }));
                        }

                        Find.WindowStack.Add(new FloatMenu(options));
                    }

                    listing.GapLine();

                    listing.Label(
                        "<color=gray><size=10>Simple: Mechanical/Technical descriptions.\nLore: Flavor/In-universe descriptions.</size></color>");
                    listing.Gap();                    
                    listing.GapLine();
                    listing.Gap();
                    listing.Label("Metabolic Limits".Colorize(Color.yellow));
                    listing.Gap();
                    listing.CheckboxLabeled("Enable Metabolic Limits", ref settings.limitMetabolism.enabled,
                        "Enable means pawn can't acquire new genes when they hit the metabolism limit");
                    listing.Label(
                        "Adjust the limit for the metabolic range for different xenotypes who can steal genes. Negative metabolism is \"better\" but hungrier.");
                    listing.Gap();
                    // These xenotypes don't have a way to acquire genes. Leave them wired up in the settings in case I add them later.
//                    DrawMetabolicLimit(listing, "Fluxspawn", settings.limitMetabolism.fluxspawn, v => settings.limitMetabolism.fluxspawn = v);
//                    DrawMetabolicLimit(listing, "Echovessels", settings.limitMetabolism.echovessel, v => settings.limitMetabolism.echovessel = v);
//                    DrawMetabolicLimit(listing, "Cradlemold", settings.limitMetabolism.cradlemold, v => settings.limitMetabolism.cradlemold = v);
                    DrawMetabolicLimit(listing, "Hallowbound", settings.limitMetabolism.hallowbound,
                        v => settings.limitMetabolism.hallowbound = v);
                    DrawMetabolicLimit(listing, "Samhaphages", settings.limitMetabolism.samhaphage,
                        v => settings.limitMetabolism.samhaphage = v);
                    DrawMetabolicLimit(listing, "Sovereign Stillness", settings.limitMetabolism.sovereign_stillness,
                        v => settings.limitMetabolism.sovereign_stillness = v);

                    listing.Gap();
                    listing.Label("Trait Limits".Colorize(Color.yellow));
                    listing.Gap();
                    listing.CheckboxLabeled("Enable Trait Limits", ref settings.limitTraits.enabled,
                        "Enable means pawn can't acquire new traits when they hit the limit");
                    listing.Label(
                        "Adjust the limit for the maximum number of traits for different xenotypes.");
                    listing.Gap();

                    DrawTraitLimit(listing, "Fluxspawn", settings.limitTraits.fluxspawn,
                        v => settings.limitTraits.fluxspawn = v);
                    DrawTraitLimit(listing, "Echovessels", settings.limitTraits.echovessel,
                        v => settings.limitTraits.echovessel = v);
                    DrawTraitLimit(listing, "Cradlemold", settings.limitTraits.cradlemold,
                        v => settings.limitTraits.cradlemold = v);
                    DrawTraitLimit(listing, "Hallowbound", settings.limitTraits.hallowbound,
                        v => settings.limitTraits.hallowbound = v);
                    DrawTraitLimit(listing, "Samhaphages", settings.limitTraits.samhaphage,
                        v => settings.limitTraits.samhaphage = v);
                    DrawTraitLimit(listing, "Sovereign Stillness", settings.limitTraits.sovereign_stillness,
                        v => settings.limitTraits.sovereign_stillness = v);

                    listing.Gap();
                    listing.Label("Population Percentage Limits".Colorize(Color.yellow));
                    listing.Label(
                        "Controls the maximum percentage of the colony that can be of a specific xenotype. Limits evolving everyone to Samhaphages or having so much micromanagement that things stop being fun.");
                    listing.Gap();
                    listing.CheckboxLabeled("Enable Population Control", ref settings.limitPercentage.enabled,
                        "Enable means if the colony distribution is becoming unbalanced you won't be able to create more of the xenotype.");
                    listing.Gap();

                    DrawPercentageLimit(listing, "Fluxspawn", settings.limitPercentage.fluxspawn,
                        v => settings.limitPercentage.fluxspawn = v);
                    DrawPercentageLimit(listing, "Echovessels", settings.limitPercentage.echovessel,
                        v => settings.limitPercentage.echovessel = v);
                    DrawPercentageLimit(listing, "Cradlemold", settings.limitPercentage.cradlemold,
                        v => settings.limitPercentage.cradlemold = v);
                    DrawPercentageLimit(listing, "Hallowbound", settings.limitPercentage.hallowbound,
                        v => settings.limitPercentage.hallowbound = v);
                    DrawPercentageLimit(listing, "Samhaphages", settings.limitPercentage.samhaphage,
                        v => settings.limitPercentage.samhaphage = v);
                    // There can only be one Sovereign Stillness 
                    // DrawPercentageLimit(listing, "Sovereign Stillness", settings.limitPercentage.sovereign_stillness, v => settings.limitPercentage.sovereign_stillness = v);

                    listing.Gap();
                    listing.Label("Genetic Complexity Multiplier".Colorize(Color.yellow));
                    listing.Gap();
                    listing.CheckboxLabeled("Enable Genetic Complexity Multiplier", ref settings.multiplierComplexity.enabled,
                        "Enable means this multiplier will be applied to the genetic complexity calculations.");
                    listing.Label(
                        "Affects amplifying to another Xenotype (e.g. evolution) and resurrection costs.");
                    listing.Gap();

                    DrawMultiplierComplexity(listing, "Fluxspawn", settings.multiplierComplexity.fluxspawn,
                        v => settings.multiplierComplexity.fluxspawn = v);
                    DrawMultiplierComplexity(listing, "Echovessels", settings.multiplierComplexity.echovessel,
                        v => settings.multiplierComplexity.echovessel = v);
                    DrawMultiplierComplexity(listing, "Cradlemold", settings.multiplierComplexity.cradlemold,
                        v => settings.multiplierComplexity.cradlemold = v);
                    DrawMultiplierComplexity(listing, "Hallowbound", settings.multiplierComplexity.hallowbound,
                        v => settings.multiplierComplexity.hallowbound = v);
                    DrawMultiplierComplexity(listing, "Samhaphages", settings.multiplierComplexity.samhaphage,
                        v => settings.multiplierComplexity.samhaphage = v);
                    DrawMultiplierComplexity(listing, "Sovereign Stillness", settings.multiplierComplexity.sovereign_stillness,
                        v => settings.multiplierComplexity.sovereign_stillness = v);                    

                    break;

                case SettingsTab.Debugging:
                    listing.Gap();

                    listing.Label(
                        "Scan colony pawns for duplicate or conflicting traits and remove them.");
                    listing.Label(
                        "Vanilla RimWorld sometimes has issues when adding a Gene that forbids traits with a pawn that has that trait.");

                    if (listing.ButtonText("Remove duplicate/conflicting traits in colony"))
                    {
                        OMW_BlacklistTraits.FixColonistsTraits();
                        Messages.Message(
                            $"{Prefix} Duplicate/conflicting traits removed from colonists.",
                            MessageTypeDefOf.TaskCompletion, false);
                    }

                    listing.Gap();
                    listing.GapLine();

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
                    listing.CheckboxLabeled("Log Abilities", ref settings.logAbilities,
                        "Detailed traces for ability application and logic.");
                    listing.CheckboxLabeled("Log Kill", ref settings.logKill,
                        "Traces for killing victims and destroying corpses.");
                    listing.CheckboxLabeled("Log CompAbilityEffect", ref settings.logCompAbilityEffect,
                        "Traces for menu generation and target selection.");
                    listing.CheckboxLabeled("Log Genes", ref settings.logGenes,
                        "Traces for gene addition, removal, and complexity calculation.");
                    listing.CheckboxLabeled("Log Resonance", ref settings.logResonance,
                        "Traces for resonance consumption and gains.");
                    listing.CheckboxLabeled("Log Hediffs", ref settings.logHediffs,
                        "Traces for technical Hediff components (e.g. ZeroWill).");
                    listing.CheckboxLabeled("Log Jobs", ref settings.logJobs,
                        "Traces for the custom 'Approach and Interact' jobs.");
                    listing.CheckboxLabeled("Log UI", ref settings.logUI, "Traces for the UIs added by the mod.");
                    listing.CheckboxLabeled("Log Mutation", ref settings.logMutation,
                        "Traces for genes getting added by the Random Mutation genes.");
                    listing.CheckboxLabeled("Log Selection", ref settings.logSelection, "Traces for genes selector.");
                    listing.CheckboxLabeled("Log Hediff", ref settings.logHediff,
                        "Traces for Hediffs (e.g. parasite).");
                    break;
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawMetabolicLimit(Listing_Standard listing, string label, int value, Action<int> setValue)
        {
            string metabolicLabel = $"{value}";
            if (listing.ButtonTextLabeled($"{label} metabolism limit", metabolicLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                int[] values = { -5, -15, -30, -60, -100, -150, -200, -300, -500, -1000, -10000 };
                foreach (int val in values)
                {
                    int targetVal = val;
                    options.Add(new FloatMenuOption($"{val}", () => setValue(targetVal)));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void DrawTraitLimit(Listing_Standard listing, string label, int value, Action<int> setValue)
        {
            if (listing.ButtonTextLabeled($"{label} trait limit", $"{value}"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                int[] values = { 3, 5, 8, 13, 15, 20, 25, 30, 35, 40, 45, 50, 60, 100, 1000 };
                foreach (int val in values)
                {
                    int targetVal = val;
                    options.Add(new FloatMenuOption($"{val}", () => setValue(targetVal)));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void DrawPercentageLimit(Listing_Standard listing, string label, int value, Action<int> setValue)
        {
            if (listing.ButtonTextLabeled($"{label} population limit", $"{value}%"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                for (int i = 0; i <= 100; i += 10)
                {
                    int targetVal = i;
                    options.Add(new FloatMenuOption($"{targetVal}%", () => setValue(targetVal)));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void DrawMultiplierComplexity(Listing_Standard listing, string label, float value, Action<float> setValue)
        {
            if (listing.ButtonTextLabeled($"{label} multiplier", $"{value}"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                for (float i = 0.5f; i <= 1.0f; i += 0.5f)
                {
                    float targetVal = i;
                    options.Add(new FloatMenuOption($"{targetVal}%", () => setValue(targetVal)));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }        

        private void DrawValueSlider(Listing_Standard listing, ref NullThrumAbilityProps abilityProps)
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
