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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;
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
            }
            else if (target.Thing is Pawn otherPawn)
            {
                if (PawnApplyFlatten.CanApplyOn(otherPawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnFlatten(target, parent.pawn)), $"Flatten {otherPawn.LabelShort}"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't flatten {otherPawn.LabelShort}. {reason}."));
                }

                ability = new ThingApplyScrub();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                if (PawnApplyRetune.CanApplyOn(otherPawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnRetune(target, parent.pawn)), $"Retune {otherPawn.LabelShort}"));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't Retune {otherPawn.LabelShort}. {reason}."));
                }

                ability = new ThingApplyHarrow();
                items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));

                if (PawnApplyHallowbound.CanApplyOn(otherPawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnApplyHallowbound(target, parent.pawn)), $"Transform {otherPawn.LabelShort} to Hallowbound."));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't transform {otherPawn.LabelShort} to Hallowbound. {reason}"));
                }                

                if (PawnApplyParasiticStinger.CanApplyOn(otherPawn, parent.pawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnApplyParasiticStinger(target, parent.pawn)), $"Implant {otherPawn.LabelShort} with Fluxspawn egg"));
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
                        Log.Message($"BetterFloatMenu is invoking {action.Method.Name}");
                        action.Invoke();
                    }
                });
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
                apply.Apply(target, actor);
            }
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

        private void JobPawnFlatten(LocalTargetInfo target, Pawn actor)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityPawnApplyFlatten(t, actor);
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
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) =>
            {
                if (t is Pawn victim) PawnApplyRetune.Apply(victim, actor);
            };
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}