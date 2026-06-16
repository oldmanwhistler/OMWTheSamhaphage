using RimWorld;

namespace OMW_Samhaphage
{
    public abstract class NullThrumXenotypeLimit
    {
        public bool enabled = true;
        public int fluxspawn;
        public int echovessel;
        public int cradlemold;
        public int hallowbound;
        public int samhaphage;
        public int sovereign_stillness;

        public NullThrumXenotypeLimit()
        {
        }

        public int GetLimit(XenotypeDef xenotypeDef)
        {
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