using System;
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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;
            NullThrumAbilityBase ability;

            if (xeno == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                // this only works because Sovereign and Samhaphage have the same menu
                OMWXenotypes.ThereCanOnlyBeOne();
                xeno = parent.pawn.genes.Xenotype;
            }            

            if ((xeno != OMW_XenotypeDefOf.omw_samhaphage) && (xeno != OMW_XenotypeDefOf.omw_sovereign_stillness))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Samhaphage abilities.", MessageTypeDefOf.NegativeEvent); 
            }
            else
            {
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

                if (PawnApplyRetune.CanApplyOn(parent.pawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => PawnApplyRetune.Apply(parent.pawn, parent.pawn)), "Retune self"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't Retune self. {reason}."));
                }
            }

            if (xeno == OMW_XenotypeDefOf.omw_samhaphage)
            {
                int reqComplexity = 200; // Apex Evolution requirement
                int currComplexity = OMWGenes.CalculateComplexity(parent.pawn);
                if (currComplexity < reqComplexity)
                {
                    items.Add(new MenuItemText(null, $"At {currComplexity}/{reqComplexity} for becoming Sovereign Stillness"));
                }
                else if (OMWXenotypes.IsSovereignStillnessInPlayerFaction())
                {
                    items.Add(new MenuItemText(null, "The Sovereign Stillness is already part of the colony. There can only be one."));
                }
                else
                {
                    items.Add(new MenuItemText((Action)(() => OMWGenes.ChangeEndotype(parent.pawn, OMW_XenotypeDefOf.omw_samhaphage, OMW_XenotypeDefOf.omw_sovereign_stillness)), "Arise to Sovereign Stillness"));
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