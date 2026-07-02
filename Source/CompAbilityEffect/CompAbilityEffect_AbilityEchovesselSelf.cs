using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityEchovesselSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEchovesselSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityEchovesselSelf);
        }
    }

    public class CompAbilityEffect_AbilityEchovesselSelf :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            if (xeno != OMW_XenotypeDefOf.omw_echovessel)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Echovessel abilities.", MessageTypeDefOf.NegativeEvent);
                return false;                
            }

            List<MenuItemBase> items = new List<MenuItemBase>();
            NullThrumAbilityBase ability;
            ability = new ThingApplyScrub();
            items.Add(ability.NewMenuItemIconPawn(target, parent.pawn, parent.pawn));
            
            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }
    }

}