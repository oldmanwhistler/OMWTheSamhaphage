using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public class ThingApplyScrub : NullThrumAbilityBase
    {
        public static bool RemoveCarcinomas(Pawn victim, Pawn caster)
        {
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in victim.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            if (carcinomas.Count == 0)
            {
                Log.Message($"{victim.LabelShort} doesn't have any carcinomas to remove.");
                return false;
            }

            ResonanceUtility.Incr("from removing carcinomas", caster, carcinomas.Count);

            foreach (Hediff carcinoma in carcinomas)
            {
                victim.health.RemoveHediff(carcinoma);
            }

            return true;
        }
        
        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            RemoveCarcinomas(victim, caster);            

            List<Gene> genesToSelectFrom = victim.genes.GenesListForReading
                .Where(g => g.Overridden)
                .ToList();

            int maxToPick = 100;
            bool returnedFromDialog = false;

            Find.WindowStack.Add(new Dialog_SelectMultipleGeneInstances(genesToSelectFrom, caster.genes.GenesListForReading, maxToPick, "Destroy", (selectedList) =>
            {
                if (selectedList != null && selectedList.Count > 0)
                {
                    ResonanceUtility.Incr($"from destroying {victim.LabelShort}'s genes", caster, selectedList.Count);
                    foreach (Gene gene in selectedList)
                    {
                        victim.genes.RemoveGene(gene);
                        Log.Message($"Destroyed {gene.Label} from {victim.LabelShort}");
                    }

                    // GeneticDissonance prevents repeated calls
                    Hediff hediffDissonance = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_GeneticDissonance, caster);
                    victim.health.AddHediff(hediffDissonance);
                }

                returnedFromDialog = true;
            }));

            return returnedFromDialog;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster = null)
        {
            if (corpse == null || caster == null) return false;
            if (corpse.InnerPawn == null) return false;

            return ApplyPawn(corpse.InnerPawn, caster);
        }


        public override bool CanApplyOnPawn(Pawn p, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (p == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (!OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} does not have a scoured mind.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance";
                return false;
            }

            return true;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (corpse == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (!corpse.InnerPawn.RaceProps.Humanlike)
            {
                reason = $"{corpse.InnerPawn.LabelShort} is not humanlike.";
                return false;
            }

            return true;
        }

        public override FloatMenuOption NewFloatMenuOptionPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new FloatMenuOption($"Filter {pawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't Filter {pawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }

        public override FloatMenuOption NewFloatMenuOptionCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new FloatMenuOption($"Scrub {corpse.InnerPawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't Scrub {corpse.InnerPawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }
    }
}
