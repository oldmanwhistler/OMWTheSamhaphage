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

        public int CurrentComplexity;
        public abstract int RequiredComplexity { get; }
        public abstract RimWorld.XenotypeDef TargetXenotype { get; }

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return
                $"{caster} reaches the genetic threshold to elevate themselves to the next level of the genetic hierarchy and become {TargetXenotype.label}";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Amplify");

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;
            OMWGenes.ChangeEndotype(victim, TargetXenotype);
            MoteMaker.MakeAttachedOverlay(victim, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
            Log.Debug($"Amplified {victim.LabelShort}: became {TargetXenotype.label}.");
            return true;
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!OMWGenes.CanChangeXenotype(victim, TargetXenotype))
            {
                reason = $"{victim} can't become {TargetXenotype.label}";
                return false;
            }

            CurrentComplexity = OMWGenes.CalculateComplexity(victim);
            if (CurrentComplexity < RequiredComplexity)
            {
                reason =
                    $"Not enough genetic complexity to become {TargetXenotype.label}. At {CurrentComplexity}/{RequiredComplexity}. Keep harrowing.";
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
