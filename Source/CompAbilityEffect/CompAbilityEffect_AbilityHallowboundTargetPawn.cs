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
            string reason;

            NullThrumAbilityBase ability;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent); 
            }
            else if (target.Thing is Pawn otherPawn)
            {
                ability = new ThingApplyStealFace();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                
                if (PawnApplyParasiticStinger.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnApplyParasiticStinger(target, parent.pawn)), $"Implant {otherPawn.Name} with Fluxspawn egg"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't implant Fluxspawn egg. {reason}"));
                }
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

        private void JobPawnApplyParasiticStinger(LocalTargetInfo target, Pawn actor)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityPawnApplyParasiticStinger(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityPawnApplyParasiticStinger(Thing thing, Pawn actor)
        {
            if (thing is Pawn target)
            {
                PawnApplyParasiticStinger sting = new PawnApplyParasiticStinger();
                sting.ApplySacrifice(target, actor, OMW_HediffDefOf.OMW_ParasiticImplantation, OMW_XenotypeDefOf.omw_fluxspawn_hiveling, 5);
            }
        }        
    }
}