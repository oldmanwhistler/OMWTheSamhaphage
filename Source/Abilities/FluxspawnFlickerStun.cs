using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{    
    public class FluxspawnFlickerStun: NullThrumAbilityPawnOnly
    {
        public override string VerbName => "Stun";

        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Stun {victim.LabelShort} and prepare them for parasitization.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/Stun");
        public override bool ApplyPawn(Pawn pawn, Pawn caster = null)
        {            
            pawn.stances.stunner.StunFor(1000, caster);

            return true;
        }

        public override bool CanApplyOnPawn(Pawn p, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (p == null)
            {
                reason = "Target is null.";
                return false;
            }

            // Check if target is a not already Retune
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (OMWGenes.HasNullThrum(p))
            {
                reason = $"{p.LabelShort} is part of the harmony of the Null-Thrum.";
                return false;
            }

            return true;
        }

    }
}
