using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityCorpseOnly: NullThrumAbilityBase
    {
        public override bool ApplyPawn(Pawn pawn, Pawn caster = null)
        {
            return false;
        }

        public override bool CanApplyOnPawn(Pawn pawn, Pawn caster, out string reason)
        {
            reason = "No living pawns.";
            return false;
        }

       public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            return NewMenuItemIconDisabled(targetInfo);
        }

        public override MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new MenuItemIcon(this.VerbName, this.VerbDescription(corpse.InnerPawn, caster), this.Icon, () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.VerbName} {corpse.InnerPawn.LabelShort} because {reason}");
            }
        }                  
    }
}