using RimWorld;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnShiftHiveling: FluxspawnShiftBase
    {
        public override string VerbName => "Transpose";
        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Transpose {caster.LabelShort} to a Fluxspawn Hiveling.";
        }


        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/ShiftFluxspawnHiveling");
        public override XenotypeDef TargetXenotype()
        {
            return OMW_XenotypeDefOf.omw_fluxspawn_hiveling;
        }
    }
}
