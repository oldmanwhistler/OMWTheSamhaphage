using System;
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
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            string reason = "Unknown reason";

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
            }
            else if (target.Thing is Corpse corpse)
            {
                if (CorpseApplyResurrect.CanApplyOn(corpse, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobResurrectHallowbound(target)), $"Sacrifice self to transform {target.Label} to Hallowbound xenotype."));
                }        
                else
                {
                    items.Add(new MenuItemText(null, $"Can't implant Hallowbound. {reason}."));
                }
            }

            if (items.Count > 0)
            {
                BetterFloatMenu.Open(items, (item) =>
                {
                    if (item.Payload is Action action)
                    {
                        Log.Message($"BetterFloatMenu is invoking {item.Payload.ToString()}");
                        action.Invoke();
                        Log.Message($"BetterFloatMenu is done invoking {item.Payload.ToString()}");
                    }
                    else
                    {
                        Log.Error($"[OMW] Samhaphage AbilityFluxSpawnTargetCorpse does not know how to handle item.Payload={item.Payload?.ToString() ?? "null"}");
                    }
                });
            }
        }
        private void JobResurrectHallowbound(LocalTargetInfo target)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = target;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => AbilityResurrectHallowbound(t, actor);
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