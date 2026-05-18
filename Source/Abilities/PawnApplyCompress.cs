using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionCompress : NullThrumSelectionGene
    {        
        public SelectionCompress(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Compress;

        // More expensive because it is stealing genes
        protected override float ResonanceTotalMultiplier => OMW_Mod.settings.multCompress;
        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeDebit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            // Compress is moving Xenogenes to Endogenes on source
            HashSet<GeneDef> alreadyHas = source.genes.Endogenes
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.Xenogenes
                .Where(g => !alreadyHas.Contains(g.def))
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }


    public class PawnApplyCompress : NullThrumAbilityPawnOnly
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Compress;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harmonize {victim.LabelShort}'s genetic frequency, integrating their xenogenes into their endogenic sequence.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Compress");

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            ThingApplyScrub scrub = new ThingApplyScrub();
            // Chain the compression logic to run only after the scrub window is closed
            return scrub.ApplyPawn(victim, caster, () => ExecuteCompress(victim, caster));
        }

        private void ExecuteCompress(Pawn victim, Pawn caster)
        {
            SelectionCompress selector = new SelectionCompress(caster, victim, victim);
            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Compressed.", MessageTypeDefOf.RejectInput);
                return;
            }

            bool activated = false;
            foreach (GenePlus plus in selector.genes)
            {
                if (selector.ResonanceDebit(plus))
                {
                    victim.genes.RemoveGene(plus.gene);
                    victim.genes.AddGene(plus.gene.def, false);
                    Log.Debug($"Compressed {plus.gene.LabelCap} on {victim.LabelShort}");
                    activated = true;
                }
            }

            if (activated)
            {
                selector.ApplyDissonance(victim, caster);
            }
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }

            if (!ResonanceUtility.HasAvailable(caster))
            {
                reason = $"{caster.LabelShort} does not have enough available resonance.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {            
                reason = $"{victim.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            return true;
        }
    }
}