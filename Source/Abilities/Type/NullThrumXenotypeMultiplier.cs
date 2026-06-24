using RimWorld;

namespace OMW_Samhaphage
{

    public abstract class NullThrumXenotypeMultiplier
    {
        protected float disabled_value = 0;
        public bool enabled = true;        
        public float fluxspawn;
        public float echovessel;
        public float cradlemold;
        public float hallowbound;
        public float samhaphage;
        public float sovereign_stillness;

        public NullThrumXenotypeMultiplier(NullThrumDifficultyPreset setting)
        {
            SetMultiplierDefaults(setting);
        }

        public abstract void SetMultiplierDefaults(NullThrumDifficultyPreset settings);

        public float GetMultiplier(XenotypeDef xenotypeDef)
        {
            if (!enabled) return disabled_value;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_fluxspawn_brute || 
                xenotypeDef == OMW_XenotypeDefOf.omw_fluxspawn_flicker || 
                xenotypeDef == OMW_XenotypeDefOf.omw_fluxspawn_hiveling) return fluxspawn;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_echovessel) return echovessel;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_cradlemold) return cradlemold;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_hallowbound) return hallowbound;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_samhaphage) return samhaphage;
            if (xenotypeDef == OMW_XenotypeDefOf.omw_sovereign_stillness) return sovereign_stillness;
            return 0;
        }
    }
}