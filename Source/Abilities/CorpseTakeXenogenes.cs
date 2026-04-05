using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public class CorpseTakeXenogenes
    {
        public bool Apply(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;

            Pawn target = corpse.InnerPawn;
            if (target == null) return false;

            if (!OMWGenes.StealXenogenes(target, caster)) return false;

            OMWAnomaly.CorpseToShamblerOrDestroy(corpse);

            return true;
        }

        public void ApplySacrifice(Corpse corpse, Pawn caster)
        {
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                Apply(corpse, caster);
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowXenogeneLossConfirmation(corpse.InnerPawn, caster, sacrificeAction);
        }


        public static bool CanApplyOn(Corpse corpse, out string reason)
        {
            reason = "unknown reason";
            if (corpse == null)
            {
                reason = "Target is null.";
                return false;
            }

            // 2. Is there a pawn inside?
            Pawn pawn = corpse.InnerPawn;
            if (pawn == null)
            {
                reason = "Corpse is missing InnerPawn or InnerPawn is invalid.";
                return false;
            }

            if (pawn.RaceProps?.Humanlike != true)
            {
                reason = "Target is not humanlike.";
                return false;
            }

            if (pawn.genes == null)
            {
                reason = "Target's InnerPawn has no gene tracker.";
                return false;
            }
            if (pawn.genes.Xenogenes.Count == 0)
            {
                reason = "Target has no xenogenes to extract.";
                return false;
            }            

            // 4. Is the pawn already being resurrected/interacted with?
            if (pawn.Spawned && !corpse.Spawned)
            {
                reason = "Pawn is already being processed.";
                return false;
            }

            // 5. Is the head missing? (RimWorld standard resurrection fails without a head)
            if (pawn.health.hediffSet.GetBrain() == null)
            {
                reason = "Vessel is decapitated; the frequency cannot be anchored.";
                return false;
            }            

            return true;
        }
    }
}