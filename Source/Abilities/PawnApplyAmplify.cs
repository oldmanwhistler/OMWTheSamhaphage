using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public abstract class PawnApplyAmplifyBase : NullThrumAbilityPawnOnly
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.amplify;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        private float ResonanceCost => AbilityProp.value;
        public abstract int RequiredComplexity { get; }
        public abstract RimWorld.XenotypeDef TargetXenotype { get; }

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            // Assuming victim is always the caster for this self-amplification ability.
            return
                $"{caster.LabelShort} reaches the genetic threshold to elevate themselves to the next level of the genetic hierarchy and become {TargetXenotype.label}. (Cost: {ResonanceCost} Resonance)";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Amplify");

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;
            
            // Consume resonance
            if (!ResonanceUtility.Incr("Amplified", caster, ResonanceCost))
            {
                Log.Error($"[OMW_Samhaphage] Failed to decrement resonance for {caster.LabelShort} during Amplify ability. This indicates a logic error where CanApplyOnPawn did not prevent the ability.");
                doOnComplete();
                return;
            }
            OMWGenes.ChangeEndotype(victim, TargetXenotype);
            MoteMaker.MakeAttachedOverlay(victim, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
            Log.Debug($"Amplified {victim.LabelShort}: became {TargetXenotype.label}.");
            // DON'T GO BACK TO THE MENU because the pawn changes xenotypes
            //doOnComplete();
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!OMWGenes.CanChangeXenotype(victim, TargetXenotype, out reason))
            {
                return false;
            }

            // Check for resonance cost
            if (!ResonanceUtility.HasAvailable(caster))
            {
                reason = $"{caster.LabelShort} does not have enough resonance.";
                return false;
            }

            if (!CanApplyLimitXenotype(TargetXenotype, out reason))
            {
                return false;
            }

            // Check for complexity

            int currentComplexity = OMWGenes.CalculateComplexity(victim);
            if (currentComplexity < RequiredComplexity)
            {
                reason =
                    $"Not enough genetic complexity to become {TargetXenotype.label}. At {currentComplexity}/{RequiredComplexity}. Keep harrowing.";
                return false;
            }

            return true;
        }
    }


    public class PawnApplyAmplifyHallowbound : PawnApplyAmplifyBase
    {
        public override int RequiredComplexity => OMW_Mod.settings.complexityHallowbound;
        public override XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_samhaphage;
    }

    public class PawnApplyAmplifySamhaphage : PawnApplyAmplifyBase
    {
        public override int RequiredComplexity => OMW_Mod.settings.complexitySamhaphage;
        public override XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_sovereign_stillness;
    }
}
