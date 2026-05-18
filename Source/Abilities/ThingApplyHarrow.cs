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

        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Harrow;

        // More expensive because it is stealing genes
        protected override float ResonanceTotalMultiplier => OMW_Mod.settings.multHarrow;
        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeDebit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            HashSet<GeneDef> alreadyHas = dest.genes.GenesListForReading
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenesResonance.Contains(g.def) && // ignore blacklisted
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
        PawnApplyFlatten Flatten = new PawnApplyFlatten();        
        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Harrow;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harrow {victim.LabelShort} and harvest their genes using resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Harrow");
        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            ThingApplyScrub scrub = new ThingApplyScrub();
            // Chain Harrow window to open after Scrub is done
            return scrub.ApplyPawn(victim, caster, () => OpenHarrowWindow(victim, caster));
        }

        private void OpenHarrowWindow(Pawn victim, Pawn caster)
        {
            SelectionHarrow selector = new SelectionHarrow(caster, victim, caster);
            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be harrowed.", MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (selector.ResonanceDebit(plus))
                    {
                        victim.genes.RemoveGene(plus.gene);
                        caster.genes.AddGene(plus.gene.def, true);
                        Log.Debug($"Harrowed {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }

                if (activated)
                {
                    // Retune the caster after harvesting to integrate new genes
                    PawnApplyRetune retune = new PawnApplyRetune();
                    retune.ApplyPawn(caster, caster);
                    selector.ApplyDissonance(victim, caster);
                }
            }));
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

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            if (ResonanceUtility.Total(caster) <= 2)
            {
                reason = $"{caster.LabelShort} does not have enough resonance to {this.AbilityName}.";
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
