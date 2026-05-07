using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class VerbRetune : NullThrumVerbBase
    {
        public VerbRetune(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override string Name => "Retune";

        // More expensive because it is stealing genes
        protected override float ResonanceTotalMultiplier => 0.5f;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            // Retune is moving Xenogenes to Endogenes on source
            HashSet<GeneDef> alreadyHas = source.genes.Endogenes
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.Xenogenes
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def) && // ignore blacklisted
                            !alreadyHas.Contains(g.def)) // ignore genes the caster already has
                            //!this.GeneIsWorthless(g)) // ignore cosmetic genes
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null)
                return new List<GeneDef>();
            return dest.genes.GenesListForReading.Select(g => g.def).ToList();
        }        
    }


    public class PawnApplyRetune : NullThrumAbilityPawnOnly
    {
        public override string VerbName => "Retune";

        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Harmonize {victim.LabelShort}'s genetic frequency, integrating their xenogenes into their endogenic sequence.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/Retune");

        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            verb = new VerbRetune(caster, victim, caster);

            if (verb.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be retuned.",
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;

            Find.WindowStack.Add(new WindowSelectGenesForVerb(verb, (selectedList) =>
            {
                foreach (GenePlus plus in selectedList)
                {
                    if (verb.ResonanceDebit(plus))
                    {
                        victim.genes.RemoveGene(plus.gene);
                        victim.genes.AddGene(plus.gene.def, true);
                        Log.Message($"Retuned {plus.gene.LabelCap} on {victim.LabelShort}");
                        activated = true;
                    }
                }
            }));

            if (activated)
            {
                verb.ApplyDissonance(victim, caster);
            }

            return activated;
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null) 
            {
                reason = "Target is null.";
                return false;
            }            
            // Check if target is a not already Retune
            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            if (!ResonanceUtility.HasAvailable(caster))
            {
                reason = $"{caster.LabelShort} does not have enough available resonance.";
                return false;
            }

            if (!OMWGenes.HasScouredMind(victim))
            {
                reason = $"{victim.LabelShort} has not had their mind scoured to prepare them for genetic manipulation.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {            
                reason = $"{victim.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
            {
                reason = $"{victim.LabelShort} is affected by Silent Servitude.";
                return false;
            }

            if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{victim.LabelShort} is affected by Genetic Dissonance";
                return false;
            }

            return true;
        }
    }
}