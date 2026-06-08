using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnTargetPawn : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnTargetPawn()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnTargetPawn);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnTargetPawn  :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
                return false;
            }

            List<MenuItemBase> items = new List<MenuItemBase>();
            NullThrumAbilityBase ability;
            if (target.Thing is Pawn otherPawn)
            {
                if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
                {
                    ability = new FluxspawnFlickerStun();
                    items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                }
                ability = new PawnApplyEnwombSacrifice();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyHallowboundSacrifice();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
            }

            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }

    }
}