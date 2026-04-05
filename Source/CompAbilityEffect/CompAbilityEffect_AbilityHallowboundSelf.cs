using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityHallowboundSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHallowboundSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityHallowboundSelf);
        }
    }

    public class CompAbilityEffect_AbilityHallowboundSelf :  CompAbilityEffect_AbilityBase

    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;

            options.Add(new FloatMenuOption(GetGeneStateDesc(parent.pawn), null) { Disabled = true });

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
                OpenWindow(options);
                return;
            }
            
            if (PawnApplyRetune.CanApplyOn(parent.pawn, out reason))
            {
                options.Add(new FloatMenuOption($"Retune self",
                    () => PawnApplyRetune.Apply(parent.pawn, parent.pawn)));
            }
            else
            {
                options.Add(new FloatMenuOption($"Can't Retune self. {reason}.", null)
                    { Disabled = true });
            }
            
            if (PawnClearXenogenes.CanApplyOn(parent.pawn, out reason))
            {
                options.Add(new FloatMenuOption($"Reject xenogenes",
                    () => PawnClearXenogenes.Apply(parent.pawn, parent.pawn)));
            }
            else
            {
                options.Add(new FloatMenuOption($"Can't reject xenogenes. {reason}.", null)
                    { Disabled = true });
            }

            int xenogenes = OMWGenes.CountXenogenes(parent.pawn);
            if (xenogenes > 0)
            {
                options.Add(new FloatMenuOption("Integrate xenogenes",
                    () => OMWGenes.XenogenesToEndogenes(parent.pawn)));
            }
            else
            {
                options.Add(
                    new FloatMenuOption("No xenogenes available to integrate", null)
                        { Disabled = true });
            }

            int reqComplexity = 0;
            int currComplexity = OMWGenes.CalculateComplexity(parent.pawn);
            if (currComplexity >= reqComplexity)
            {
                options.Add(new FloatMenuOption("Arise to Samhaphage",
                () => OMWGenes.ChangeEndotype(parent.pawn, OMW_XenotypeDefOf.omw_hallowbound, OMW_XenotypeDefOf.omw_samhaphage)));
            }
            else
            {
                options.Add(new FloatMenuOption($"At {currComplexity}/{reqComplexity} for becoming Samhaphage", null) { Disabled = true });                            
            }

            OpenWindow(options);
        }
   }                

}