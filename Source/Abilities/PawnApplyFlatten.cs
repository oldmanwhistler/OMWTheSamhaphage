using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public class PawnApplyFlatten
    {
        private static void PurgeNegativeMemories(Pawn pawn)
        {
            var memoryHandler = pawn?.needs?.mood?.thoughts?.memories;
            if (memoryHandler == null) return;

            List<Thought_Memory> memories = memoryHandler.Memories;

            for (int i = memories.Count - 1; i >= 0; i--)
            {
                // Example logic: Remove any thought that has a negative mood impact
                if (memories[i].MoodOffset() < 0f)
                {
                    memoryHandler.RemoveMemory(memories[i]);
                }
            }
        }

        public bool Apply(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (!victim.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind))
            {
                victim.genes.AddGene(OMW_GeneDefOf.OMW_ScouredMind, false);
            }

            if (victim.InMentalState)
            {
                victim.mindState.mentalStateHandler.Reset();
            }

            // OMW_SilentServitude prevents repeated calls to flatten
            Hediff hediff_Flatten = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_SilentServitude, caster);            
            victim.health.AddHediff(hediff_Flatten);

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            PurgeNegativeMemories(victim);

            victim.genes.AddGene(OMW_GeneDefOf.OMW_UnstableMutationMinor, true);

            OMWGenes.RemoveDisabledGenes(victim);
            OMWGenes.Refresh(victim);

            OMWGenes.IncrResonance(caster);


            return true;
        }

        public static bool CanApplyOn(Pawn p, out string reason)
        {
            reason = "unknown reason";

            if (p == null) 
            {
                reason = "Target is null.";
                return false;
            }            
            // Check if target is a not already Flatten
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} has a scoured mind.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
            {
                reason = $"{p.LabelShort} is affected by Silent Servitude.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance";
                return false;
            }

            return true;
        }
    }
}