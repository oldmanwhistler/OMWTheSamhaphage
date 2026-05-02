using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftHiveling: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose to Hiveling";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/ShiftFluxspawnHiveling");
        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_hiveling;
        }
    }
}
