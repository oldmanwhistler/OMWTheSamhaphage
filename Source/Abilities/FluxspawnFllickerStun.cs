using RimWorld;
using Verse;

namespace OMW_Samhaphage
{    
    public class FluxspawnFickerStun: NullThrumAbilityPawnOnly
    {

        public override bool ApplyPawn(Pawn pawn, Pawn caster = null)
        {            
            pawn.stances.stunner.StunFor(5000, caster);

            return true;
        }

        public override bool CanApplyOnPawn(Pawn p, out string reason)
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

        public override FloatMenuOption NewFloatMenuOptionPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, out reason))
            {
                return new FloatMenuOption($"Stun {pawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't stun {pawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }
    }
}
