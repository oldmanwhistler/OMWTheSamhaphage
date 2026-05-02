using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftFlicker: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose to Flicker";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/ShiftFluxspawnFlicker");
        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_flicker;
        }
    }
}
