using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    [DefOf]
    public static class OMW_HediffDefOf
    {
        // This MUST exactly match the <defName> in your Hediff XML
        public static HediffDef OMW_SilentServitude;
        public static HediffDef OMW_GeneticDissonance;
        public static HediffDef OMW_ParasiticImplantation;
        public static HediffDef OMW_TemporaryUnconscious;
        public static HediffDef OMW_Assimilator;
        public static HediffDef OMW_CradlemoldAttunement;

        static OMW_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_HediffDefOf));
        }
    }

    [DefOf]
    public static class OMW_XenotypeDefOf
    {
        // This MUST exactly match the <defName> in your Xenotype XML
        public static XenotypeDef omw_cradlemold;
        public static XenotypeDef omw_echovessel;
        public static XenotypeDef omw_fluxspawn_brute;
        public static XenotypeDef omw_fluxspawn_flicker;
        public static XenotypeDef omw_fluxspawn_hiveling;
        public static XenotypeDef omw_hallowbound;
        public static XenotypeDef omw_samhaphage;
        public static XenotypeDef omw_sovereign_stillness;

        static OMW_XenotypeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_XenotypeDefOf));
        }
    }

    [DefOf]
    public static class OMW_JobDefOf
    {
        // This MUST exactly match the <defName> in your Xenotype XML
        public static JobDef OMW_ApproachAndInteract;

        static OMW_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_JobDefOf));
        }
    }

    [DefOf]
    public static class OMW_GeneDefOf
    {
        public static GeneDef OMW_ScouredMind;
        public static GeneDef OMW_NullThrum;
        public static GeneDef OMW_Resonance;

        public static GeneDef OMW_Mutation_3_StrayFrequency;        
        public static GeneDef OMW_Mutation_4_HarmonicDistortion;
        public static GeneDef OMW_Mutation_1_FrequencyBleed;
        public static GeneDef OMW_Mutation_5_PhaseRupture;
        public static GeneDef OMW_Mutation_2_SpectrumInterference;
        public static GeneDef OMW_Cradlemold_Frame;
        public static GeneDef OMW_Fluxspawn_Nudist;
        public static GeneDef OMW_Fluxspawn_Fertility;
        public static GeneDef OMW_FluxSpawn_Hiveling_Frame;
        public static GeneDef OMW_FluxSpawn_Brute_Frame;
        public static GeneDef OMW_FluxSpawn_Flicker_Frame;
        public static GeneDef OMW_Hallowbound_Frame;
        public static GeneDef OMW_Hallowbound_EntropyWell;
        public static GeneDef OMW_Samhaphage_Frame;
        public static GeneDef OMW_SovereignStillness_Frame;
        public static GeneDef OMW_PsychicAbility_SovereignStillness;        

        static OMW_GeneDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_GeneDefOf));
        }
    }

    [DefOf]
    public static class OMW_StatDefOf
    {
        public static StatDef OMW_StatResonance;
        static OMW_StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_StatDefOf));
        }
    }

    [DefOf]
    public static class OMW_EffecterDefOf
    {
        public static EffecterDef OMW_YumEffect;

        static OMW_EffecterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_EffecterDefOf));
        }
    }

    [DefOf]
    public static class OMW_BackstoryDefOf
    {
        public static BackstoryDef OMW_Flatten_Childhood;
        public static BackstoryDef OMW_Flatten_Adulthood;
        static OMW_BackstoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_BackstoryDefOf));
        }
    }

    [DefOf]
    public static class OMW_ThoughtDefOf
    {
        public static ThoughtDef OMW_PhaseLock_Flush;
        public static ThoughtDef OMW_PhaseLock_Surrender;

        static OMW_ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OMW_ThoughtDefOf));
        }
    }    
}    