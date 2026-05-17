using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityHallowboundTargetPawn : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHallowboundTargetPawn()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityHallowboundTargetPawn);
        }
    }

    public class CompAbilityEffect_AbilityHallowboundTargetPawn :  CompAbilityEffect_AbilityBase

    {
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            NullThrumAbilityBase ability;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent);
                return;
            }
            else if (target.Thing is Pawn otherPawn)
            {
                ability = new ThingApplySample();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyCrosstalk();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                
                ability = new PawnApplyInfestFluxspawnHiveling();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        action.Invoke();
                    }
                });
            }
        }

    }
}
