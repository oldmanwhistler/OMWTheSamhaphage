using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public abstract class NullThrumSelectionGene : NullThrumSelectionBase
    {
        protected NullThrumSelectionGene(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            this.SetGenesToSelectFromPlus(source, dest);
        }

        public List<GenePlus> genes;

        // Abstract methods

        protected abstract float ResonanceTotalMultiplier { get; }

        protected abstract List<Gene> GenesToSelectFrom(Pawn source, Pawn dest);
        protected abstract List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest);


        // Concrete methods

        public NullThrumResonanceType ResonanceType => NullThrumUtility.ResonanceType(this.AbilityType);

        protected float GeneValue(Gene gene)
        {
            return ResonanceUtility.GeneResonanceValue(gene.def) * this.ResonanceTotalMultiplier;
        }

        protected bool GeneIsWorthless(Gene gene)
        {

            if (gene.def.displayCategory.defName.Contains("Cosmetic"))
            {
                return true;
            }
            return false;
        }

        // GenesPlus is a wrapper class that has useful info for the UI, such as resonance value and conflict info
        private void SetGenesToSelectFromPlus(Pawn source, Pawn dest)
        {
            List<Gene> genesToSelectFrom = this.GenesToSelectFrom(source, dest);
            List<GeneDef> conflictDefs = this.ConflictGeneDefs(source, dest) ?? new List<GeneDef>();
            this.genes = GenePlusUtility.ConvertToGenePlus(source, genesToSelectFrom, conflictDefs);
            foreach (GenePlus gene in this.genes)
            {
                gene.value = this.GeneValue(gene.gene);
            }
        }

        public bool ResonanceDebit(GenePlus plus)
        {
            if (this.ResonanceType == NullThrumResonanceType.ResonanceTypeCredit)
                Log.Error($"NullThrumSelectionGene:{this.Name} is configured to be a Credit but it is calling ResonanceDebit()");

            float value = this.GeneValue(plus.gene);
            if (ResonanceUtility.HasAvailable(caster, value))
            {
                ResonanceUtility.Decr(caster, value);
                return true;
            }
            else
            {
                Messages.Message($"Not enough Resonance to {Name} {plus.gene.LabelCap}.", MessageTypeDefOf.RejectInput);
                return false;
            }
        }

        public void ResonanceCredit(GenePlus plus)
        {
            if (this.ResonanceType == NullThrumResonanceType.ResonanceTypeDebit)
                Log.Error($"NullThrumSelectionGene:{this.Name} is configured to be a Debit but it is calling ResonanceCredit()");

            float value = this.GeneValue(plus.gene);
            ResonanceUtility.Incr($"Apply credit", caster, value);
        }
    }
}