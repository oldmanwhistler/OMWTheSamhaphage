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
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            NullThrumAbilityBase ability;
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent);
                return false;                
            }
            else
            {
                ability = new PawnApplyRetune();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

                ability = new PawnApplyCompress();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
                
                ability = new PawnApplyAmplifyHallowbound();
                items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
            }

            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }
    }

}