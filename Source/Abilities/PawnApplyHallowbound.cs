using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public class PawnApplyHallowbound
    {
        public bool Apply(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;
            
            OMWGenes.ChangeXenotype(victim, victim.genes?.Xenotype, OMW_XenotypeDefOf.omw_hallowbound);

            return true;
        }

        public void ApplySacrifice(Pawn victim, Pawn caster)
        {
            string msg = $"{caster.LabelShort} has died making {victim.LabelShort} a Hallowbound.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (Apply(victim, caster))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(caster, caster);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(caster, sacrificeAction);
        }        

        public static bool CanApplyOn(Pawn p, out string reason)
        {
            reason = "unknown reason";

            if (p == null) 
            {
                reason = "Target is null.";
                return false;
            }            
            // Check if target is a not already Hallowbound
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (OMWGenes.HasNullThrum(p))
            {
                return true;
            }
            
            if (!OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} has not had their mind scoured to prepare them for genetic manipulation.";
                return false;
            }

            if (!p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
            {
                reason = $"{p.LabelShort} is not affected by Silent Servitude.";
                return false;
            }

            return true;
        }
    }
}