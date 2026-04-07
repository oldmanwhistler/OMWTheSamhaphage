using RimWorld;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftHiveling: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose to Hiveling";

        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_hiveling;
        }
    }
}
