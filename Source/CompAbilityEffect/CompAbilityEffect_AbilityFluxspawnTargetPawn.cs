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
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            string reason = "Unknown reason";
            NullThrumAbilityBase ability;

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
                OpenWindow(options);
                return;                
            }
            if (target.Thing is Pawn otherPawn)
            {
                if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
                {
                    ability = new FluxspawnFickerStun();
                    options.Add(ability.NewFloatMenuOptionPawn(target, otherPawn, parent.pawn));
                }
                if (PawnApplyPregnant.CanApplyOn(otherPawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Sacrifice self to transform {otherPawn.LabelShort} to Cradlemold xenotype.",
                        () => JobEnwomb(target)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't transform {otherPawn.LabelShort} to Cradlemold. {reason}", null) { Disabled = true });
                }

                if (PawnApplyHallowbound.CanApplyOn(otherPawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Sacrifice self to transform {otherPawn.LabelShort} to Hallowbound xenotype.",
                        () => JobPawnApplyHallowbound(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't transform {otherPawn.LabelShort} to Hallowbound. {reason}",
                        null) { Disabled = true });
                }
            }

            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }

        private void JobEnwomb(LocalTargetInfo target)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityEnwomb(t, actor);            
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }
        
        private void AbilityEnwomb(Thing thing, Pawn father)
        {
            if (thing is Pawn mother)
            {
                PawnApplyPregnant prego = new PawnApplyPregnant();
                prego.ApplySacrifice(mother, father, OMW_HediffDefOf.OMW_SilentServitude,
                    OMW_XenotypeDefOf.omw_cradlemold);
            }
        }

        private void JobPawnApplyHallowbound(LocalTargetInfo target, Pawn actor)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityPawnApplyHallowbound(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityPawnApplyHallowbound(Thing thing, Pawn actor)
        {
            if (thing is Pawn target)
            {
                PawnApplyHallowbound apply = new PawnApplyHallowbound();
                apply.ApplySacrifice(target, actor);
            }
        }
    }
}