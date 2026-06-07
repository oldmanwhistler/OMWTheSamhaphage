using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityBase
    {
        public abstract Texture2D Icon { get; }

        public abstract NullThrumAbilityProps AbilityProp { get; }        
        public abstract NullThrumAbilityType AbilityType { get; }

        public string AbilityName => NullThrumUtility.ToString(this.AbilityType);
        public abstract string AbilityDescription(Pawn victim, Pawn caster);

        public Action<bool> onComplete;

        /// <summary>
        /// Indicates if this specific ability is lethal to its target.
        /// </summary>
        public virtual bool IsLethal => false;

        protected static Logger Log = new Logger("Abilities");

        protected void doOnComplete(bool value)
        {            
            this.onComplete?.Invoke(value);
            this.onComplete = null;
        }

        protected System.Action onCompleteAction(bool value)
        {
            return () => doOnComplete(value);
        }

        public void ShowLethalConfirmation(Pawn pawn, System.Action sacrificeAction)
        {
            string msg = $"Warning: Activating this ability will kill {pawn.LabelShort}. Are you sure?";
            Dialog_MessageBox window = new Dialog_MessageBox(
                text: msg,
                buttonAText: "Confirm".Translate(),
                buttonAAction: sacrificeAction,
                buttonBText: "Cancel".Translate(),
                buttonBAction: onCompleteAction(false),
                buttonADestructive: true,
                title: "Lethal Ability".Translate()
            );

            Find.WindowStack.Add(window);
        }

        public void ShowCorpseConfirmation(Pawn pawn, System.Action sacrificeAction)
        {
            string msg = $"Warning: Activating this ability will destroy {pawn.LabelShort}. Are you sure?";
            Dialog_MessageBox window = new Dialog_MessageBox(
                text: msg,
                buttonAText: "Confirm".Translate(),
                buttonAAction: sacrificeAction,
                buttonBText: "Cancel".Translate(),
                buttonBAction: onCompleteAction(false),
                buttonADestructive: true,
                title: "Lethal Ability".Translate()
            );

            Find.WindowStack.Add(window);
        }        

        public void ApplyThing(Thing thing, Pawn caster)
        {
            if (thing is Pawn pawn)
            {
                ApplyPawn(pawn, caster);                
            }
            else if (thing is Corpse corpse)
            {
                ApplyCorpse(corpse, caster);                
            }
        }

        public abstract void ApplyPawn(Pawn victim, Pawn caster);

        public abstract void ApplyCorpse(Corpse corpse, Pawn caster);

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

        protected void Job(LocalTargetInfo targetInfo, Pawn caster)
        {
            Job_ApproachAndInteract job = new Job_ApproachAndInteract();
            job.def = OMW_JobDefOf.OMW_ApproachAndInteract;
            job.targetA = targetInfo;
            job.ignoreForbidden = true;
            
            // The delegate needs to match the signature: (Pawn actor, Thing t)
            job.onInteract = (actor, t) => { ApplyThing(t, actor); };
            
            // Return true if the job was successfully started/queued.
            // The actual result will be sent via onAbilityComplete later.
            if (!caster.jobs.TryTakeOrderedJob(job))           
            {
                Log.Error($"{caster.LabelShort} was not able to take job {this.AbilityName}");    
            }            
        }

        public MenuItemIcon NewMenuItemIconDisabled(LocalTargetInfo targetInfo, string reason = null)
        {
            string msg = $"Can't apply {this.ToString()} on {targetInfo.Label}";
            if (reason != null)
            {
                msg += "\n" + reason;
            }

            return new MenuItemIcon(this, msg);
        }

        public MenuItemIcon NewMenuItemIcon(LocalTargetInfo targetInfo, Pawn caster)
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

        public abstract MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster);

        public abstract MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse,
            Pawn caster);
            
    }
}