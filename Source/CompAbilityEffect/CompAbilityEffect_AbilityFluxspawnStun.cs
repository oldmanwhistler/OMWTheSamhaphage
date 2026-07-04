using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnStun : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnStun()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnStun);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnStun :  CompAbilityEffect
    {
        protected static Logger Log = new Logger("CompAbilityEffect");
        
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Debug($"{parent.pawn}.Apply called with target {target} and dest {dest}");
            base.Apply(target, dest);
            Log.Debug($"{parent.pawn}.Apply is calling Job to move to target");
            this.Job(target, dest, this.parent.pawn);
        }

        public void Job(LocalTargetInfo target, LocalTargetInfo dest, Pawn caster)
        {
            Log.Debug($"{parent.pawn}.Job");

            Job_ApproachAndInteract job = Job_ApproachAndInteract.CreateAndAssign(target, caster,
                (actor, t) => ApplyStun(target.Pawn, caster));
            if (job == null)
            {
                Log.Error($"Failed to create job for {caster} to approach and interact with {target}");
            }
            else
            {
                Log.Debug($"Job created successfully for {caster}. Job: {job}");
            }
        }


        private void ApplyStun(Pawn target, Pawn caster)
        {
                 // Apply a custom temporary unconsciousness hediff
            HediffDef stunHediffDef = OMW_HediffDefOf.OMW_TemporaryUnconscious;
            if (stunHediffDef != null)
            {
                // Remove existing stun hediff if present
                Hediff existingHediff = target.health.hediffSet.GetFirstHediffOfDef(stunHediffDef);
                if (existingHediff != null)
                {
                    target.health.RemoveHediff(existingHediff);
                }

                Log.Debug($"About to add OMW_TemporaryUnconscious to {target.LabelShort}");

                // Add new stun hediff with duration
                Hediff stunHediff = HediffMaker.MakeHediff(stunHediffDef, target);
                target.health.AddHediff(stunHediff);

                Log.Debug($"Applied OMW_TemporaryUnconscious to {target.LabelShort}");
            } else
            {
                Log.Error("OMW_TemporaryUnconscious hediff not found. Stun effect will not be applied.");
            }
        }        
    }
}