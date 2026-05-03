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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;

            NullThrumAbilityBase ability;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Hallowbound abilities.", MessageTypeDefOf.NegativeEvent); 
            }
            else if (target.Thing is Corpse corpse)
            {
                ability = new ThingApplyStealFace();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));                
                
                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));

                ability = new CorpseApplyResurrectEchovessel();
                items.Add(ability.NewMenuItemIconCorpse(target, corpse, parent.pawn));
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Message($"BetterFloatMenu is invoking {item.Payload.ToString()}");
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