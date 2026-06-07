using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyHallowbound : NullThrumAbilityPawnOnly
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.hallowbound;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Hallowbound");
        public virtual bool SacrificeCaster => false;
        public override bool IsLethal => false;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return SacrificeCaster 
                ? $"Sacrifice yourself to transpose {victim.LabelShort} into a Hallowbound."
                : $"Transpose {victim.LabelShort} into a Hallowbound.";
        }

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            if (!SacrificeCaster)
            {
                OMWGenes.ChangeXenotype(victim, OMW_XenotypeDefOf.omw_hallowbound);
                doOnComplete(true);
                return;
            }

            string msg = $"{caster.LabelShort} has died making {victim.LabelShort} a Hallowbound.";
            System.Action sacrificeAction = () =>
            {
                OMWGenes.ChangeXenotype(victim, OMW_XenotypeDefOf.omw_hallowbound);
                KillUtility.PawnKillDestroy(caster, caster);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                // Needs to be false so doesn't get stuck on a loop
                doOnComplete(false);
            };

            ShowLethalConfirmation(caster, sacrificeAction);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }
            
            return true;
        }
    }

    public class PawnApplyHallowboundSacrifice : PawnApplyHallowbound
    {
        public override bool SacrificeCaster => true;
        public override bool IsLethal => true;
    }
}