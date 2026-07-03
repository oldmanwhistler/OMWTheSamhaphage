using System.Collections.Generic;
using System.Linq;
using Verse;

// Based onAlphaGenes' RandomMutation hediff comp (c)2021 juanosarg. License CC-BY-NC-ND.
// See original at: https://github.com/juanosarg/AlphaGenes/blob/d6f14ee6106ce01351c86eb369703edde65bce66/1.6/Source/AlphaGenes/AlphaGenes/HediffComps/HediffComp_RandomMutation.cs

// The difference from Alpha Genes:
// - uses my own "random gene blacklist control", although if you look at OMW_BlacklistGenes mine respects the WretchBlacklistDef.
// - the genes are only removed if they remained xenogenes.
// - it can filter out genes not within the min/max metabolism range.
//
// With thanks to juanosarg for the original code and inspiration. With the original code, the genes would be removed even if they were copied to another Pawn (although duplicating the gene from the gene.def would solve that). I want these random genes to be something that could be harvested or used to build stronger pawns, so I only remove the genes if they are still xenogenes.

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
        static Logger Log = new Logger("Mutation");
        private HediffCompProperties_RandomMutation Props => (HediffCompProperties_RandomMutation)props;

        public List<GeneDef> geneDefs = new List<GeneDef>();

        public bool Active = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref this.geneDefs, nameof(this.geneDefs), LookMode.Def);
            Scribe_Values.Look(ref this.Active, nameof(this.Active));
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // do nothing
            if (this.parent?.pawn.Map == null) return;

            if (!Active) 
            {
                Active = true;
                this.geneDefs?.Clear();

                HashSet<GeneDef> alreadyHas = this.parent.pawn.genes?.GenesListForReading
                    .Where(g => !g.Overridden)
                    .Select(g => g.def)
                    .ToHashSet() ?? new HashSet<GeneDef>();

                // BlacklistedGenesDontMutate will contain all the genes that force/suppress traits

                for (int ii = 0; ii < Props.numberOfGenes; ii++)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) =>
                        !OMW_BlacklistGenes.BlacklistedGenesDontMutate.Contains(x) &&
                        !alreadyHas.Contains(x) &&
                        x.biostatMet >= Props.minMetabolism &&
                        x.biostatMet <= Props.maxMetabolism).RandomElement();

                    if (gene != null)
                    {
                        Log.Debug($"CompPostTick: {this.parent.pawn.Name} add mutated xenogene # {ii} {gene.defName}");
                        this.geneDefs?.Add(gene);
                        this.parent.pawn.genes?.AddGene(gene, true);
                    }
                }
            }

            if (this.parent.pawn.IsHashIntervalTick(Props.period))
            {
                if (!this.geneDefs.NullOrEmpty())
                {
                    // Iterate backwards to safely remove items without corrupting the loop or collection state.
                    // Only remove if they didn't become endogenes (preserving the "evolution" mechanic).
                    for (int ii = this.geneDefs.Count - 1; ii >= 0; ii--)
                    {
                        if (this.parent?.pawn?.genes?.HasXenogene(this.geneDefs[ii]) == true)
                        {
                            Gene gene = this.parent.pawn.genes.GetGene(this.geneDefs[ii]);
                            if (gene != null)
                            {
                                Log.Debug($"CompPostTick: {this.parent.pawn.Name} remove mutated xenogene # {ii} {gene.def.defName}");
                                this.parent.pawn.genes?.RemoveGene(gene);
                            }
                            else
                            {
                                Log.Debug($"CompPostTick: skipping invalid gene[{ii}]");
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

            // Iterate backwards for safety during removal
            for (int i = this.geneDefs.Count - 1; i >= 0; i--)
            {
                if (this.parent?.pawn?.genes?.HasXenogene(this.geneDefs[i]) == true)
                {
                    Gene gene = this.parent.pawn.genes.GetGene(this.geneDefs[i]);
                    if (gene != null)
                    {
                        Log.Debug($"CompPostPostRemoved: remove gene {gene.Label}");
                        this.parent.pawn.genes?.RemoveGene(gene);
                    }
                }
            }
            Active = false;
            this.geneDefs?.Clear();
        }

    }
}