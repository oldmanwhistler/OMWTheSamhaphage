using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class CorpseApplyResurrectBase: NullThrumAbilityCorpseOnly
    {
        protected int complexityCost;

        public abstract RimWorld.XenotypeDef TargetXenotype { get;  }
        public abstract Verse.HediffDef TargetHediff { get; }    
        public abstract bool SacrificeCaster { get; }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.resurrect;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        private bool ApplyResurrect(Corpse corpse, Pawn caster)
        {
            if (corpse == null || corpse.InnerPawn == null) return false;

            Pawn victim = corpse.InnerPawn;

            if (victim.Faction != Faction.OfPlayer)
            {
                victim.SetFaction(Faction.OfPlayer);
            }

            if (!SacrificeCaster && !ResonanceUtility.Decr(caster, complexityCost))
            {
                Log.Error(
                    $"[OMW_Samhaphage] Failed to decrement resonance for {caster.LabelShort} during {AbilityType}. This indicates a logic error where CanApplyOnPawn did not prevent the ability.");
                doOnComplete();
                return false;
            }

            if (this.TargetXenotype != null)
            {
                OMWGenes.ChangeXenotype(victim, this.TargetXenotype, false);
            }

            try
            {
                ResurrectionUtility.TryResurrect(victim);
            }
            catch (System.NullReferenceException e)
            {
                Log.Error($"[OMW] Resurrection failed during Notify_PostResurrected for {victim.Label}: {e.Message}");
            }

            if (!victim.Spawned) return false;

            victim.health.RestorePart(victim.RaceProps.body.corePart);

            if (this.TargetHediff != null)
            {
                victim.health.AddHediff(this.TargetHediff);
            }

            if (!victim.health.hediffSet.HasHediff(HediffDef.Named("OMW_Reassembled")))
            {
                victim.health.AddHediff(HediffDef.Named("OMW_Reassembled"));
            }

            return true;
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return;

            if (!this.SacrificeCaster)
            {
                ApplyResurrect(corpse, caster);
                doOnComplete();
                return;
            }

            string msg = $"{caster.LabelShort} has died resurrecting {corpse.InnerPawn.LabelShort}.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (ApplyResurrect(corpse, null))
                {
                    KillUtility.PawnKillDestroy(caster, caster);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }                
            };

            // Open the confirmation dialog
            ShowLethalConfirmation(caster, sacrificeAction);
        }

        public bool CanApplyResurrect(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (corpse == null)
            {
                reason = "Target is null.";
                return false;
            }            

            // Is there a pawn inside?
            Pawn victim = corpse.InnerPawn;
            if (victim == null)
            {
                reason = "Corpse is missing InnerPawn or InnerPawn is invalid.";
                return false;
            }

            if (victim.RaceProps?.Humanlike != true)
            {
                reason = "Target is not humanlike.";
                return false;
            }

            if (corpse.GetRotStage() == RotStage.Dessicated)
            {
                reason = "Vessel is dessicated; the frequency cannot be anchored.";
                return false;
            }

            if (corpse.GetRotStage() == RotStage.Rotting)
            {
                reason = "Vessel is rotting; the frequency cannot be anchored.";
                return false;
            }

            // Is the pawn already being resurrected/interacted with?
            if (victim.Spawned && !corpse.Spawned)
            {
                reason = "Pawn is already being processed.";
                return false;
            }

            // Is the head missing? (RimWorld standard resurrection fails without a head)
            if (victim.health.hediffSet.GetBrain() == null)
            {
                reason = "Vessel is decapitated; the frequency cannot be anchored.";
                return false;
            }

            return true;
        }        
    }
}