using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
       public class GenePlus
    {
        public Gene gene;
        public float value = 0f;
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
            stats += $"\nResonance Value: {this.value}";            
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

            if (OMW_BlacklistGenes.BlacklistedGenes.Any(x => x.geneDef == this.gene.def))
            {
                // get the reason why it's blacklisted
                BlacklistGene bl = OMW_BlacklistGenes.BlacklistedGenes.FirstOrDefault(x => x.geneDef == this.gene.def);
                if (bl != null)
                {
                    tip += $"\n\n<color=#ffcc00>(Blacklisted: {bl.blacklistReason})</color>";
                }
            }

            return tip;
        }
    }
}