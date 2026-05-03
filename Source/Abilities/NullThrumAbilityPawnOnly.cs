using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityPawnOnly: NullThrumAbilityBase
    {
        public override bool ApplyCorpse(Corpse corpse, Pawn caster = null)
        {
            return false;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "No corpses.";
            return false;
        }

       public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new MenuItemIcon(this.VerbName, $"{this.VerbName} {pawn.LabelShort} {this.VerbDescription}", this.Icon, () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.VerbName} {pawn.LabelShort} because {reason}");
            }
        }

        public override MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            return NewMenuItemIconDisabled(targetInfo);
        }        
    }
}