using RimWorld;
using Verse;

namespace OMW_Samhaphage
{    
    public abstract class FluxspawnShiftBase: NullThrumAbilityPawnOnly
    {
        public abstract XenotypeDef TargetXenotype();

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.transpose;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override void ApplyPawn(Pawn pawn, Pawn caster)
        {
            if (ResonanceUtility.HasAvailable(caster, OMW_Mod.settings.abilityValue.transpose.value))
            {
                ResonanceUtility.Decr(caster, OMW_Mod.settings.abilityValue.transpose.value);
                OMWGenes.ChangeEndotype(pawn, TargetXenotype());
            }
            else
            {
                Messages.Message($"Not enough Resonance to transpose into {TargetXenotype()}.", MessageTypeDefOf.RejectInput);
            }

            doOnComplete();
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            // Check if target is a not already Retune
            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {
                reason = $"{caster.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if ((victim.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_brute) && 
                (victim.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) &&
                (victim.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                reason = $"{victim.LabelShort} is not a Fluxspawn.";
                return false;
            }

            if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{victim.LabelShort} is affected by Genetic Dissonance.";
                return false;
            }

            if (!ResonanceUtility.HasAvailable(caster, OMW_Mod.settings.abilityValue.transpose.value))
            {
                reason = $"{caster.LabelShort} does not have enough available resonance (needs {OMW_Mod.settings.abilityValue.transpose.value})";
                return false;
            }

            return true;
        }    
    }
}
