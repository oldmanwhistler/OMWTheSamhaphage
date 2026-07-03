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
    public class PawnApplyPhaseLock : NullThrumAbilityPawnOnly
    {
        protected int complexityCost;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.phaselock;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Phase lock {victim.LabelShort} with Fluxspawn embryos.\nThe process is lethal and will birth a litter of Fluxspawn.";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/PhaseLock", false) ??
                                          BaseContent.BadTex;
        public override bool IsLethal => true;
        
        public virtual HediffDef TargetHediffDef => OMW_HediffDefOf.OMW_ParasiticImplantation;
        public virtual XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_fluxspawn_hiveling;        

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            // if (!ResonanceUtility.Decr(caster, complexityCost))
            // {
            //     Log.Error(
            //         $"[OMW_Samhaphage] Failed to decrement resonance for {caster.LabelShort} during {AbilityType}. This indicates a logic error where CanApplyOnPawn did not prevent the ability.");
            //     doOnComplete();
            //     return;
            // }

            if (victim.InMentalState)
            {
                victim.mindState.mentalStateHandler.CurState.RecoverFromState();
            }            

            // Based on Hospitality: Room Service by ledronas and Claude: https://steamcommunity.com/sharedfiles/filedetails/?id=3756771316

            // Find the bed with proper validation for Lovin job
            Building_Bed foundBed = (Building_Bed)GenClosest.ClosestThingReachable(
                victim.Position,
                victim.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.OnCell,
                TraverseParms.For(victim),
                9999f,
                thing => thing is Building_Bed b &&
                         b.def.building.bed_humanlike &&
                         b.SleepingSlotsCount > 1 &&
                         // Check bed ownership: unowned bed OR both pawns own it
                         (b.OwnersForReading.Count == 0 || 
                          b.OwnersForReading.Contains(victim) || b.OwnersForReading.Contains(caster)) && 
                         // Check both pawns can reach and use the bed
                         victim.CanReach(b, PathEndMode.ClosestTouch, Danger.Some) &&
                         caster.CanReach(b, PathEndMode.ClosestTouch, Danger.Some)
            );
         
            if (foundBed == null)
            {
                // Send a message to the player in-game explaining why the ability failed
                Messages.Message($"There are no valid double beds available for {victim.LabelShort}.", MessageTypeDefOf.RejectInput, false);
                return;
            }            

            var job = JobMaker.MakeJob(JobDefOf.Lovin, victim, foundBed);
            caster.jobs.StartJob(job, JobCondition.InterruptForced, resumeCurJobAfterwards: false);

            if (caster.jobs.curJob == job && caster.jobs.curDriver != null)
            {
                // Wait until the pawns actually finish the lovin' job (i.e. they made it to the
                // bed and went through with it) before charging payment or granting thoughts -
                caster.jobs.curDriver.AddFinishAction(condition =>
                    OnLovinFinished(caster, victim, condition));
            }
        }

        private static void PurgeNegativeMemories(Pawn victim)
        {
            var memoryHandler = victim?.needs?.mood?.thoughts?.memories;
            if (memoryHandler == null) return;

            List<Thought_Memory> memories = memoryHandler.Memories;

            for (int i = memories.Count - 1; i >= 0; i--)
            {
                // Remove any thought that has a negative mood impact
                if (memories[i].MoodOffset() < 0f)
                {
                    memoryHandler.RemoveMemory(memories[i]);
                }
            }
        }        

        private static void OnLovinFinished(Pawn caster, Pawn victim, JobCondition condition)
        {
            //if (grantedTempRelation) RemoveTempLoverRelation(pawn, guest);

            if (condition != JobCondition.Succeeded) return; // interrupted/errored partway - no payment, no thoughts

            int brainDamage = 4;
            if (victim.health.hediffSet.GetBrain() != null)
            {
                MoteMaker.MakeStaticMote(victim.TrueCenter(), victim.Map, ThingDefOf.Mote_ThoughtGood);
                victim.TakeDamage(new DamageInfo(DamageDefOf.Cut, brainDamage, 0, -1, caster,
                    victim.health.hediffSet.GetBrain()));
                caster.skills?.Learn(SkillDefOf.Melee, 50f);
            }
            caster.skills?.Learn(SkillDefOf.Social, 50f);
            caster.needs?.mood?.thoughts?.memories?.TryGainMemory(OMW_ThoughtDefOf.OMW_PhaseLock_Flush, victim);

            if (!victim.Dead)
            {
                PurgeNegativeMemories(victim);
                victim.needs?.mood?.thoughts?.memories?.TryGainMemory(OMW_ThoughtDefOf.OMW_PhaseLock_Surrender, caster);
            }

            if (ColonyUtility.CradlemoldPregenancy(caster, victim))
            {
                MoteMaker.MakeStaticMote(caster.TrueCenter(), caster.Map, ThingDefOf.Mote_ThoughtBad);
            }
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (victim.HostileTo(caster))
            {
                reason = $"{victim.LabelShort} is hostile.";
                return false;
            }

            if (victim.HasActiveGene(GeneDefOf.Deathless))
            {
                reason = $"{victim.LabelShort} is deathless.";
                return false;
            }

            // check if they are male
            if (victim.gender != Gender.Male)
            {
                reason = $"{victim.LabelShort} is not male.";
                return false;
            }

            // victim must not be drafted
            if (victim.Drafted)
            {
                reason = $"{victim.LabelShort} is drafted.";
                return false;
            }

            // victim must be awake
            if (victim.Downed)
            {
                reason = $"{victim.LabelShort} is downed.";
                return false;
            }
            
            complexityCost = OMWGenes.CalculateComplexity(victim, true, caster.genes.Xenotype);
            if (!ResonanceUtility.HasAvailable(caster, complexityCost))
            {
                reason =
                    $"{caster.LabelShort} does not have enough resonance to phase lock {victim.LabelShort}. Requires {complexityCost} resonance.";
                return false;
            }            

            return CanApplyLimitXenotype(TargetXenotype, out reason);
        }
    }
}