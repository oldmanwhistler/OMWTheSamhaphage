using RimWorld;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftFlicker: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose to Flicker";

        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_flicker;
        }
    }
}
