using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilitySamhaphageTargetCorpse : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySamhaphageTargetCorpse()
        {
            this.compClass = typeof(CompAbilityEffect_AbilitySamhaphageTargetCorpse);
        }
    }

    public class CompAbilityEffect_AbilitySamhaphageTargetCorpse :  CompAbilityEffect_AbilityBase

    {
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            NullThrumAbilityBase ability;

            if (xeno == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                // this only works because Sovereign and Samhaphage have the same menu
                OMWXenotypes.ThereCanOnlyBeOne();
                xeno = parent.pawn.genes.Xenotype;
            }

            if ((xeno != OMW_XenotypeDefOf.omw_samhaphage) && (xeno != OMW_XenotypeDefOf.omw_sovereign_stillness))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Samhaphage abilities.", MessageTypeDefOf.NegativeEvent);
                return;
            }
            else if (target.Thing is Corpse corpse)
            {
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyMute();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyAttenuate();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyBootleg();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));                
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Debug($"BetterFloatMenu is invoking {item.Payload.ToString()}");
                        action.Invoke();
                    }
                });
            }
        }

    }
}