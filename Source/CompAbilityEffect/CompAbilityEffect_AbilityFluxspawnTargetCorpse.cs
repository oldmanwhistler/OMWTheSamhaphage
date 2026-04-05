using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnTargetCorpse : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnTargetCorpse()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnTargetCorpse);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnTargetCorpse :  CompAbilityEffect_AbilityBase

    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            string reason = "Unknown reason";

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
                OpenWindow(options);
                return;                
            }
            if (target.Thing is Corpse corpse)
            {
                if (CorpseApplyResurrect.CanApplyOn(corpse, out reason))
                {
                    options.Add(new FloatMenuOption($"Sacrifice self to transform {target.Label} to Hallowbound xenotype.",
                        () => JobResurrectHallowbound(target)));
                }        
                else
                {
                    options.Add(new FloatMenuOption($"Can't implant Hallowbound. {reason}.", null)
                    { Disabled = true });
                }
            }
            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }
        private void JobResurrectHallowbound(LocalTargetInfo target)
        {
            Job_OMW_XenotypeAbility job = new Job_OMW_XenotypeAbility();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onArrival = (actor, t) => AbilityResurrectHallowbound(t, actor);
            parent.pawn.jobs.TryTakeOrderedJob(job);
        }

        private void AbilityResurrectHallowbound(Thing thing, Pawn actor)
        {
            Corpse corpse = thing as Corpse;
            CorpseApplyResurrect resurrect = new CorpseApplyResurrect();
            resurrect.ApplySacrifice(corpse, actor, OMW_HediffDefOf.OMW_SilentServitude,
                OMW_XenotypeDefOf.omw_hallowbound);
        }
    }
}