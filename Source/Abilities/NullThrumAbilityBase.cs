using System;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityBase
    {
        public NullThrumVerbBase verb;

        public abstract Texture2D Icon { get; }
        public abstract string VerbName { get; }
        public abstract string VerbDescription(Pawn victim, Pawn caster);

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

        public MenuItemIcon NewMenuItemIconDisabled(LocalTargetInfo targetInfo, string reason = null)
        {
            string msg = $"Can't apply {this.ToString()} on {targetInfo.Label}";
            if (reason != null)
            {
                msg += "\n" + reason;
            }
            return new MenuItemIcon(this.VerbName, msg, this.Icon);
        }        
        public MenuItemIcon NewMenuItemIcon(LocalTargetInfo targetInfo, Pawn caster = null)
        {
            if (targetInfo.Thing is Pawn pawn)
            {
                return NewMenuItemIconPawn(targetInfo, pawn, caster);
            }
            else if (targetInfo.Thing is Corpse corpse)
            {
                return NewMenuItemIconCorpse(targetInfo, corpse, caster);
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo);
            }
        }

        public abstract MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null);

        public abstract MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse,
            Pawn caster = null);
    }
}