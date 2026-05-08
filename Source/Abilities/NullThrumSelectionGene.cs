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

        protected abstract float ResonanceTotalMultiplier { get; }

        public int SelectionMaxCost => ResonanceUtility.Total(this.caster, this.ResonanceTotalMultiplier);

        // Abstract methods

        protected abstract List<Gene> GenesToSelectFrom(Pawn source, Pawn dest);
        protected abstract List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest);


        // Concrete methods

        protected float GeneValue(Gene gene)
        {
            float cpx = gene.def.biostatCpx;
            float arc = 3 * gene.def.biostatArc;
            float met = gene.def.biostatMet;
            met = met / 4 * -1;
            float tmp = cpx + met + arc;
            if (tmp < 0f) tmp = 0f;
            return tmp * this.ResonanceTotalMultiplier;
        }

        protected bool GeneIsWorthless(Gene gene)
        {
            return (gene.def.biostatCpx == 0) && (gene.def.biostatArc == 0) && (gene.def.biostatMet == 0);
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
            float value = this.GeneValue(plus.gene);
            ResonanceUtility.Incr($"Apply credit", caster, value);
        }
    }
}