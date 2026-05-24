using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyEnwomb : NullThrumAbilityPawnOnly
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.enwomb;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Implant {victim.LabelShort} with a new life.";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Enwomb");

        public virtual HediffDef TargetHediff => null;
        public virtual XenotypeDef TargetXenotype => null;
        public virtual bool SacrificeCaster => false;

        private static List<GeneDef> cachedPreggoGenes;

        private void PregnancyGenesTransfer(Pawn mother, Pawn father)
        {
            // Get all active genes from the father
            List<Gene> daddyGenes = father.genes.GenesListForReading
                .Where(g => !g.Overridden)
                .ToList();

            if (cachedPreggoGenes == null)
            {
                List<string> preggoGeneNames = new List<string>
                {
                    "AG_EggLaying",
                    "BS_AutoPregnancy",
                    "WVC_IncestLover",
                    "BS_VeryEarlyMaturity",
                    "BS_EarlyMaturity",
                    "BS_BirthLitter",
                    "BS_BirthTwins",
                    "BS_MinimalPregnancy",
                    "BS_ShortPregnancy",
                    "AG_FastGestation",
                    "AG_SlowGestation",
                    "Sterile",
                    "VU_NearSterile",
                    "WVC_AgeDebuff_Sterile",
                    "BS_EverFertile",
                    "AG_FertilityIndoors",
                    "AG_FertilityDarkness",
                    "WVC_BaselinerFertility",
                    "Fertile",
                    "AG_ReducedFertile"
                };

                // Get a list of the actual GeneDefs for those names, filtering out any that don't exist in the current mod setup
                cachedPreggoGenes = preggoGeneNames.Select(name => DefDatabase<GeneDef>.GetNamed(name)).ToList();
            }

            List<Gene> genesToTransfer = daddyGenes
                .Where(g => cachedPreggoGenes.Contains(g.def))
                .ToList();

            foreach (Gene gene in genesToTransfer)
            {
                if (!mother.genes.HasActiveGene(gene.def))
                {
                    mother.genes.AddGene(gene.def, xenogene: true);
                }
            }
        }

        public override bool ApplyPawn(Pawn mother, Pawn father = null)
        {
            if (mother == null || father == null) return false;

            if (!SacrificeCaster)
            {
                return ExecutePregnancy(mother, father);
            }

            bool value = false;
            string msg = $"{father.LabelShort} has died making {mother.LabelShort} pregnant.";
            System.Action sacrificeAction = () =>
            {
                if (ExecutePregnancy(mother, father))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(father, father);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                    value = true;
                }
            };

            OMW_UIHelpers.ShowLethalConfirmation(father, sacrificeAction);
            return value;
        }

        private bool ExecutePregnancy(Pawn mother, Pawn father)
        {
            // Life finds a way.
            if (mother.gender != Gender.Female)
            {
                mother.genes.AddGene(DefDatabase<GeneDef>.GetNamed("AG_Female"), true);
            }
            OMWHediffs.RemoveHediff(mother, HediffDefOf.Sterilized);

            if (TargetXenotype != null)
            {
                OMWGenes.ChangeXenotype(mother, TargetXenotype);
            }

            OMWGenes.Refresh(mother);

            this.PregnancyGenesTransfer(mother, father);            

            Hediff_Pregnant hediff_Pregnant =
                (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
            hediff_Pregnant.Severity = PregnancyUtility.GeneratedPawnPregnancyProgressRange.TrueMin;
            hediff_Pregnant.SetParents(mother, father, null);
            mother.health.AddHediff(hediff_Pregnant);

            if (TargetHediff != null && !mother.health.hediffSet.HasHediff(TargetHediff))
            {
                mother.health.AddHediff(TargetHediff);
            }
            
            MoteMaker.MakeStaticMote(mother.TrueCenter(), mother.Map, ThingDefOf.Mote_ThoughtBad);
            return true;
        }

        public override bool CanApplyOnPawn(Pawn p, Pawn caster, out string reason)
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

    public class PawnApplyEnwombSacrifice : PawnApplyEnwomb
    {
        public override HediffDef TargetHediff => OMW_HediffDefOf.OMW_SilentServitude;
        public override XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_cradlemold;
        public override bool SacrificeCaster => true;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Sacrifice yourself to transform {victim.LabelShort} into a Cradlemold factory.";
    }
}