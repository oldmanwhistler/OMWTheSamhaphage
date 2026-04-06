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
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;

            NullThrumAbilityBase ability;

            if (xeno != OMW_XenotypeDefOf.omw_hallowbound)
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
            }
            else if (target.Thing is Pawn otherPawn)
            {
                if (PawnTakeXenogenes.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Take xenogenes from {otherPawn.Name}",
                        () => JobPawnTakeXenogenes(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't take xenogenes. {reason}", null) { Disabled = true });
                }

                ability = new PawnTeratogenicHealing();
                options.Add(ability.NewFloatMenuOptionPawn(target, otherPawn, parent.pawn));
                
                if (PawnApplyParasiticStinger.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Implant {otherPawn.Name} with Fluxspawn egg",
                        () => JobPawnApplyParasiticStinger(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't implant Fluxspawn egg. {reason}", null) { Disabled = true });
                }
            }

            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }

        private void JobPawnTakeXenogenes(LocalTargetInfo target, Pawn actor)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityPawnTakeXenogenes(t, actor);                        
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }
        
        private void AbilityPawnTakeXenogenes(Thing thing, Pawn actor)
        {
            if (thing is Pawn target)
            {
                PawnTakeXenogenes take = new PawnTakeXenogenes();
                take.ApplySacrifice(target, actor);
            }
        }

        private void JobEchovessel(LocalTargetInfo target)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityEchovessel(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityEchovessel(Thing thing, Pawn actor)
        {
            Corpse corpse = thing as Corpse;
            CorpseApplyResurrect resurrect = new CorpseApplyResurrect();
            resurrect.Apply(corpse, OMW_HediffDefOf.OMW_SilentServitude,
                OMW_XenotypeDefOf.omw_echovessel);
        }
      
        private void JobCorpseTakeXenogenes(LocalTargetInfo target, Pawn actor)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityCorpseTakeXenogenes(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityCorpseTakeXenogenes(Thing thing, Pawn actor)
        {
            if (thing is Corpse corpse)
            {
                CorpseTakeXenogenes take = new CorpseTakeXenogenes();
                if (take.Apply(corpse, actor))
                {
                    corpse.Destroy(DestroyMode.Vanish);
                }
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