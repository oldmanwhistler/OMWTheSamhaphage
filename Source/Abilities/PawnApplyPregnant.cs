using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Verse;

namespace OMW_Samhaphage
{
    public class PawnApplyPregnant
    {
        private List<GeneDef> preggoGenes;

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
                // Order implies prioritization
                List<string> preggoGeneNames = new List<string>();            
                preggoGeneNames.AddRange(new string[]
                {
                    // eggs
                    "AG_EggLaying",
                    // weird
                    "BS_AutoPregnancy",
                    "WVC_IncestLover",
                    // maturity
                    "BS_VeryEarlyMaturity",
                    "BS_EarlyMaturity",
                    // amount
                    "BS_BirthLitter",
                    "BS_BirthTwins",
                    // gestation
                    "BS_MinimalPregnancy",
                    "BS_ShortPregnancy",
                    "AG_FastGestation",
                    "AG_SlowGestation",
                    // fertility
                    "Sterile",
                    "VU_NearSterile",
                    "WVC_AgeDebuff_Sterile",
                    "BS_EverFertile",
                    "AG_FertilityIndoors",
                    "AG_FertilityDarkness",
                    "WVC_BaselinerFertility",
                    "Fertile",
                    "AG_ReducedFertile"
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

        public bool Apply(Pawn mother, Pawn father, HediffDef targetHeDiff = null, XenotypeDef targetXenotype = null)
        {
            if (mother == null || father == null) return false;

            // Life finds a way.
            if (mother.gender != Gender.Female)
            {
                mother.genes.AddGene(DefDatabase<GeneDef>.GetNamed("AG_Female"), true);
            }
            OMWHediffs.RemoveHediff(mother, HediffDefOf.Sterilized);

            if (targetXenotype != null)
            {
                OMWGenes.ChangeXenotype(mother, null, targetXenotype);
            }

            OMWGenes.Refresh(mother);

            this.pregnancyGenesTransfer(mother, father);            

            // Tried to vibe code this; failed. I ended up copying it from MiscUtility.cs in WVC_RacesBiotech
            Hediff_Pregnant hediff_Pregnant =
                (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
            hediff_Pregnant.Severity = PregnancyUtility.GeneratedPawnPregnancyProgressRange.TrueMin;
            hediff_Pregnant.SetParents(mother, father, null);
            mother.health.AddHediff(hediff_Pregnant);

            if (targetHeDiff != null)
            {
                mother.health.AddHediff(targetHeDiff);
            }
            
            // Optional: Visual mote to show it worked
            MoteMaker.MakeStaticMote(mother.TrueCenter(), mother.Map, ThingDefOf.Mote_ThoughtBad);
            return true;
        }

        public void ApplySacrifice(Pawn mother, Pawn father, HediffDef targetHeDiff = null, XenotypeDef
            targetXenotype = null)
        {
            string msg = $"{father.LabelShort} has died making {mother.LabelShort} pregnant.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (Apply(mother, father, targetHeDiff, targetXenotype))
                {

                    OMWAnomaly.PawnToShamblerOrKillDestroy(father, father);                    
                    Messages.Message(msg,
                        MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(father, sacrificeAction);
        }

        public static bool CanApplyOn(Pawn p, out string reason)
        {
            reason = "unknown reason";
            if (p == null) 
            {
                reason = "Target is null.";
                return false;
            }

            if (!Find.Storyteller.difficulty.ChildrenAllowed)
            {
                reason = "Children are not allowed on this difficulty.";
                return false;
            }

            // Check if target is a not already pregnant
            if (!p.RaceProps.Humanlike)
            {
                reason = "Target is not humanlike.";
                return false;
            }
            if (p.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
            {
                reason = "Target is already pregnant.";
                return false;
            }

            return true;
        }
    }
}