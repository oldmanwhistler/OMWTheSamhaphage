using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnTargetCorpse : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnTargetCorpse()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnTargetCorpse);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnTargetCorpse :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            NullThrumAbilityBase ability;
            XenotypeDef xeno = parent.pawn.genes.Xenotype;            

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
            }
            else if (target.Thing is Corpse corpse)
            {
                ability = new ThingApplyAttenuate();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new CorpseApplyResurrectHallowbound();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
            }

            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }
    }
}