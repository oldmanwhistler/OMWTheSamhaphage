using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityCradlemoldTargetCorpse : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityCradlemoldTargetCorpse()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityCradlemoldTargetCorpse);
        }
    }

    public class CompAbilityEffect_AbilityCradlemoldTargetCorpse :  CompAbilityEffect_AbilityBase

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

            if (target.Thing is Corpse corpse)
            {
                ability = new ThingApplySample();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));                
                
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));
            }

            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }        
    }
}