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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            NullThrumAbilityBase ability;

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
                return;
            }
            else if (target.Thing is Pawn otherPawn)
            {
                ability = new PawnTeratogenicHealing();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));

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
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Message($"BetterFloatMenu is invoking {item.Payload.ToString()}");
                        action.Invoke();
                        Log.Message($"BetterFloatMenu is done invoking {item.Payload.ToString()}");
                    }
                    else
                    {
                        Log.Error($"[OMW] Samhaphage AbilityFluxSpawnTargetPawn does not know how to handle item.Payload={item.Payload?.ToString() ?? "null"}");
                    }
                });
            }
        }

    }
}