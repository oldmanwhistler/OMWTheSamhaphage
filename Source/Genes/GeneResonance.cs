using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using AlphaGenes;

namespace OMW_Samhaphage
{
    public class Gene_ResourceResonance : Gene_Resource
    {
        // 1. Mandatory overrides from the base class you provided:
        public override float InitialResourceMax => 1.0f;
        protected override Color BarColor => new Color(0.36f, 0.22f, 0.42f); // Bruise-Purple
        protected override Color BarHighlightColor => new Color(0.54f, 0.17f, 0.89f); // Neon-Violet
        public override float MinLevelForAlert => 0.1f;
        public override string ResourceLabel => "resonance";

        public Gene_ResourceResonance() : base()
        {
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.Value = Rand.Range(0.03f, 0.20f);
        }        
    }
    public class GeneGizmo_ResourceResonance : GeneGizmo_Resource
    {
        Gene_ResourceResonance ResourceGene => gene as Gene_ResourceResonance;

        public GeneGizmo_ResourceResonance(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor,
            Color barhighlightColor)
            : base(gene, drainGenes, barColor, barhighlightColor)
        {
        }

        private static bool draggingBar;
        protected override bool DraggingBar
        {
            get { return draggingBar; }
            set { draggingBar = value; }
        }


        protected override string GetTooltip()
        {
            string text =
                $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValueForDisplay} / {gene.MaxForDisplay}\n";

            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text = text + "\n\n" + gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN")).Resolve();
            }

            return text;
        }
    }
}