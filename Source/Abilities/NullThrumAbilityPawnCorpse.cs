using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityPawnCorpse: NullThrumAbilityBase
    {
       public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new MenuItemIcon(this.AbilityName, this.AbilityDescription(pawn, caster), this.Icon, () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.AbilityName} {pawn.LabelShort} because {reason}");
            }
        }

        public override MenuItemIcon NewMenuItemIconCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new MenuItemIcon(this.AbilityName, this.AbilityDescription(corpse.InnerPawn, caster), this.Icon, () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(targetInfo, $"Can't {this.AbilityName} {corpse.InnerPawn.LabelShort} because {reason}");
            }
        }  
    }
}