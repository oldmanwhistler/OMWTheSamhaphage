using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

// Google Gemini example

namespace OMW_Samhaphage
{
    public class Job_OMW_XenotypeAbility : Job
    {
        // Store the specific logic you want to execute later
        public System.Action<Pawn, Thing> onArrival;
    }

    public class JobDriver_ApproachAndInteract : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // TOIL 1: Walk to the target (Corpse or Pawn)
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil interactToil = ToilMaker.MakeToil();
            interactToil.initAction = () =>
            {
                // Cast the job back to your custom type to get the action
                if (job is Job_OMW_XenotypeAbility omwJob)
                {
                    omwJob.onArrival?.Invoke(pawn, TargetA.Thing);
                }
            };
            interactToil.defaultCompleteMode = ToilCompleteMode.Instant;

            yield return interactToil;
        }
    }
}