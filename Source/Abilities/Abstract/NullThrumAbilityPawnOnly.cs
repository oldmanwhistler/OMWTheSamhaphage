using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityPawnOnly: NullThrumAbilityBase
    {
        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            return;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "No corpses.";
            return false;
        }

       public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new MenuItemIcon(this, this.AbilityDescription(pawn, caster), () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.AbilityName} {pawn.LabelShort} because {reason}");
            }
        }

        public override MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster)
        {
            return NewMenuItemIconDisabled(targetInfo);
        }        
    }
}