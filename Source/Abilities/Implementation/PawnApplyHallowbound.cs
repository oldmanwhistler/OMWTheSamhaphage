using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyHallowbound : NullThrumAbilityPawnOnly
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        protected int complexityCost;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.hallowbound;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Hallowbound", false) ??
                                          BaseContent.BadTex;
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
                if (!ResonanceUtility.Decr(caster, complexityCost))
                {
                    Log.Error($"[OMW_Samhaphage] Failed to decrement resonance for {caster.LabelShort} during {AbilityType}. This indicates a logic error where CanApplyOnPawn did not prevent the ability.");
                    doOnComplete();
                    return;
                }
                OMWGenes.ChangeXenotype(victim, OMW_XenotypeDefOf.omw_hallowbound);
                doOnComplete();
                return;
            }

            string msg = $"{caster.LabelShort} has died making {victim.LabelShort} a Hallowbound.";
            System.Action sacrificeAction = () =>
            {
                OMWGenes.ChangeXenotype(victim, OMW_XenotypeDefOf.omw_hallowbound);
                KillUtility.PawnKillDestroy(caster, caster);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);                
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

            if (!this.SacrificeCaster)
            {
                complexityCost = OMWGenes.CalculateComplexity(victim, true, caster.genes.Xenotype);
                if (!ResonanceUtility.HasAvailable(caster, complexityCost))
                {
                    reason =
                        $"{caster.LabelShort} does not have enough transpose {victim.LabelShort} to {OMW_XenotypeDefOf.omw_hallowbound.label}. Requires {complexityCost} resonance.";
                    return false;
                }
            }            

            return CanApplyLimitXenotype(OMW_XenotypeDefOf.omw_hallowbound, out reason);
        }
    }

    public class PawnApplyHallowboundSacrifice : PawnApplyHallowbound
    {
        public override bool SacrificeCaster => true;
        public override bool IsLethal => true;
    }
}