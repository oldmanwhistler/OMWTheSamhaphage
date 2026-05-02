using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityPawnCorpse: NullThrumAbilityBase
    {
       public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new MenuItemIcon(() => Job(targetInfo, caster), $"{this.VerbName} {pawn.LabelShort} {this.VerbDescription}", this.Icon);
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.VerbName} {pawn.LabelShort} because {reason}");
            }
        }

        public override MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new MenuItemIcon(() => Job(targetInfo, caster), $"{this.VerbName} {corpse.InnerPawn.LabelShort} {this.VerbDescription}", this.Icon);
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.VerbName} {corpse.InnerPawn.LabelShort} because {reason}");
            }
        }  
    }
}