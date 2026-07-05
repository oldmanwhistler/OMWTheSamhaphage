using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public class HediffCompProperties_Assimilator : HediffCompProperties
    {
        public int numSlugsEaten;
        public HediffCompProperties_Assimilator()
        {
            this.compClass = typeof(HediffComp_Assimilator);
        }       
    }

    public class HediffComp_Assimilator : HediffComp
    {
        private HediffCompProperties_Assimilator Props => (HediffCompProperties_Assimilator)props;        

        public override string CompTipStringExtra
        {
            get { return $"Fluxspawn consumed: {Props.numSlugsEaten}"; }
        }        

        public void EatSlug()
        {
            Log.Message("HediffComp_Assimilator: ate a slug");
            Props.numSlugsEaten += 1;
        }

        public int SlugsEatten()
        {
            return Props.numSlugsEaten;
        }

        // Once a day integrate a random xenogene

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.parent?.pawn.Map == null) return;

            Log.Message($"HediffComp_Assimilator: {parent.pawn.Name} CompPostTick({severityAdjustment})");

            if (severityAdjustment >= 1f)
            {
                severityAdjustment = 0.1f;
                IntegrateXenogene();
            }
        }

        private void IntegrateXenogene()
        {
            if (this.parent?.pawn?.genes?.Xenogenes.Count == 0) return;

            Log.Message($"HediffComp_Assimilator: integrating a random xenogene for {parent.pawn.Name}");

            List<Gene> xenogenes = this.parent.pawn.genes.Xenogenes;
            xenogenes.Shuffle();
            Gene gene = xenogenes[0];

            Log.Message($"HediffComp_Assimilator: {parent.pawn.Name} integrating {gene.def.label}");

            this.parent.pawn.genes.Xenogenes.Remove(gene);
            this.parent.pawn.genes.Endogenes.Insert(0, gene);
            OMWGenes.Refresh(this.parent.pawn);
        }
    }
}

