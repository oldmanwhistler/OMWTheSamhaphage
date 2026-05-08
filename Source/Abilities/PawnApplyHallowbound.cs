using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyHallowbound : NullThrumAbilityPawnOnly
    {
        public override string VerbName => "Hallowbound";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Hallowbound");
        public virtual bool SacrificeCaster => false;

        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return SacrificeCaster 
                ? $"Sacrifice yourself to transpose {victim.LabelShort} into a Hallowbound."
                : $"Transpose {victim.LabelShort} into a Hallowbound.";
        }

        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            if (!SacrificeCaster)
            {
                OMWGenes.ChangeXenotype(victim, victim.genes?.Xenotype, OMW_XenotypeDefOf.omw_hallowbound);
                return true;
            }

            string msg = $"{caster.LabelShort} has died making {victim.LabelShort} a Hallowbound.";
            System.Action sacrificeAction = () =>
            {
                OMWGenes.ChangeXenotype(victim, victim.genes?.Xenotype, OMW_XenotypeDefOf.omw_hallowbound);
                OMWAnomaly.PawnToShamblerOrKillDestroy(caster, caster);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
            };

            OMW_UIHelpers.ShowLethalConfirmation(caster, sacrificeAction);
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
                reason = $"{p.LabelShort} has not had their mind scoured.";
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

    public class PawnApplyHallowboundSacrifice : PawnApplyHallowbound
    {
        public override bool SacrificeCaster => true;
    }
}