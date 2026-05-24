using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyFlatten : NullThrumAbilityPawnOnly
    {
        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Flatten;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Scour {victim.LabelShort}'s mind, removing negative memories and preparing them for genetic manipulation.";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Flatten");

        private static void PurgeNegativeMemories(Pawn victim)
        {
            Log.Debug($"Flatten::START::PurgeNegativeMemories({victim.LabelShort})");
            var memoryHandler = victim?.needs?.mood?.thoughts?.memories;
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
            Log.Debug($"Flatten::DONE::PurgeNegativeMemories({victim.LabelShort})");
        }

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            Log.Debug($"START::Flatten::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return false;

            if (!victim.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind) && (victim.genes != null))
            {
                Log.Debug($"Flatten - adding Scoured Mind to {victim.LabelShort}");                
                victim.genes.AddGene(OMW_GeneDefOf.OMW_ScouredMind, xenogene: false);
                Log.Debug($"Flatten - done adding Scoured Mind to {victim.LabelShort}");
            }

            if (victim.InMentalState)
            {
                Log.Debug($"Flatten - about to remove mental state from {victim.LabelShort}");
                victim.mindState.mentalStateHandler.CurState.RecoverFromState();
                Log.Debug($"Flatten - done remove mental state from {victim.LabelShort}");
            }

            if (!victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
            {
                Log.Debug($"Flatten - {victim.LabelShort} is getting Silent Servitude hediff");
                // OMW_SilentServitude prevents repeated calls to flatten
                Hediff hediff_Flatten = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_SilentServitude, caster);            
                victim.health.AddHediff(hediff_Flatten);
                Log.Debug($"Flatten - {victim.LabelShort} done Silent Servitude hediff");
            }

            PurgeNegativeMemories(victim);

            Log.Debug($"Flatten - adding Unstable Mutation Minor to {victim.LabelShort}");
            victim.genes.AddGene(OMW_GeneDefOf.OMW_UnstableMutationMinor, xenogene: true);

            Log.Debug($"Flatten - refreshing dirty graphics on {victim.LabelShort}");
            OMWGenes.Refresh(victim);

            ResonanceUtility.Incr($"from flattening {victim.LabelShort}",  caster, OMW_Mod.settings.gainFlatten);

            Log.Debug($"DONE::Flatten::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            return true;
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }

            if (OMWGenes.HasScouredMind(victim))
            {
                reason = $"{victim.LabelShort} already has a scoured mind.";
                return false;
            }

            return true;
        }
        
        // Like CanApplyOnPawn except it will return true if the pawn already has a scoured mind
        public bool HasOrCanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            // Only care about SilentServitude if they don't have flatten
            // This might not matter and just be a left-over when I was first coding it.
            if (!OMWGenes.HasScouredMind(victim))
            {
                if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
                {
                    reason = $"{victim.LabelShort} is affected by Silent Servitude.";
                    return false;
                }
            }

            if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{victim.LabelShort} is affected by Genetic Dissonance.";
                return false;
            }
            return true; 
        }
    }
}