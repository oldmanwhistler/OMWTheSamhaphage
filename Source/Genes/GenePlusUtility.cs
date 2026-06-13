using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
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