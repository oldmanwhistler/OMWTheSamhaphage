using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionScrub : NullThrumSelectionGene
    {
        public SelectionScrub(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) {}
        
        public override string Name => "Scrub";
        // Cheap because it is destroying genes
        protected override float ResonanceTotalMultiplier => 0.5f;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def) && // ignore blacklisted
                        g.Overridden && // must be overridden to be scrubbed
                        !this.GeneIsWorthless(g)) // ignore cosmetic genes
                .ToList();            
        }
    
        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }

// ### Scrub (Harvest)

// Collect carcinomas and disabled genes as resonance.

// - Requires a scoured mind / blocked by dissonance.
// - Victim loses carcinomas and Caster gains resonance.
// - Caster can pay resonance to destroy disabled genes on Victim.
// - Applies dissonance to Victim.

    public class ThingApplyScrub : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override string AbilityName => "Scrub";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Scrub {victim.LabelShort} of their carcinomas and useless genes.\nConverts carcinomas to resonance and opens a menu to destroy deactivated genes.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Scrub");

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

            float amount = carcinomas.Count * 1.5f;
            ResonanceUtility.Incr("from removing carcinomas", caster, amount);

            foreach (Hediff carcinoma in carcinomas)
            {
                victim.health.RemoveHediff(carcinoma);
            }

            return true;
        }

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            SelectionScrub selector = new SelectionScrub(caster, victim, null);

            RemoveCarcinomas(victim, caster);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            } 

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be scrubbed.", MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, (selectedList) =>
            {
                foreach (GenePlus plus in selectedList)
                {
                    if (selector.ResonanceDebit(plus))
                    {
                        victim.genes.RemoveGene(plus.gene);
                        Log.Message($"Destroyed {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }
            }));

            if (activated)
            {
                selector.ApplyDissonance(victim, caster);
            }
            return activated;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;
            if (corpse.InnerPawn == null) return false;

            return ApplyPawn(corpse.InnerPawn, caster);
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
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
    }
}
