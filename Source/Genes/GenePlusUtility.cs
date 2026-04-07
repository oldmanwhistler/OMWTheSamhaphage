using System.Collections.Generic;
using Verse;

namespace OMW_Samhaphage
{
    public class GenePlus
    {
        public Gene gene;
        public float cost = 0f;
        public string destinationConflictStr = "";
        public bool isXenogene;
        

        public GenePlus(Gene gene)
        {
            this.gene = gene;
        }

        public bool HasConflict()
        {
            return !this.destinationConflictStr.NullOrEmpty();
        }

        public override string ToString()
        {
            var genetype = "Endogene";
            if (this.isXenogene)            {
                genetype = "Xenogene";
            }
            var stats = $"\n\n{genetype}\nMetabolism: {this.gene.def.biostatMet:+#;-#;0}" +
                           $"\nComplexity: {this.gene.def.biostatCpx}";

            if (this.gene.def.biostatArc > 0)
            {
                stats += $"\nArchite: {this.gene.def.biostatArc}";
            }
            stats += $"\nResonance Cost: {this.cost}";            
            var tip = $"{this.gene.LabelCap}\n\n{this.gene.def.DescriptionFull}{stats}";
            if (this.gene.overriddenByGene != null)
            {
                tip += $"\n\n<color=#999999>(This gene is overridden by {this.gene.overriddenByGene.LabelCap})</color>";
            }

            if (destinationConflictStr != "")
            {
                // Adds a red warning with the specific gene name
                tip += $"\n\n<color=#ff6666>(This gene conflicts with {destinationConflictStr})</color>";
            }
            return tip;
        }
    }

    public static class GenePlusUtility
    {

        public static List<GenePlus> ConvertToGenePlus(Pawn pawn, List<Gene> genes, List<GeneDef> destConflicts = null)
        {
            List<GenePlus> endoGenes = new List<GenePlus>();
            List<GenePlus> xenoGenes = new List<GenePlus>();

            foreach (Gene gene in genes)
            {
                GenePlus plus = new GenePlus(gene);
                plus.isXenogene = pawn.genes.Xenogenes.Contains(gene);

                List<string> tmpDestConflicts = [];
                if (destConflicts != null)
                {
                    foreach (GeneDef possible in destConflicts)
                    {
                        if (gene.def == possible || gene.def.ConflictsWith(possible))
                        {
                            tmpDestConflicts.Add(possible.LabelCap);
                        }
                    }
                }
                plus.destinationConflictStr = string.Join(", ", tmpDestConflicts);

                if (plus.isXenogene) xenoGenes.Add(plus);
                else endoGenes.Add(plus);
            }

            xenoGenes.AddRange(endoGenes);
            return xenoGenes;
        }
    }
}