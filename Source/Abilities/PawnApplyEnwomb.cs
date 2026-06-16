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
        public override bool IsLethal => false;

        private void PregnancyGenesTransfer(Pawn mother, Pawn father)
        {
            // Get all active genes from the father
            List<Gene> daddyGenes = father.genes.GenesListForReading
                .Where(g => !g.Overridden)
                .ToList();

            List<Gene> genesToTransfer = daddyGenes
                .Where(g => OMW_BlacklistGenes.PreggoGenes.Contains(g.def))
                .ToList();

            foreach (Gene gene in genesToTransfer)
            {
                if (!mother.genes.HasActiveGene(gene.def))
                {
                    mother.genes.AddGene(gene.def, xenogene: true);
                }
            }
        }

        public override void ApplyPawn(Pawn mother, Pawn father)
        {
            if (mother == null || father == null) return;

            if (!SacrificeCaster)
            {
                ApplyPregnancy(mother, father);
                doOnComplete();
                return;
            }

            string msg = $"{father.LabelShort} has died making {mother.LabelShort} pregnant.";
            System.Action sacrificeAction = () =>
            {
                if (ApplyPregnancy(mother, father))
                {
                    KillUtility.PawnKillDestroy(father, father);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                }

                // Needs to be false so doesn't get stuck on a loop
                                
            };

            ShowLethalConfirmation(father, sacrificeAction);
        }

        private bool ApplyPregnancy(Pawn mother, Pawn father)
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

            return CanApplyLimitXenotype(TargetXenotype, out reason);
        }
    }

    public class PawnApplyEnwombSacrifice : PawnApplyEnwomb
    {
        public override HediffDef TargetHediff => OMW_HediffDefOf.OMW_SilentServitude;
        public override XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_cradlemold;
        public override bool SacrificeCaster => true;
        public override bool IsLethal => true;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Sacrifice yourself to transform {victim.LabelShort} into a Cradlemold factory.";
    }
}