using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using AlphaGenes;

namespace OMW_Samhaphage
{
    public class Gene_ResourceResonance : Gene_Resource
    {
        private const int PassiveGainIntervalTicks = 1000; // Avoids CS0108 name conflict

        public override float InitialResourceMax => 200f;
        protected override Color BarColor => new Color(0.36f, 0.22f, 0.42f); // Bruise-Purple
        protected override Color BarHighlightColor => new Color(0.54f, 0.17f, 0.89f); // Neon-Violet
        public override float MinLevelForAlert => 1f;
        public override string ResourceLabel => "resonance";


        public override int PostProcessValue(float value)
        {
            return Mathf.RoundToInt(value);
        }

        public Gene_ResourceResonance() : base()
        {
            this.max = InitialResourceMax;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            // initialize with a random amount
            if (Value <= 0)
            {
                Value = Rand.Range(3, 20);
            }
        }

        public override void Tick()
        {
            base.Tick();

            // Use the hash interval to prevent performance hits every frame
            if (pawn.IsHashIntervalTick(PassiveGainIntervalTicks))
            {
                ApplyPassiveGain();
            }
        }

        private void ApplyPassiveGain()
        {
            if (pawn.genes == null) return;

            // Dynamically fetch the gain from the Pawn's stats
            // This looks for OMW_StatResonance defined in your StatDefs.xml
            float dailyGain = pawn.GetStatValue(StatDef.Named("OMW_StatResonance"));

            if (dailyGain != 0)
            {
                // Calculate gain: (Daily Amount / 60000 ticks in a day) * Ticks Passed
                float gainPerInterval = (dailyGain / 60000f) * (float)PassiveGainIntervalTicks;
                OffsetResonance(gainPerInterval);
            }
        }

        public void OffsetResonance(float offset)
        {
            // Value and MaxForDisplay are built-in fields from Gene_Resource
            Value = Mathf.Clamp(Value + offset, 0f, MaxForDisplay);
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

        protected override bool DraggingBar
        {
            get { return false; }
            set {  }
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