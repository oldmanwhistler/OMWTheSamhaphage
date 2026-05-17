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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
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
                ability = new CorpseApplyResurrectHallowbound();
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
                    }
                    else
                    {
                        Log.Error($"[OMW] Samhaphage AbilityFluxSpawnTargetCorpse does not know how to handle item.Payload={item.Payload?.ToString() ?? "null"}");
                    }
                });
            }
        }
    }
}