using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyFlatten : NullThrumAbilityPawnOnly
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.flatten;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Scour {victim.LabelShort}'s mind, removing negative memories and preparing them for genetic manipulation.";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Flatten", false) ??
                                          BaseContent.BadTex;

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

        private static void PurgeGenesIncapableViolence(Pawn victim, Pawn caster)
        {
            Log.Debug($"Flatten::START::PurgeGenesIncapableViolence({victim.LabelShort})");
            if (victim?.genes == null) return;

            List<Gene> genes = victim.genes.GenesListForReading;
            for (int i = genes.Count - 1; i >= 0; i--)
            {
                bool removeGene = (genes[i].def.disabledWorkTags & WorkTags.Violent) != 0;
                if (!removeGene)
                {
                    foreach (GeneticTraitData traitData in genes[i].def.forcedTraits)
                    {
                        if (traitData.def == TraitDefOf.Kind)
                        {
                            removeGene = true;
                            break;
                        }
                    }
                }

                if (removeGene)
                {
                    Log.Debug(
                        $"Flatten::Purging gene {genes[i].def.defName} from {victim.LabelShort} because it disables violence.");
                    float value = ResonanceUtility.GeneResonanceValue(genes[i].def);
                    ResonanceUtility.Incr($"Purging gene that prevents violence: {genes[i].def.defName}", caster,
                        value);
                    victim.genes.RemoveGene(genes[i]);
                }
            }
            Log.Debug($"Flatten::DONE::PurgeGenesIncapableViolence({victim.LabelShort})");
        }

        private static void PurgeTraitsIncapableViolence(Pawn victim, Pawn caster)
        {
            Log.Debug($"Flatten::START::PurgeTraitsIncapableViolence({victim.LabelShort})");
            if (victim?.story?.traits == null) return;

            List<Trait> traits = victim.story.traits.allTraits;
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                bool removeTrait = false;
                if ((traits[i].def.disabledWorkTags & WorkTags.Violent) != 0)
                {
                    removeTrait = true;
                }
                else if (traits[i].def == TraitDefOf.Kind)
                {
                    removeTrait = true;
                }

                if (removeTrait)
                {
                    Log.Debug($"Flatten::Purging trait {traits[i].def.defName} from {victim.LabelShort} because it conflicts with Scoured Mind.");
                    float value = ResonanceUtility.TraitResonanceValue(traits[i]);
                    ResonanceUtility.Incr($"Purging trait that prevents violence: {traits[i].def.defName}", caster, value);
                    victim.story.traits.RemoveTrait(traits[i]);
                }
            }
            
            Log.Debug($"Flatten::DONE::PurgeTraitsIncapableViolence({victim.LabelShort})");
        }        

        private static void PurgeBackstoryIncapableViolence(Pawn victim, Pawn caster)
        {
            Log.Debug($"Flatten::START::PurgeBackstoryIncapableViolence({victim.LabelShort})");
            if (victim?.story == null) return;

            // Replace childhood if it disables violence
            if (victim.story.Childhood != null)
            {
                bool replace = ((victim.story.Childhood.workDisables & WorkTags.Violent) != 0);
                foreach (BackstoryTrait traitData in victim.story.Childhood.forcedTraits)
                {
                    if (traitData.def == TraitDefOf.Kind)
                    {
                        replace = true;
                        break;
                    }
                }                
                if (replace)
                {
                    Log.Debug(
                        $"Flatten::Purging childhood backstory {victim.story.Childhood.defName} from {victim.LabelShort} because it disables violence.");
                    victim.story.Childhood = OMW_BackstoryDefOf.OMW_Flatten_Childhood;
                }
            }


            // Replace adulthood if it disables violence
            if (victim.story.Adulthood != null)
            {
                bool replace = ((victim.story.Adulthood.workDisables & WorkTags.Violent) != 0);
                if (victim.story.Adulthood.forcedTraits != null)
                {
                    foreach (BackstoryTrait traitData in victim.story.Adulthood.forcedTraits)
                    {
                        if (traitData.def == TraitDefOf.Kind)
                        {
                            replace = true;
                            break;
                        }
                    }
                }
                if (replace)
                {
                    Log.Debug(
                        $"Flatten::Purging adulthood backstory {victim.story.Adulthood.defName} from {victim.LabelShort} because it disables violence.");
                    victim.story.Adulthood = OMW_BackstoryDefOf.OMW_Flatten_Adulthood;
                }
            }
            Log.Debug($"Flatten::DONE::PurgeBackstoryIncapableViolence({victim.LabelShort})");
        }    

        private static void PurgeIncapableViolence(Pawn victim, Pawn caster)
        {
            PurgeGenesIncapableViolence(victim, caster);
            PurgeTraitsIncapableViolence(victim, caster);
            PurgeBackstoryIncapableViolence(victim, caster);        
        }

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            
            Log.Debug($"START::Flatten::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

           if (victim.WorkTagIsDisabled(WorkTags.Violent))
            {
                PurgeIncapableViolence(victim, caster);
            }            

            if (!victim.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind) && (victim.genes != null))
            {
                Log.Debug($"Flatten - adding Scoured Mind to {victim.LabelShort}");           
                // have to make it a xenogene in case there are conflicting genes that effect suppressed traits
                victim.genes.AddGene(OMW_GeneDefOf.OMW_ScouredMind, xenogene: true);
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

            int brainDamage = 2;
            if (victim.health.hediffSet.GetBrain() != null)
            {
                victim.TakeDamage(new DamageInfo(DamageDefOf.Cut, brainDamage, 0, -1, caster,
                    victim.health.hediffSet.GetBrain()));
                Log.Debug($"Applied {brainDamage} brain damage to {victim.LabelShort} due to ability use.");
            }
           
            PurgeNegativeMemories(victim);

            Log.Debug($"Flatten - adding Unstable Mutation Minor to {victim.LabelShort}");
            victim.genes.AddGene(OMW_GeneDefOf.OMW_UnstableMutationMinor, xenogene: true);

            Log.Debug($"Flatten - refreshing dirty graphics on {victim.LabelShort}");
            OMWGenes.Refresh(victim);

            ResonanceUtility.Incr($"from flattening {victim.LabelShort}", caster, AbilityProp.value);

            Log.Debug($"DONE::Flatten::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");

            doOnComplete();
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

            if (victim.Dead)
            {
                reason = "Target is dead.";
                return false;
            }

            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            // Don't apply if the pawn isn't part of the colony, a prisoner or a slave. This is to prevent flattening random pawns in the world that are not part of the player's control.
            if (!victim.IsColonist && !victim.IsPrisoner && !victim.IsSlave)
            {
                reason = $"{victim.LabelShort} is not part of the colony, a prisoner or a slave and using this ability would cause hostility.";
                return false;
            }

            return true; 
        }
    }
}