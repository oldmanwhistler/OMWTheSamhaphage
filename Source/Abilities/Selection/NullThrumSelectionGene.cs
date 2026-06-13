using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public class NullThrumSelectionGeneBlocked
    {
        GeneDef geneDef;
        string reason;

        NullThrumSelectionGeneBlocked(GeneDef geneDef, string reason)
        {
            this.geneDef = geneDef;
            this.reason = reason;
        }
    }    
    public abstract class NullThrumSelectionGene : NullThrumSelectionBase
    {
        protected NullThrumSelectionGene(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            this.SetGenesToSelectFromPlus(source, dest);
        }

        public List<GenePlus> genes;

        public List<GenePlus> unselectableGenes;

        // Abstract methods

        protected abstract float ResonanceTotalMultiplier { get; }

        protected abstract List<NullThrumSelectionGeneBlocked> GenesBlockedFromSelection(Pawn source, Pawn dest);
        protected abstract List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, List<NullThrumSelectionGeneBlocked> blocked);
        protected abstract List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest);


        // Concrete methods

        public NullThrumResonanceType ResonanceType => NullThrumUtility.ResonanceType(this.AbilityType);

        protected float GeneValue(Gene gene)
        {
            if (gene == null)
            {
                Log.Error(
                    $"{Name}::GeneValue() called with a null gene");
                return 0f;
            }
            Log.Debug(
                $"{Name}::GeneValue({gene.Label}) has archite: {gene.def.biostatArc}, complexity: {gene.def.biostatCpx}, metabolism: {gene.def.biostatMet}");
            float value = ResonanceUtility.GeneResonanceValue(gene.def);
            float final = value * this.ResonanceTotalMultiplier;
            Log.Debug($"{Name}::GeneValue({gene.Label}) = {final}   ({value} * {this.ResonanceTotalMultiplier})");
            // everywhere else it is expected that GeneValue returns a positive numbers
            if (final < 0)
            {
                final = 0.1f;
            }
            return final;
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
            Log.Debug($"{Name}::SetGenesToSelectFromPlus({source.LabelShort}, {dest.LabelShort})");
            List<NullThrumSelectionGeneBlocked> blocked = this.GenesBlockedFromSelection(source, dest);
            List<Gene> genesToSelectFrom = this.GenesToSelectFrom(source, dest, blocked);
            List<GeneDef> conflictDefs = this.ConflictGeneDefs(source, dest) ?? new List<GeneDef>();
            Log.Debug($"{Name}:: genesToSelectFrom.Count = {genesToSelectFrom.Count}, conflictDefs.Count = {conflictDefs.Count}");
            this.genes = GenePlusUtility.ConvertToGenePlus(source, genesToSelectFrom, conflictDefs);
            foreach (GenePlus gene in this.genes)
            {
                gene.value = this.GeneValue(gene.gene);
            }
            // Build a list of all the genes that could not be selected
            List<Gene> genesThatCantBeSelected = new List<Gene>();
            foreach (Gene gene in source.genes.GenesListForReading)
            {
                if (!genesToSelectFrom.Contains(gene))
                {
                    genesThatCantBeSelected.Add(gene);
                }
            }
            List<GeneDef> conflictDefs2 = new List<GeneDef>();
            this.unselectableGenes = GenePlusUtility.ConvertToGenePlus(source, genesThatCantBeSelected, conflictDefs2);
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