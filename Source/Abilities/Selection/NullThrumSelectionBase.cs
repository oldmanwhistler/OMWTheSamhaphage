using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public abstract class NullThrumSelectionBase
    {
        protected static Logger Log = new Logger("Selection");
        protected Pawn caster;
       
        protected NullThrumSelectionBase(Pawn caster, Pawn source, Pawn dest)
        {
            // Need to store this for calculating resonance value and max selection
            this.caster = caster;
        }

        // Abstract methods
        public abstract NullThrumAbilityType AbilityType { get; }
        public abstract NullThrumAbilityProps AbilityProp { get; }

        public string Name => NullThrumUtility.ToString(this.AbilityType);

        // Concrete methods

        public float SelectionMaxCost()
        {
            if (this.caster == null) return 0f;
            if (this.AbilityProp.resonanceType == NullThrumResonanceType.ResonanceTypeDebit)
                return ResonanceUtility.Total(this.caster);
            if (this.AbilityProp.resonanceType == NullThrumResonanceType.ResonanceTypeCredit)
                return OMW_Mod.settings.resonanceMax - ResonanceUtility.Total(this.caster);
            return 0f;
        }
    }
}