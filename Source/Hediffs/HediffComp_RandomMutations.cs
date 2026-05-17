using System.Collections.Generic;
using System.Linq;
using Verse;

// Inspired by AlphaGenes' RandomMutation hediff comp (c) juanosarg. 
// See original at: https://github.com/juanosarg/AlphaGenes/blob/d6f14ee6106ce01351c86eb369703edde65bce66/1.6/Source/AlphaGenes/AlphaGenes/HediffComps/HediffComp_RandomMutation.cs

// The difference from Alpha Genes:
// - uses my own "random gene blacklist control", although if you look at OMW_BlacklistGenes mine respects the WretchBlacklistDef.
// - the genes are only removed if they remained xenogenes.
// - it can filter out genes not within the min/max metabolism range.

namespace OMW_Samhaphage
{
    public class HediffCompProperties_RandomMutation : HediffCompProperties
    {
        public int numberOfGenes = 1;
        public int period = 60000;
        public int minMetabolism = -100;
        public int maxMetabolism = 100;

        public HediffCompProperties_RandomMutation()
        {
            compClass = typeof(HediffComp_RandomMutation);
        }
    }
    public class HediffComp_RandomMutation : HediffComp
    {
        private HediffCompProperties_RandomMutation Props => (HediffCompProperties_RandomMutation)props;

        public List<GeneDef> geneDefs = new List<GeneDef>();

        public bool Active = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref this.geneDefs, nameof(this.geneDefs));
            Scribe_Values.Look(ref this.Active, nameof(this.Active));
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Active && this.parent.pawn.Map != null)
            {
                Active = true;
                this.geneDefs?.Clear();

                HashSet<GeneDef> alreadyHas = this.parent.pawn.genes.GenesListForReading
                    .Select(g => g.def)
                    .ToHashSet();
                for (int i = 0; i < Props.numberOfGenes; i++)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) =>
                        !OMW_BlacklistGenes.BlacklistedGenesMutation.Contains(x) &&
                        !alreadyHas.Contains(x) &&
                        x.biostatMet >= Props.minMetabolism &&
                        x.biostatMet <= Props.maxMetabolism).RandomElement();

                    if (gene != null)
                    {
                        this.geneDefs.Add(gene);
                        this.parent.pawn.genes?.AddGene(gene, true);
                    }
                }
            }

            if (this.parent.pawn.IsHashIntervalTick(Props.period))
            {

                if (!this.geneDefs.NullOrEmpty())
                {
                    // only remove genes if they didn't become endogenes, otherwise the player would lose the gene permanently and it would be more frustrating than fun. This also means that if a gene becomes an endogene, it will stay with the pawn permanently, which fits with the headcanon of these mutations being a way for the xenotypes to evolve and adapt to their environment over time.
                    for (int i = 0; i < this.geneDefs.Count; i++)
                    {
                        if (this.parent.pawn.genes?.HasXenogene(this.geneDefs[i]) == true)
                        {
                            Gene gene = this.parent.pawn.genes?.GetGene(this.geneDefs[i]);
                            if (gene != null)
                            {
                                this.parent.pawn.genes?.RemoveGene(gene);
                            }
                        }
                    }
                    this.geneDefs?.Clear();
                }
                Active = false;
            }

        }


        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            // only remove genes if they didn't become endogenes, otherwise the player would lose the gene permanently and it would be more frustrating than fun. This also means that if a gene becomes an endogene, it will stay with the pawn permanently, which fits with the headcanon of these mutations being a way for the xenotypes to evolve and adapt to their environment over time.
            for (int i = 0; i < this.geneDefs.Count; i++)
            {
                if (this.parent.pawn.genes?.HasXenogene(this.geneDefs[i]) == true)
                {
                    Gene gene = this.parent.pawn.genes?.GetGene(this.geneDefs[i]);
                    if (gene != null)
                    {
                        this.parent.pawn.genes?.RemoveGene(gene);
                    }
                }
            }
            Active = false;
            this.geneDefs?.Clear();
        }

    }
}