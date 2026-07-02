using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityEchovesselTargetPawn : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEchovesselTargetPawn()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityEchovesselTargetPawn);
        }
    }

    public class CompAbilityEffect_AbilityEchovesselTargetPawn :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            if (xeno != OMW_XenotypeDefOf.omw_echovessel)
            {
                // hybrids lose the ability
                Messages.Message(
                    $"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Echovessel abilities.",
                    MessageTypeDefOf.NegativeEvent);
                return false;
            }            

            List<MenuItemBase> items = new List<MenuItemBase>();
            NullThrumAbilityBase ability;
            if (target.Thing is Pawn otherPawn)
            {
                ability = new ThingApplyScrub();
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
