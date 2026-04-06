using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OMW_Samhaphage
{
    public class Job_ApproachAndInteract : Job
    {
        public static Job_ApproachAndInteract CreateAndAssign(LocalTargetInfo target, Pawn caster, Action<Pawn, Thing> onInteract)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.ignoreForbidden = true;
            job.count = 1;
            job.targetA = target;
            job.onInteract = onInteract;
            caster.jobs.EndCurrentJob(JobCondition.InterruptForced);
            caster.jobs.TryTakeOrderedJob(job);            
            return job;
        }
        public Action<Pawn, Thing> onInteract;        
    }

    public class JobDriver_ApproachAndInteract : JobDriver
    {
        // Use a property to avoid repeated casting
        private Job_ApproachAndInteract SpecificJob;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Reserve the target globally. 
            // 1 is the max pawns, -1 is 'all stacks'. 
            if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
            {
                Log.Error($"[OMW_Samhaphage] Failed to reserve target {job.targetA} for job {job.def}. Target may be reserved by another pawn.");
                return false;
            }

            // Optional: Only reserve Midsection if specifically needed for Bio-interactions
            ReservationLayerDef midsectionLayer = DefDatabase<ReservationLayerDef>.GetNamed("Midsection", false);
            if (midsectionLayer != null)
            {
                if (!pawn.Reserve(job.targetA, job, 1, -1, midsectionLayer, errorOnFailed))
                {
                    Log.Error($"[OMW_Samhaphage] Failed to reserve target {job.targetA} on Midsection layer for job {job.def}. Target may be reserved by another pawn.");
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Cast the job back to your custom type to get the action
            if (job is Job_ApproachAndInteract omwJob)
            {
                this.SpecificJob = omwJob;
            }
            else
            {
                Log.Error(
                    $"[OMW_Samhaphage] Job {job} is not of type Job_ApproachAndInteract. This should never happen.");
                yield break;
            }

            Log.Message($"[OMW_Samhaphage] Starting JobDriver_ApproachAndInteract for pawn {pawn} on target {TargetA.Thing}."); // Debug log

            this.FailOn(() => SpecificJob?.onInteract == null);
            this.FailOnDespawnedOrNull(TargetIndex.A);

            Log.Message(
                $"[OMW_Samhaphage] {pawn} Toils_Goto.GotoThing {TargetA.Thing}."); // Debug log


            // 1. The Approach
            // Use ClosestTouch to avoid 'Standing on Head' errors with beds
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Log.Message(
                $"[OMW_Samhaphage] {pawn} killing current targets job {TargetA.Thing}."); // Debug log


            // 2. The Interaction
            Toil interactToil = ToilMaker.MakeToil("InteractToil");
            interactToil.defaultDuration = 10;

            // 1.6 Specific: This prevents the 'Social' check that causes 
            // pawns to 'abort' jobs on sleeping allies/colonists.
            interactToil.initAction = delegate
            {
                Pawn targetPawn = TargetA.Thing as Pawn;
                if (targetPawn != null)
                {
                    if (!targetPawn.HostileTo(pawn))
                    {
                        Log.Message(
                            $"[OMW_Samhaphage] Target {targetPawn} is friendly to {pawn}. Ending job.");
                        targetPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
            };

            interactToil.tickAction = delegate { pawn.rotationTracker.FaceTarget(TargetA); };

            Log.Message(
                $"[OMW_Samhaphage] interactToil WithProgressBar"); // Debug log

            // Ensure the progress bar shows up even if they are 'under' blankets
            interactToil.WithProgressBarToilDelay(TargetIndex.A);

            yield return interactToil;

            Log.Message(
                $"[OMW_Samhaphage] {pawn} will do the interaction to {TargetA.Thing}"); // Debug log


            // 3. Final Execution
            yield return Toils_General.Do(() => { SpecificJob.onInteract?.Invoke(pawn, TargetA.Thing); });

            Log.Message(
                $"[OMW_Samhaphage] Jobs done!"); // Debug log
        }
    }
}