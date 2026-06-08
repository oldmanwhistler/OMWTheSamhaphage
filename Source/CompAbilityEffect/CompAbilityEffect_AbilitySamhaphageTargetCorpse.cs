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
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;

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
                return false;
            }

            List<MenuItemBase> items = new List<MenuItemBase>();
            NullThrumAbilityBase ability;

            if (target.Thing is Corpse corpse)
            {
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyMute();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                if ((xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    ability = new ThingApplyNullify();
                    items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));
                }

                ability = new ThingApplyBootleg();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                if ((xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    ability = new ThingApplyExcise();
                    items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));
                }

                ability = new CorpseApplyRender();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                if ((xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    ability = new CorpseApplyResurrectEchovessel();
                    items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));
                }
            }

            if (items.Count > 0)
            {
                return DoOpenMenu(target, dest, items);
            }

            return false;
        }
    }
}