using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public class CorpseApplyResurrectHallowbound : CorpseApplyResurrectBase
    {
        public override Verse.HediffDef TargetHediff => OMW_HediffDefOf.OMW_SilentServitude;
        public override RimWorld.XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_hallowbound;
        public override bool SacrificeCaster => true;
        public override bool IsLethal => true;
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/ResurrectHallowbound",
            false) ?? BaseContent.BadTex;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            if (this.SacrificeCaster)
                return $"Resurrect {victim.LabelShort} as Hallowbound by sacrificing yourself.";
            else
                return $"Resurrect {victim.LabelShort} as Hallowbound.";
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (!CanApplyResurrect(corpse, caster, out reason))
            {
                return false;
            }

            Pawn victim = corpse.InnerPawn;

            if (victim.health.hediffSet.HasHediff(HediffDef.Named("OMW_Reassembled")))
            {
                reason = "Vessel has already been reassembled; the frequency cannot be anchored.";
                return false;
            }

            if (!this.SacrificeCaster)
            {
                complexityCost = OMWGenes.CalculateComplexity(victim, true, caster.genes.Xenotype);
                if (!ResonanceUtility.HasAvailable(caster, complexityCost))
                {
                    reason =
                        $"{caster.LabelShort} does not have enough resonance to resurrect {victim.LabelShort}. Requires {complexityCost} resonance.";
                    return false;
                }
            }            

            return CanApplyLimitXenotype(TargetXenotype, out reason);            
        }
    }
}