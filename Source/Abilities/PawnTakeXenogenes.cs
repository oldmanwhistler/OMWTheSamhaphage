using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public class PawnTakeXenogenes
    {
        public bool Apply(Pawn target, Pawn caster)
        {             
            if (target == null || caster == null) return false;

            if (!OMWGenes.StealXenogenes(target, caster)) return false;

            return true;
        }

        public void ApplySacrifice(Pawn target, Pawn caster)
        {
            // if the pawn has the Null-Thrum this isn't fatal
            if (OMWGenes.HasNullThrum((target)))
            {
                Apply(target, caster);
                return;
            }
            string msg = $"{target.LabelShort} has been killed by {caster.LabelShort} while taking xenogenes.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (Apply(target, caster))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(caster, target);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(target, sacrificeAction);
        }

        public static bool CanApplyOn(Pawn target, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (target == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (target.RaceProps?.Humanlike != true)
            {
                reason = "Target is not humanlike.";
                return false;
            }
            
            if (target.HostileTo(caster))
            {
                reason = "Target is hostile.";
                return false;
            }

            if (target.genes == null)
            {
                reason = "Target's InnerPawn has no gene tracker.";
                return false;
            }

            if (target.genes.Xenogenes.Count == 0)
            {
                reason = "Target has no xenogenes to extract.";
                return false;
            }

            return true;
        }
    }
}