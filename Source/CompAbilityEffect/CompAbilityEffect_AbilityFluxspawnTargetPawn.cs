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
            List<MenuItemBase> items = new List<MenuItemBase>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;            
            string reason = "Unknown reason";
            NullThrumAbilityBase ability;

            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                Messages.Message($"{parent.pawn.LabelShort} is a {xeno} Xenotype and can't use Fluxspawn abilities.", MessageTypeDefOf.NegativeEvent);
            }
            else if (target.Thing is Pawn otherPawn)
            {
                ability = new PawnTeratogenicHealing();
                items.Add(ability.NewMenuItemIcon(target, parent.pawn));

                if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
                {
                    ability = new FluxspawnFlickerStun();
                    items.Add(ability.NewMenuItemIconPawn(target, otherPawn, parent.pawn));
                }
                if (PawnApplyPregnant.CanApplyOn(otherPawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobEnwomb(target)), $"Sacrifice self to transform {otherPawn.LabelShort} to Cradlemold xenotype."));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't transform {otherPawn.LabelShort} to Cradlemold. {reason}"));
                }

                if (PawnApplyHallowbound.CanApplyOn(otherPawn, out reason))
                {
                    items.Add(new MenuItemText((Action)(() => JobPawnApplyHallowbound(target, parent.pawn)), $"Sacrifice self to transform {otherPawn.LabelShort} to Hallowbound xenotype."));
                }
                else
                {
                    items.Add(new MenuItemText(null, $"Can't transform {otherPawn.LabelShort} to Hallowbound. {reason}"));
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
                        Log.Error($"[OMW] Samhaphage AbilityFluxSpawnTargetPawn does not know how to handle item.Payload={item.Payload?.ToString() ?? "null"}");
                    }
                });
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