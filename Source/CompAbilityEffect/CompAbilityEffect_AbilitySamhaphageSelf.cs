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
                return;
            }
            else
            {
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

                ability = new PawnApplyRetune();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

                ability = new ThingApplyNullify();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

                ability = new PawnApplyUnmute();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
            }

            if (xeno == OMW_XenotypeDefOf.omw_samhaphage)
            {
                ability = new PawnApplyAmplifySamhaphage();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
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