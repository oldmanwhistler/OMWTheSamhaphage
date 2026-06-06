using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OMW_Samhaphage
{
    public class Job_ApproachAndInteract : Job
    {
        static Logger Log = new Logger("Job");
        public static Job_ApproachAndInteract CreateAndAssign(LocalTargetInfo target, Pawn caster, Action<Pawn, Thing> onInteract)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.ignoreForbidden = true;
            job.count = 1;
            job.targetA = target;
            job.onInteract = onInteract;
            caster.jobs.TryTakeOrderedJob(job);            
            return job;
        }
        public Action<Pawn, Thing> onInteract;        
    }

    public class JobDriver_ApproachAndInteract : JobDriver
    {
        static Logger Log = new Logger("Job");
        // Use a property to avoid repeated casting
        private Job_ApproachAndInteract _specificJob;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Reserve the target globally. 
            // 1 is the max pawns, -1 is 'all stacks'. 
            if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
            {
                Log.Error($"Failed to reserve target {job.targetA} for job {job.def}. Target may be reserved by another pawn.");
                return false;
            }

            // Optional: Only reserve Midsection if specifically needed for Bio-interactions
            ReservationLayerDef midsectionLayer = DefDatabase<ReservationLayerDef>.GetNamed("Midsection", false);
            if (midsectionLayer != null)
            {
                if (!pawn.Reserve(job.targetA, job, 1, -1, midsectionLayer, errorOnFailed))
                {
                    Log.Error($"Failed to reserve target {job.targetA} on Midsection layer for job {job.def}. Target may be reserved by another pawn.");
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Safety check for save-loading
            this.FailOn(() => job == null);

            // Initialize the job if necessary
            yield return new Toil
            {
                initAction = () =>
                {
                    if (job == null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Errored);
                        return;
                    }

                    // Give the target a job to stand still so we don't have to chase them.
                    // We use JobDefOf.Wait with a generous duration (1000 ticks is ~16 seconds).
                    if (TargetA.Thing is Pawn targetPawn && targetPawn != pawn && targetPawn.Spawned && !targetPawn.Dead)
                    {
                        Job waitJob = JobMaker.MakeJob(JobDefOf.Wait, 1000);
                        targetPawn.jobs.TryTakeOrderedJob(waitJob, JobTag.Misc);
                        Log.Debug($"{targetPawn.LabelShort} assigned Wait job to facilitate approach by {pawn.LabelShort}.");
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            }; 
            
            // Cast the job back to your custom type to get the action
            if (job is Job_ApproachAndInteract omwJob)
            {
                this._specificJob = omwJob;
            }

            Log.Debug($"Starting JobDriver_ApproachAndInteract for pawn {pawn} on target {TargetA.Thing}."); // Debug log

            this.FailOn(() => this._specificJob?.onInteract == null);
            this.FailOnDespawnedOrNull(TargetIndex.A);

            Log.Debug(
                $"{pawn} Toils_Goto.GotoThing {TargetA.Thing}."); // Debug log

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Log.Debug(
                $"{pawn} killing current targets job {TargetA.Thing}."); // Debug log


            // 2. The Interaction
            Toil interactToil = ToilMaker.MakeToil("InteractToil");
            interactToil.defaultDuration = 10;

            // 1.6 Specific: This prevents the 'Social' check that causes 
            // pawns to 'abort' jobs on sleeping allies/colonists.
            interactToil.initAction = delegate
            {
                if (TargetA.Thing is Pawn targetPawn && targetPawn != pawn && targetPawn.Spawned && !targetPawn.Dead)
                {
                    Log.Debug($"Target {targetPawn.LabelShort} interrupted to allow interaction.");
                    // Now that we've arrived, give them a fresh wait job to ensure they stay still 
                    // during the actual interaction progress bar.
                    Job waitJob = JobMaker.MakeJob(JobDefOf.Wait, 500);
                    targetPawn.jobs.TryTakeOrderedJob(waitJob, JobTag.Misc);
                }
            };

            interactToil.tickAction = delegate { pawn.rotationTracker.FaceTarget(TargetA); };

            Log.Debug(
                $"interactToil WithProgressBar"); // Debug log

            // Ensure the progress bar shows up even if they are 'under' blankets
            interactToil.WithProgressBarToilDelay(TargetIndex.A);

            yield return interactToil;

            Log.Debug(
                $"{pawn} will do the interaction to {TargetA.Thing}"); // Debug log


            // 3. Final Execution
            yield return Toils_General.Do(() => { this._specificJob?.onInteract?.Invoke(pawn, TargetA.Thing); });

            Log.Debug(
                $"Jobs done!"); // Debug log
        }
    }
}