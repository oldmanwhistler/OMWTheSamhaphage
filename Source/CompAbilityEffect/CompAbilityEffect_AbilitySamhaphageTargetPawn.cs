using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilitySamhaphageTargetPawn : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySamhaphageTargetPawn()
        {
            this.compClass = typeof(CompAbilityEffect_AbilitySamhaphageTargetPawn);
        }
    }

    public class CompAbilityEffect_AbilitySamhaphageTargetPawn :  CompAbilityEffect_AbilityBase

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

            if (target.Thing is Pawn otherPawn)
            {
                ability = new PawnApplyFlatten();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyRetune();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyCompress();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                if ((xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    ability = new ThingApplyNullify();
                    items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                }


                ability = new ThingApplyBootleg();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                if ((xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    ability = new ThingApplyExcise();
                    items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                }

                ability = new ThingApplyMute();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyUnmute();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyAttenuate();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyHallowbound();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new PawnApplyInfestFluxspawnHiveling();
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