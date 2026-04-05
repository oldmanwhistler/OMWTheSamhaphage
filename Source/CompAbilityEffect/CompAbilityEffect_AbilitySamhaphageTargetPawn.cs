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
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;

            if (xeno == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                // this only works because Sovereign and Samhaphage have the same menu
                OMWXenotypes.ThereCanOnlyBeOne();
                xeno = parent.pawn.genes.Xenotype;
            }

            if ((xeno != OMW_XenotypeDefOf.omw_samhaphage) && (xeno != OMW_XenotypeDefOf.omw_sovereign_stillness))
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
            }
            else if (target.Thing is Pawn otherPawn)
            {
                if (PawnApplyFlatten.CanApplyOn(otherPawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Flatten {otherPawn.LabelShort}",
                        () => JobPawnFlatten(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't flatten {otherPawn.LabelShort}. {reason}.", null)
                        { Disabled = true });
                }

                if (PawnApplyRetune.CanApplyOn(otherPawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Retune {otherPawn.LabelShort}",
                        () => JobPawnRetune(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't Retune {otherPawn.LabelShort}. {reason}.", null)
                        { Disabled = true });
                }                

                if (PawnTakeXenogenes.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Take xenogenes from {otherPawn.LabelShort}",
                        () => JobPawnTakeXenogenes(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't take xenogenes. {reason}", null) { Disabled = true });
                }

                if (PawnApplyHallowbound.CanApplyOn(otherPawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Transform {otherPawn.LabelShort} to Hallowbound.",
                        () => JobPawnApplyHallowbound(target, parent.pawn)));
                }
                else
                {
                    options.Add(new FloatMenuOption($"Can't transform {otherPawn.LabelShort} to Hallowbound. {reason}", null) { Disabled = true });
                }                

                if (PawnApplyParasiticStinger.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    options.Add(new FloatMenuOption($"Implant {otherPawn.LabelShort} with Fluxspawn egg",
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
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityPawnTakeXenogenes(t, actor);                        
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
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityEchovessel(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityEchovessel(Thing thing, Pawn actor)
        {
            Corpse corpse = thing as Corpse;
            CorpseApplyResurrect resurrect = new CorpseApplyResurrect();
            resurrect.Apply(corpse, OMW_HediffDefOf.OMW_SilentServitude,
                OMW_XenotypeDefOf.omw_echovessel);
        }

        private void JobPawnApplyHallowbound(LocalTargetInfo target, Pawn actor)
        {
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityPawnApplyHallowbound(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityPawnApplyHallowbound(Thing thing, Pawn actor)
        {
            if (thing is Pawn target)
            {
                PawnApplyHallowbound apply = new PawnApplyHallowbound();
                apply.Apply(target, actor);
            }
        }        
      
        private void JobCorpseTakeXenogenes(LocalTargetInfo target, Pawn actor)
        {
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityCorpseTakeXenogenes(t, actor);
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
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityPawnApplyParasiticStinger(t, actor);
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

        private void JobPawnFlatten(LocalTargetInfo target, Pawn actor)
        {
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityPawnApplyFlatten(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityPawnApplyFlatten(Thing thing, Pawn actor)
        {
            if (thing is Pawn victim)
            {
                PawnApplyFlatten flatten = new PawnApplyFlatten();
                flatten.Apply(victim, actor);
            }
        }

        private void JobPawnRetune(LocalTargetInfo target, Pawn actor)
        {
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actpr, t) =>
            {
                if (t is Pawn victim) PawnApplyRetune.Apply(victim, actor);
            };
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}