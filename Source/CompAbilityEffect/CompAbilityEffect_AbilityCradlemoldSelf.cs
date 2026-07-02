using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityCradlemoldSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityCradlemoldSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityCradlemoldSelf);
        }
    }

    public class CompAbilityEffect_AbilityCradlemoldSelf :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            if (xeno != OMW_XenotypeDefOf.omw_cradlemold)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Cradlemold abilities.", MessageTypeDefOf.NegativeEvent);
                return false;                
            }

            List<MenuItemBase> items = new List<MenuItemBase>();
            NullThrumAbilityBase ability;
            ability = new PawnApplyRetune();
            items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));

            ability = new PawnApplyCompress();
            items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
            
            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }
    }

}