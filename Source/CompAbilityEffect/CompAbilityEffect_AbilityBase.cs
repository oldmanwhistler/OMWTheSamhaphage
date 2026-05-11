using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class CompAbilityEffect_AbilityBase : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message($"{parent.pawn}.Apply called with target {target} and dest {dest}");
            base.Apply(target, dest);
            if (target.Thing is Pawn targetPawn)
            {
                // Prevent targeting hostiles if the ability is meant for the hive/allies
                if (targetPawn.HostileTo(parent.pawn))
                {
                    Messages.Message("Cannot target hostile creatures with this ability.", targetPawn, MessageTypeDefOf.RejectInput, false);
                    return;
                }

                if (targetPawn == parent.pawn)
                {
                    OpenMenu(target, dest);
                    return; // No need to move to target if target is self
                }
            }

            Log.Message($"{parent.pawn}.Apply is calling Job to move to target");
            this.Job(target, dest, this.parent.pawn);
        }
        
        public abstract void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest);

        // Does nothing, but makes moves the caster to the target before opening the menu.
        // Menu won't open if the caster isn't in range.
        public void Job(LocalTargetInfo target, LocalTargetInfo dest, Pawn caster)
        {
            Log.Message($"{parent.pawn}.Job");
            LocalTargetInfo targetClosure = target;
            LocalTargetInfo destClosure = dest;
            Job_ApproachAndInteract job = Job_ApproachAndInteract.CreateAndAssign(target, caster, 
                (actor, t) => OpenMenu(targetClosure, destClosure));
            if (job == null)
            {
                Log.Error($"Failed to create job for {caster} to approach and interact with {target}");
            }
            else
            {
                Log.Message($"Job created successfully for {caster} to approach and interact with {target}. Job: {job}");
            }
        }        

        public void OpenWindow(List<FloatMenuOption> options)
        {
            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }       
    }
}