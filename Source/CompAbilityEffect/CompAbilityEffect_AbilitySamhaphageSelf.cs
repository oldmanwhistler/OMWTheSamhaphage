using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilitySamhaphageSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySamhaphageSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilitySamhaphageSelf);
        }
    }

    public class CompAbilityEffect_AbilitySamhaphageSelf :  CompAbilityEffect_AbilityBase

    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption(GetGeneStateDesc(parent.pawn), null) { Disabled = true });
            // fighter types can shift to hiveling

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;

            if (xeno == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                // this only works because Sovereign and Samhaphage have the same menu
                OMWXenotypes.ThereCanOnlyBeOne();
                xeno = parent.pawn.genes.Xenotype;
            }            

            if ((xeno != OMW_XenotypeDefOf.omw_samhaphage) && (xeno != OMW_XenotypeDefOf.omw_sovereign_stillness))
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                { Disabled = true });                
            }
            else
            {
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
            }

            if (xeno == OMW_XenotypeDefOf.omw_samhaphage)
            {
                int reqComplexity = 200;
                int currComplexity = OMWGenes.CalculateComplexity(parent.pawn);
                if (currComplexity < reqComplexity)
                {
                    options.Add(new FloatMenuOption(
                            $"At {currComplexity}/{reqComplexity} for becoming Sovereign Stillness",
                            null)
                        { Disabled = true });
                }
                else if (OMWXenotypes.IsSovereignStillnessInPlayerFaction())
                {
                    options.Add(new FloatMenuOption(
                            $"The Sovereign Stillness is already part of the colony. There can only be one.",
                            null)
                        { Disabled = true });
                }
                else
                {
                    options.Add(new FloatMenuOption("Arise to Sovereign Stillness",
                        () => OMWGenes.ChangeEndotype(parent.pawn, OMW_XenotypeDefOf.omw_samhaphage, OMW_XenotypeDefOf.omw_sovereign_stillness)));
                }
            }

            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }
    }                

}