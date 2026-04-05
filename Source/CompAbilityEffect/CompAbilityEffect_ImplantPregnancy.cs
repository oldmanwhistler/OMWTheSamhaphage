using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityImplantWithPregnancy : CompProperties_AbilityEffect
    {
        // overwridden by AbilityDef
        public HediffDef hediffDef;

        // overwridden by AbilityDef
        public string targetXenotype = "Baseliner";
        public CompProperties_AbilityImplantWithPregnancy()
        {
            compClass = typeof(CompAbilityEffect_ImplantWithPregnancy);
        }
    }

    public class CompAbilityEffect_ImplantWithPregnancy : CompAbilityEffect
    {
        private new CompProperties_AbilityImplantWithPregnancy Props => (CompProperties_AbilityImplantWithPregnancy)props;
        private List<GeneDef> preggoGenes;
        private void AbsorbXenogenes(Pawn pawn)
        {
            // 1. Snapshot the Xenogenes (copy to avoid modification-during-enumeration errors)
            List<GeneDef> genesToMove = new List<GeneDef>();
            foreach (Gene xenoGene in pawn.genes.Xenogenes)
            {
                genesToMove.Add(xenoGene.def);
            }

            // 2. Remove all Xenogenes
            pawn.genes.ClearXenogenes();

            // 3. Add them back as Endogenes (Germline)
            foreach (GeneDef geneDef in genesToMove)
            {
                // The second parameter 'false' indicates it is NOT a xenogene
                pawn.genes.AddGene(geneDef, false);
            }
        }
        private void ImplantXenotype(Pawn target, XenotypeDef sourceXenotype)
        {
            // Clear existing xenogenes first if you want a clean override
            target.genes.ClearXenogenes();

            // Update the label so the UI shows the correct Xenotype name
            target.genes.SetXenotypeDirect(sourceXenotype);

            // Add all genes from the new xenotype as Xenogenes
            foreach (GeneDef geneDef in sourceXenotype.AllGenes)
            {
                target.genes.AddGene(geneDef, xenogene: true);
            }

            target.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void pregnancyGenesTransfer(Pawn mother, Pawn father)
        {
            // Get all active genes from the father
            List<Gene> daddyGenes = father.genes.GenesListForReading
                .Where(g => !g.Overridden)
                .ToList();

            // will only execute once
            if (preggoGenes == null)
            {
                // Names of all the pregnancy genes in common mods I support
                List<string> preggoGeneNames = new List<string>();
                preggoGeneNames.AddRange(new string[] {
                    "Sterile",
                    "Fertile",
                    "BS_AutoPregnancy",
                    "BS_EarlyMaturity",
                    "BS_EverFertile",
                    "BS_MinimalPregnancy",
                    "BS_BirthLitter",
                    "BS_BirthTwins",
                    "BS_ShortPregnancy",
                    "BS_VeryEarlyMaturity",
                    "BS_EarlyMaturity",
                    "BS_BirthTwins",
                    "BS_BirthLitter",
                    "BS_MinimalPregnancy",
                    "BS_ShortPregnancy",
                    "BS_EverFertile",
                    "VU_NearSterile",
                    "AG_ReducedFertile",
                    "AG_FertilityDarkness",
                    "AG_FertilityLight",
                    "AG_FertilityOutdoors",
                    "AG_FertilityIndoors",
                    "AG_FastGestation",
                    "AG_SlowGestation",
                    "AG_EggLaying",
                    "WVC_IncestLover",
                    "WVC_AgeDebuff_Sterile",
                    "WVC_BaselinerFertility"
                });

                // Get a list of the actual GeneDefs for those names, filtering out any that don't exist in the current mod setup
                preggoGenes = preggoGeneNames.Select(name => DefDatabase<GeneDef>.GetNamed(name)).ToList();
            }


            // Find which of the father's active genes match our "preggo" list
            List<Gene> genesToTransfer = daddyGenes
                .Where(g => preggoGenes.Contains(g.def))
                .ToList();

            // Add those genes to the mother
            foreach (Gene gene in genesToTransfer)
            {
                // Check if mother already has it to avoid duplicates/errors
                if (!mother.genes.HasActiveGene(gene.def))
                {
                    // Second parameter 'true' usually adds it as a Xenogene
                    mother.genes.AddGene(gene.def, xenogene: true);
                }
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn mother = target.Pawn;
            Pawn father = this.parent.pawn;

            if (mother == null || father == null) return;

            // Apply hediff from ability to mother
            mother.health.AddHediff(Props.hediffDef);

            // Move xenotypes to genes
            this.AbsorbXenogenes(mother);
            // Change the mother to new xenotype
            XenotypeDef sourceXenotype = DefDatabase<XenotypeDef>.GetNamed(Props.targetXenotype);
            this.ImplantXenotype(mother, sourceXenotype);
            this.pregnancyGenesTransfer(mother, father);

            HediffDef hediffSilentServitude = OMW_HediffDefOf.OMW_SilentServitude;
            mother.health.AddHediff(hediffSilentServitude);

            if (!Find.Storyteller.difficulty.ChildrenAllowed)
            {
                return;
            }

            // Tried to vibe code this; failed. I ended up copying it from MiscUtility.cs in WVC_RacesBiotech
            Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
            hediff_Pregnant.Severity = PregnancyUtility.GeneratedPawnPregnancyProgressRange.TrueMin;
            hediff_Pregnant.SetParents(mother, father, null);
            mother.health.AddHediff(hediff_Pregnant);

            // Optional: Visual mote to show it worked
            MoteMaker.MakeStaticMote(mother.TrueCenter(), mother.Map, ThingDefOf.Mote_ThoughtBad);
        }

        // Validation logic for the 1.6 targeter
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn p = target.Pawn;
            if (p == null) return false;

            // Check if target is a not already pregnant
            if (!p.RaceProps.Humanlike) return false;
            if (p.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman)) return false;

            // Love finds a way.
            if (p.gender != Gender.Female)
            {
                p.genes.AddGene(DefDatabase<GeneDef>.GetNamed("AG_Female"), true);
            }

            return base.CanApplyOn(target, dest);
        }
    }
}