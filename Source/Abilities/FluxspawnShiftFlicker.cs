using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftFlicker: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose";
        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Transpose {caster.LabelShort} to a Fluxspawn Flicker.";
        }
    
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/ShiftFluxspawnFlicker");
        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_flicker;
        }
    }
}
