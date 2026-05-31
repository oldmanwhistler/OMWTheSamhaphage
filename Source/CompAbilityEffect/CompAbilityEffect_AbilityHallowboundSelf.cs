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

            NullThrumAbilityBase ability;
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent);
                return;                
            }
            else
            {
                ability = new PawnApplyRetune();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
            
                int reqComplexity = OMW_Mod.settings.complexityHallowbound;
                int currComplexity = OMWGenes.CalculateComplexity(parent.pawn);
                if (currComplexity >= reqComplexity)
                {
                    items.Add(new MenuItemText((Action)(() => OMWGenes.ChangeEndotype(parent.pawn, OMW_XenotypeDefOf.omw_samhaphage)), "Arise to Samhaphage"));
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
                        Log.Debug($"BetterFloatMenu is invoking {action.Method.Name}");
                        action.Invoke();
                    }
                });
            }
        }
    }

}