using System;
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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent);                
            }
            else
            {
                if (PawnApplyRetune.CanApplyOn(parent.pawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => PawnApplyRetune.Apply(parent.pawn, parent.pawn)), "Retune self"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't Retune self. {reason}."));
                }

                if (PawnClearXenogenes.CanApplyOn(parent.pawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => PawnClearXenogenes.Apply(parent.pawn, parent.pawn)), "Reject xenogenes"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't reject xenogenes. {reason}."));
                }

                int xenogenes = OMWGenes.CountXenogenes(parent.pawn);
                if (xenogenes > 0)
                {
                    items.Add(new MenuItemText((Action)(() => OMWGenes.XenogenesToEndogenes(parent.pawn)), "Integrate xenogenes"));
                }
                else
                {
                    items.Add(new MenuItemText(null, "No xenogenes available to integrate"));
                }

                int reqComplexity = 0;
                int currComplexity = OMWGenes.CalculateComplexity(parent.pawn);
                if (currComplexity >= reqComplexity)
                {
                    items.Add(new MenuItemText((Action)(() => OMWGenes.ChangeEndotype(parent.pawn, OMW_XenotypeDefOf.omw_hallowbound, OMW_XenotypeDefOf.omw_samhaphage)), "Arise to Samhaphage"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"At {currComplexity}/{reqComplexity} for becoming Samhaphage"));
                }
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Message($"BetterFloatMenu is invoking {action.Method.Name}");
                        action.Invoke();
                    }
                });
            }
        }
    }

}