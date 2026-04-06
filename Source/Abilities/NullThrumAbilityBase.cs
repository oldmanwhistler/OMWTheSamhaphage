using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityBase
    {
        public bool ApplyThing(Thing thing, Pawn caster = null)
        {
            if (thing is Pawn pawn)
            {
                return ApplyPawn(pawn, caster);
            }
            else if (thing is Corpse corpse)
            {
                return ApplyCorpse(corpse, caster);
            }
            else
            {
                return false;
            }
        }

        public abstract bool ApplyPawn(Pawn pawn, Pawn caster = null);

        public abstract bool ApplyCorpse(Corpse corpse, Pawn caster = null);

        public bool CanApplyOnThing(Thing thing, Pawn caster, out string reason)
        {
            if (thing is Pawn pawn)
            {
                return CanApplyOnPawn(pawn, caster, out reason);
            }
            else if (thing is Corpse corpse)
            {
                return CanApplyOnCorpse(corpse, caster, out reason);
            }
            else
            {
                reason = "Only applies on the living or the dead.";
                return false;
            }
        }

        public abstract bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason);

        public abstract bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason);

        public FloatMenuOption NewFloatMenuOptionDisabled(LocalTargetInfo targetInfo)
        {
            return new FloatMenuOption($"Can't apply {this.ToString()} on {targetInfo.Label}", null) { Disabled = true };
        }
        public FloatMenuOption NewFloatMenuOption(LocalTargetInfo targetInfo, Pawn caster = null)
        {
            if (targetInfo.Thing is Pawn pawn)
            {
                return NewFloatMenuOptionPawn(targetInfo, pawn, caster);
            }
            else if (targetInfo.Thing is Corpse corpse)
            {
                return NewFloatMenuOptionCorpse(targetInfo, corpse, caster);
            }
            else
            {
                return NewFloatMenuOptionDisabled(targetInfo);
            }            
        }

        public abstract FloatMenuOption NewFloatMenuOptionPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null);

        public abstract FloatMenuOption NewFloatMenuOptionCorpse(LocalTargetInfo targetInfo, Corpse corpse,
            Pawn caster = null);

        public void Job(LocalTargetInfo targetInfo, Pawn caster)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = targetInfo;
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            // We use the 't' passed from the JobDriver to ensure target validity
            job.onInteract = (actor, t) => ApplyThing(t, actor);
            caster.jobs.TryTakeOrderedJob(job);
        }
    }
}