using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnSelf);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnSelf :  CompAbilityEffect_AbilityBase

    {
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            NullThrumAbilityBase ability;

            // Add the gene state as a non-interactive header

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) &&
                (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
                return;
            }

            ability = new PawnTeratogenicHealing();
            items.Add(ability.NewMenuItemIcon(target, parent.pawn));

            if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_brute)
            {
                ability = new FluxspawnShiftHiveling();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
                ability = new FluxspawnShiftFlicker();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
            }
            else if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
            {
                ability = new FluxspawnShiftHiveling();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
                ability = new FluxspawnShiftBrute();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
            }
            else if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_hiveling)
            {
                ability = new FluxspawnShiftBrute();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
                ability = new FluxspawnShiftFlicker();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Debug($"BetterFloatMenu is invoking {item.Payload.ToString()}");
                        action.Invoke();
                        Log.Debug($"BetterFloatMenu is done invoking {item.Payload.ToString()}");
                    } else
                    {
                        Log.Error($"[OMW] Samhaphage AbilityFluxSpawn does not know how to handle item.Payload={item.Payload.ToString()}");
                    }
                });
            }
        }
    }
}