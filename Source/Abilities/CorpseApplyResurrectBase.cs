using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class CorpseApplyResurrectBase: NullThrumAbilityCorpseOnly
    {
        public abstract RimWorld.XenotypeDef TargetXenotype { get;  }
        public abstract Verse.HediffDef TargetHediff { get; }    
        public abstract bool SacrificeCaster { get; }

        private bool ApplyResurrect(Corpse corpse)
        {
            if (corpse == null || corpse.InnerPawn == null) return false;

            Pawn pawn = corpse.InnerPawn;

            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }

            if (this.TargetXenotype != null)
            {
                OMWGenes.ChangeXenotype(pawn, null, this.TargetXenotype);
            }

            if (this.TargetHediff != null)
            {
                pawn.health.AddHediff(this.TargetHediff);
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

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;
            if (! this.SacrificeCaster)
            {
                return ApplyResurrect(corpse);
            }

            bool value = false;
            string msg = $"{caster.LabelShort} has died resurrecting {corpse.InnerPawn.LabelShort}.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (ApplyResurrect(corpse))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(caster, caster);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                        
                    value = true;
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(caster, sacrificeAction);
            return value;
        }
    }
}