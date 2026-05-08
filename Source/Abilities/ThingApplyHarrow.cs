using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionHarrow : NullThrumSelectionGene
    {
        public SelectionHarrow(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override string Name => "Harrow";

        // More expensive because it is stealing genes
        protected override float ResonanceTotalMultiplier => 1.5f;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            HashSet<GeneDef> alreadyHas = dest.genes.GenesListForReading
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def) && // ignore blacklisted
                            !alreadyHas.Contains(g.def) && // ignore genes the caster already has
                            !this.GeneIsWorthless(g)) // ignore cosmetic genes
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null)
                return new List<GeneDef>();
            return dest.genes.GenesListForReading.Select(g => g.def).ToList();
        }        
    }

// ### Harrow (Theft)

// Reclaims and archives specific genes from the host.

// - Requires a scoured mind / blocked by dissonance.
// - Caster can pay resonance to take genes from Victim.
// - Applies dissonance to Victim.

    public class ThingApplyHarrow : NullThrumAbilityPawnCorpse
    {
        public override string VerbName => "Harrow";

        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Harrow {victim.LabelShort} and harvest their genes using resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Harrow");
        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;
            verb = new SelectionHarrow(caster, victim, caster);

            if (verb.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be harrowed.",
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(verb, (selectedList) =>
            {
                foreach (GenePlus plus in selectedList)
                {
                    if (verb.ResonanceDebit(plus))
                    {
                        victim.genes.RemoveGene(plus.gene);
                        caster.genes.AddGene(plus.gene.def, true);
                        Log.Message($"Harrowed {plus.gene.LabelCap} from {victim.LabelShort}");
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
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            if (ResonanceUtility.Total(caster) <= 2)
            {
                reason = $"{caster.LabelShort} does not have enough resonance to {this.VerbName}.";
                return false;
            }

            if (!OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} does not have a Scoured Mind. Must be Flattened.";
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
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            return true;
        }
    }
}
