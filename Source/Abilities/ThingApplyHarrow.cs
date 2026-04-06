using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public class ThingApplyHarrow : NullThrumAbilityBase
    {

        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            // 1. Get the caster's genes as a HashSet of GeneDefs for high-speed lookup
            HashSet<GeneDef> casterGeneDefs = caster.genes.GenesListForReading
                .Select(g => g.def)
                .ToHashSet();

            // 2. Filter the victim's genes: only keep those whose Def is NOT in the caster's set
            List<Gene> genesToSelectFrom = victim.genes.GenesListForReading
                .Where(g => !casterGeneDefs.Contains(g.def))
                .ToList();

            int maxToPick = ResonanceUtility.Total(caster);
            bool returnedFromDialog = false;

            Find.WindowStack.Add(new Dialog_SelectMultipleGeneInstances(genesToSelectFrom, caster.genes.GenesListForReading, maxToPick, (selectedList) =>
            {
                if (selectedList != null && selectedList.Count > 0)
                {
                    ResonanceUtility.Decr(caster, selectedList.Count);
                    foreach (Gene gene in selectedList)
                    {
                        victim.genes.RemoveGene(gene);
                        Log.Message($"Removed {gene.Label} from {victim.LabelShort}");
                        caster.genes.AddGene(gene.def, true);
                        Log.Message($"Added {gene.Label} to {caster.LabelShort}");
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

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have available resonance.";
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

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have available resonance.";
                return false;
            }

            return true;
        }

        public override FloatMenuOption NewFloatMenuOptionPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new FloatMenuOption($"Harrow {pawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't harrow {pawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }

        public override FloatMenuOption NewFloatMenuOptionCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new FloatMenuOption($"Harrow {corpse.InnerPawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't harrow {corpse.InnerPawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }
    }
}
