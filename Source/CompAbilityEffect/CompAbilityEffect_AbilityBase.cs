using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class CompAbilityEffect_AbilityBase : CompAbilityEffect
    {
        protected static Logger Log = new Logger("CompAbilityEffect");

        private bool canApplyAgain = false;

        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.Drafted)
            {
                reason = "Biological restructuring cannot be performed while drafted.";
                return true;
            }
            return base.GizmoDisabled(out reason);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Debug($"{parent.pawn}.Apply called with target {target} and dest {dest}");
            base.Apply(target, dest);
            if (target.Thing is Pawn targetPawn)
            {
                // Prevent targeting hostiles if the ability is meant for the hive/allies
                if (targetPawn.HostileTo(parent.pawn))
                {
                    Messages.Message("Cannot target hostile creatures with this ability.", targetPawn,
                        MessageTypeDefOf.RejectInput, false);
                    return;
                }

                if (targetPawn == parent.pawn)
                {
                    OpenMenu(target, dest);
                    return; // No need to move to target if target is self
                }
            }

            Log.Debug($"{parent.pawn}.Apply is calling Job to move to target");
            this.Job(target, dest, this.parent.pawn);
        }

        // Does nothing, but makes moves the caster to the target before opening the menu.
        public void Job(LocalTargetInfo target, LocalTargetInfo dest, Pawn caster)
        {
            Log.Debug($"{parent.pawn}.Job");
            if (target.Thing is Pawn targetPawn && targetPawn.HostileTo(caster))
            {
                Messages.Message("Cannot target hostile creatures with this ability.", targetPawn,
                    MessageTypeDefOf.RejectInput, false);
                return;
            }

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
                Log.Debug($"Job created successfully for {caster}. Job: {job}");
            }
        }        
        public abstract bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest);

        public void ApplyAgain(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!canApplyAgain)
            {
                Log.Debug("ApplyAgain can't do anything because canApplyAgain is false.");
                return;
            }
            canApplyAgain = false;

            // LocalTargetInfo is a struct; check IsValid instead of null.
            if (!target.IsValid)
            {
                Log.Debug($"ApplyAgain: Target is invalid, not re-opening.");
                return;
            }
            // Ensure the target still exists before re-opening (e.g., corpse wasn't consumed/destroyed)
            if (target.HasThing && (target.Thing == null || target.Thing.Destroyed))
            {
                Log.Debug("ApplyAgain: Target was destroyed, not re-opening menu.");
                return;
            }

            // Dest is optional. Only block the re-open if dest WAS valid but the thing it pointed to 
            // is now gone (e.g. the secondary target was consumed/destroyed).
            if (dest.IsValid && dest.HasThing && (dest.Thing == null || dest.Thing.Destroyed))
            {
                Log.Debug("ApplyAgain: Dest was destroyed, not re-opening menu.");
                return;
            }
            Log.Debug("ApplyAgain is running Apply again after the window closed.");
            Apply(target, dest);
        }

        protected bool DoOpenMenu(LocalTargetInfo target, LocalTargetInfo dest, List<MenuItemBase> items)
        {
            BetterFloatMenu.Open(items, parent.pawn, (item) => 
            {
                if ((item is MenuItemIcon menuItem) && (item.Payload is System.Action action))
                {
                    Log.Debug($"DoOpenMenu is invoking {item.Payload?.ToString() ?? "null"}");
                    this.canApplyAgain = true;
                    menuItem.Ability.SetOnComplete(() => ApplyAgain(target, dest));
                    action.Invoke();
                }
                else
                {
                    Log.Error(
                        $"DoOpenMenu does not know how to handle item.Payload={item.Payload?.ToString() ?? "null"}");
                }
                return false;
            });
            return true;
        }
    }
}