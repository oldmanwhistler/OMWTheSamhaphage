using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public class CorpseApplyResurrect
    {
        public bool Apply(Corpse corpse, HediffDef targetHeDiff = null, XenotypeDef targetXenotype = null)
        {
            if (corpse == null || corpse.InnerPawn == null) return false;

            Pawn pawn = corpse.InnerPawn;

            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }

            if (targetXenotype != null)
            {
                OMWGenes.ChangeXenotype(pawn, null, targetXenotype);
            }

            if (targetHeDiff != null)
            {
                pawn.health.AddHediff(targetHeDiff);
            }

            try
            {
                ResurrectionUtility.TryResurrect(pawn);
            }
            catch (System.NullReferenceException e)
            {
                Log.Error($"[OMW] Resurrection failed during Notify_PostResurrected for {pawn.Label}: {e.Message}");
            }

            if (!pawn.Spawned) return false;

            pawn.health.RestorePart(pawn.RaceProps.body.corePart);

            return true;
        }

        public void ApplySacrifice(Corpse corpse, Pawn actor, HediffDef targetHeDiff = null, XenotypeDef
            targetXenotype = null)
        {
            string msg = $"{actor.LabelShort} has died resurrecting {corpse.InnerPawn.LabelShort}.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (Apply(corpse, targetHeDiff, targetXenotype))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(actor, actor);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(actor, sacrificeAction);
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