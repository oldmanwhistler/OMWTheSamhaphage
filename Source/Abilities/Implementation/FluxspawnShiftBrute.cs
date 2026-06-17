using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftBrute: FluxspawnShiftBase
    {
        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Transpose {caster.LabelShort} to a Fluxspawn Brute.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/ShiftFluxspawnBrute");

        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_brute;
        }
    }
}
