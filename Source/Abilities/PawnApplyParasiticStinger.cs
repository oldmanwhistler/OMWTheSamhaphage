using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using AlphaGenes;

namespace OMW_Samhaphage
{
    public class PawnApplyParasiticStinger
    {
        // Taken from AlphaGenes parasiticStinger
        public void Apply(Pawn pawn, Pawn implanter, HediffDef targetHediffDef, XenotypeDef targetXenotypeDef, int numBabies = 1)
        {

            // Based on https://github.com/juanosarg/AlphaGenes/blob/d6f14ee6106ce01351c86eb369703edde65bce66/1.6/Source/AlphaGenes/AlphaGenes/Ability%20Comps/CompAbilityEffect_ParasiticStinger.cs
            // (c) juanosarg.

            if (pawn == null) return;
            HealthUtility.DamageUntilDowned(pawn);

            pawn.health.AddHediff(targetHediffDef);
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(targetHediffDef);

            // This HediffComp will be used to pass the mother's genes and other info to the eggs, so they can inherit them. 
            HediffComp_ParasitesXenotype comp = hediff.TryGetComp<HediffComp_ParasitesXenotype>();

            // TODO: should probably just use a pawnkindDef instead of targetXenotypeDef
            comp.motherDef = implanter.kindDef;
            comp.mother = implanter;
            comp.motherFaction = implanter.Faction;            
            // unlike AlphaGenes, the baby will be something different than the mother.
            comp.motherXenotypeDef = targetXenotypeDef;
            comp.numBabies = numBabies;

            FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, new Vector3(0f, 0f, 0.26f));

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(
                (Thought_Memory)ThoughtMaker.MakeThought(InternalDefOf.AG_Parasite), implanter);

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(
                (Thought_Memory)ThoughtMaker.MakeThought(InternalDefOf.AG_Parasite_Social), implanter);

            for (int i = 0; i < 20; i++)
            {
                IntVec3 c;
                CellFinder.TryFindRandomReachableCellNearPosition(pawn.Position, pawn.Position, pawn.Map, 2,
                    TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                FilthMaker.TryMakeFilth(c, pawn.Map, ThingDefOf.Filth_Blood);
            }
        }

        public void ApplySacrifice(Pawn target, Pawn caster, HediffDef targetHediffDef, XenotypeDef targetXenotypeDef,
            int numBabies = 1)
        {
            string msg = $"{target.LabelShort} has been implanted by {caster.LabelShort} and will die when the egg(s) hatch.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                Apply(target, caster, targetHediffDef, targetXenotypeDef, numBabies);
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(target, sacrificeAction);
        }

        public static bool CanApplyOn(Pawn pawn, Pawn implanter, out string reason)
        {
            reason = "unknown reason";
            if (pawn == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (pawn.HostileTo(implanter))
            {
                reason = $"{pawn.LabelShort} is hostile.";
                return false;
            }

            if (pawn.HasActiveGene(GeneDefOf.Deathless))
            {
                reason = $"{pawn.LabelShort} is deathless.";
                return false;
            }

            if (OMWGenes.HasNullThrum(pawn))
            {
                reason = $"{pawn.LabelShort} is part of the Null-Thrum.";
                return false;
            }

            return true;
        }
    }
}