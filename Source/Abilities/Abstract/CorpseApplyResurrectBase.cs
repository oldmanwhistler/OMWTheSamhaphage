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

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.resurrect;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
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
                OMWGenes.ChangeXenotype(pawn, this.TargetXenotype, false);
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

            if (this.TargetHediff != null)
            {
                pawn.health.AddHediff(this.TargetHediff);
            }

            if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("OMW_Reassembled")))
            {
                pawn.health.AddHediff(HediffDef.Named("OMW_Reassembled"));
            }

            return true;
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return;

            if (!this.SacrificeCaster)
            {
                ApplyResurrect(corpse);
                doOnComplete();
                return;
            }

            string msg = $"{caster.LabelShort} has died resurrecting {corpse.InnerPawn.LabelShort}.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (ApplyResurrect(corpse))
                {
                    KillUtility.PawnKillDestroy(caster, caster);
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }

                // Needs to be false so doesn't get stuck on a loop
                
            };

            // Open the confirmation dialog
            ShowLethalConfirmation(caster, sacrificeAction);
        }
    }
}