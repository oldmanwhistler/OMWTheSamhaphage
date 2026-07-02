using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityHallowboundTargetCorpse : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHallowboundTargetCorpse()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityHallowboundTargetCorpse);
        }
    }

    public class CompAbilityEffect_AbilityHallowboundTargetCorpse :  CompAbilityEffect_AbilityBase

    {
        public override bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            XenotypeDef xeno = parent.pawn.genes.Xenotype;


            if ((xeno != OMW_XenotypeDefOf.omw_hallowbound) && (xeno != OMW_XenotypeDefOf.omw_echovessel))
            {
                // hybrids lose the ability
                Messages.Message(
                    $"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Echovessel orHallowbound abilities.",
                    MessageTypeDefOf.NegativeEvent);
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

                ability = new ThingApplyCrosstalk();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyAttenuate();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new CorpseApplyResurrectEchovessel();
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