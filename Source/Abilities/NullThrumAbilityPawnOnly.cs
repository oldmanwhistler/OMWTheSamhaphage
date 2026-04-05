using Verse;

namespace OMW_Samhaphage
{
    public abstract class NullThrumAbilityPawnOnly: NullThrumAbilityBase
    {
        public override bool ApplyCorpse(Corpse corpse, Pawn caster = null)
        {
            return false;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, out string reason)
        {
            reason = "No corpses.";
            return false;
        }

        public override FloatMenuOption NewFloatMenuOptionCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            return NewFloatMenuOptionDisabled(targetInfo);
        }
    }
}