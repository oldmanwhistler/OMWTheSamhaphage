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

        public List<TraitPlus> unselectableTraits;

        protected abstract float ResonanceTotalMultiplier { get; }

        // Abstract methods

        protected abstract List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest);
        protected abstract List<TraitDef> ConflictTraitDefs(Pawn source, Pawn dest);


        // Concrete methods
        public NullThrumResonanceType ResonanceType => NullThrumUtility.ResonanceType(this.AbilityType);
        
        protected float TraitValue(Trait trait)
        {
            if (trait == null)
            {
                Log.Error(
                    $"{Name}::TraitValue() called with a null TRait");
                return 0f;
            }

            float value = ResonanceUtility.TraitResonanceValue(trait);
            float final = value * this.ResonanceTotalMultiplier;
            Log.Debug($"{Name}::TraitValue({trait.Label}) = {final}   ({value} * {this.ResonanceTotalMultiplier})");
            // everywhere else it is expected that TraitValue returns a positive numbers
            if (final < 0)
            {
                final = 0.1f;
            }

            return final;
        }        

        // TraitPlus is a wrapper class that has useful info for the UI
        private void SetTraitsToSelectFromPlus(Pawn source, Pawn dest)
        {
            List<Trait> traitsToSelectFrom = this.TraitsToSelectFrom(source, dest);
            List<TraitDef> conflictDefs = this.ConflictTraitDefs(source, dest) ?? new List<TraitDef>();
            this.traits = TraitPlusUtility.ConvertToTraitPlus(dest, traitsToSelectFrom, conflictDefs);
            foreach (TraitPlus plus in this.traits)
            {
                plus.value = this.TraitValue(plus.trait);
            }

            // Build a list of all the traits that could not be selected
            List<Trait> traitsThatCantBeSelected = new List<Trait>();
            foreach (Trait trait in source.story.traits.allTraits)
            {
                if (!traitsToSelectFrom.Contains(trait))
                {
                    traitsThatCantBeSelected.Add(trait);
                }
            }

            List<TraitDef> conflictDefs2 = new List<TraitDef>();
            this.unselectableTraits = TraitPlusUtility.ConvertToTraitPlus(source, traitsThatCantBeSelected, conflictDefs2);
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