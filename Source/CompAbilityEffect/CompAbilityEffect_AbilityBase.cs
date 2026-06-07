using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public abstract class CompAbilityEffect_AbilityBase : CompAbilityEffect
    {
        protected static Logger Log = new Logger("CompAbilityEffect");

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
                    Messages.Message("Cannot target hostile creatures with this ability.", targetPawn, MessageTypeDefOf.RejectInput, false);
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
        
        public abstract bool OpenMenu(LocalTargetInfo target, LocalTargetInfo dest);

        // Does nothing, but makes moves the caster to the target before opening the menu.
        public void Job(LocalTargetInfo target, LocalTargetInfo dest, Pawn caster)
        {
            Log.Debug($"{parent.pawn}.Job");
            if (target.Thing is Pawn targetPawn && targetPawn.HostileTo(caster))
            {
                Messages.Message("Cannot target hostile creatures with this ability.", targetPawn, MessageTypeDefOf.RejectInput, false);
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

        protected bool DoOpenMenuAgain(LocalTargetInfo target, LocalTargetInfo dest, bool success)
        {
            if (target == null || dest == null) return false;
            Log.Debug(
                $"DoOpenMenu is done invoking ability.onComplete and returned {success}");
            if (success) OpenMenu(target, dest);
            return success;
        }
        protected bool DoOpenMenu(LocalTargetInfo target, LocalTargetInfo dest, List<MenuItemBase> items)
        {
            BetterFloatMenu.Open(items, parent.pawn, (item) => // OnSelected now expects Func<MenuItemBase, bool>
            {
                if ((item is MenuItemIcon menuItem) && (item.Payload is System.Action action))
                {
                    Log.Debug($"DoOpenMenu is invoking {item.Payload?.ToString() ?? "null"}");
                    menuItem.Ability.onComplete = (success) => DoOpenMenuAgain(target, dest, success);
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