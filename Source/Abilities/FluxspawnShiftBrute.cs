using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftBrute: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose ";
        public override string VerbDescription => "to a Fluxspawn Brute.";         
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/ShiftFluxspawnBrute");

        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_brute;
        }
    }
}
