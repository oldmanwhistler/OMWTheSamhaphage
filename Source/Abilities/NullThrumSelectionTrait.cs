using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public abstract class NullThrumSelectionTrait : NullThrumSelectionBase
    {
        protected NullThrumSelectionTrait(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            this.SetTraitsToSelectFromPlus(source, dest);
        }

        public List<TraitPlus> traits;

        protected abstract float ResonanceTotalMultiplier { get; }

        protected abstract NullThrumResonanceType ResonanceType { get; }

        // Abstract methods

        protected abstract List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest);
        protected abstract List<TraitDef> ConflictTraitDefs(Pawn source, Pawn dest);


        // Concrete methods

        protected virtual float TraitValue(Trait trait)
        {
            // Traits don't have biostats. Defaulting to a flat value of 1.0 per trait 
            // adjusted by the specific ability multiplier.
            return 1.0f * this.ResonanceTotalMultiplier;
        }

        protected virtual bool TraitIsWorthless(Trait trait)
        {
            // Placeholder for trait-specific logic (e.g. ignoring 'neutral' traits)
            return false;
        }

        // TraitPlus is a wrapper class that has useful info for the UI
        private void SetTraitsToSelectFromPlus(Pawn source, Pawn dest)
        {
            List<Trait> traitsToSelectFrom = this.TraitsToSelectFrom(source, dest);
            List<TraitDef> conflictDefs = this.ConflictTraitDefs(source, dest) ?? new List<TraitDef>();
            this.traits = TraitPlusUtility.ConvertToTraitPlus(source, traitsToSelectFrom, conflictDefs);
            foreach (TraitPlus plus in this.traits)
            {
                plus.value = this.TraitValue(plus.trait);
            }
        }

        public bool ResonanceDebit(TraitPlus plus)
        {
            if (this.ResonanceType == NullThrumResonanceType.ResonanceTypeCredit)
                Log.Error($"NullThrumSelectionTrait:{this.Name} is configured to be a Credit but it is calling ResonanceDebit()");

            float value = this.TraitValue(plus.trait);
            if (ResonanceUtility.HasAvailable(caster, value))
            {
                ResonanceUtility.Decr(caster, value);
                return true;
            }
            else
            {
                Messages.Message($"Not enough Resonance to {Name} {plus.trait.LabelCap}.", MessageTypeDefOf.RejectInput);
                return false;
            }
        }

        public void ResonanceCredit(TraitPlus plus)
        {
            if (this.ResonanceType == NullThrumResonanceType.ResonanceTypeDebit)
                Log.Error($"NullThrumSelectionTrait:{this.Name} is configured to be a Debit but it is calling ResonanceCredit()");

            float value = this.TraitValue(plus.trait);
            ResonanceUtility.Incr($"Apply credit", caster, value);
        }
    }
}