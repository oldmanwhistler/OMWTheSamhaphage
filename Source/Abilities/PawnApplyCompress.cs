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

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.compress;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked  GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if (source?.genes == null) return blocked;

            HashSet<GeneDef> endogeneDefs = source.genes.Endogenes
                .Where(g => !g.Overridden)
                .Select(g => g.def)
                .ToHashSet();

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (!source.genes.Xenogenes.Contains(gene))
                {
                    Log.Debug($"blocking {gene.def} because germline");
                    blocked.Append(gene.def, "Germline");
                    isBlocked = true;
                }
                else if (endogeneDefs.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because already harmonized");
                    blocked.Append(gene.def, "Already Harmonized");
                    isBlocked = true;
                }
                if (!isBlocked)
                {
                    Log.Debug($"not blocking {gene.def}: {gene.Label}");
                }
            }
            return blocked;
        }

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionGeneBlocked blocked)
        {
            if (source?.genes == null) return new List<Gene>();
            return source.genes.GenesListForReading
                .Where(g => !blocked.Has(g.def))
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }


    public class PawnApplyCompress : NullThrumAbilityPawnOnly
    {
        private PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionCompress selectorCompress;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.compress;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harmonize {victim.LabelShort}'s genetic frequency, integrating their xenogenes into their endogenic sequence.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Compress");

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);
            
            ThingApplyScrub.DoAbility(victim, caster, () => ExecuteCompress(victim, caster));
        }

        private void ExecuteCompress(Pawn victim, Pawn caster)
        {
            // previous abilities could have changed the genes so make sure things still apply
            string reason;
            if (!CanApplyOnPawn(victim, caster, out reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                doOnComplete();
                return;
            }

            bool activated = false;
            foreach (GenePlus plus in selectorCompress.genes)
            {
                if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene) && selectorCompress.ResonanceDebit(plus))
                {
                    // Atomic Move: Manipulating lists directly prevents 3rd party mods from reacting to a "removed" gene 
                    // before the "added" gene exists, which was causing the Trait conflict NRE.
                    victim.genes.Xenogenes.Remove(plus.gene);
                    victim.genes.Endogenes.Insert(0, plus.gene);
                    Log.Debug($"Compressed {plus.gene.LabelCap} on {victim.LabelShort}");
                    activated = true;
                }
            }

            if (activated)
            {
                OMWGenes.ApplyDissonance(victim, caster);
                OMWGenes.Refresh(victim);
            }
            doOnComplete();
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

            selectorCompress = new SelectionCompress(caster, victim, victim);
            if (selectorCompress.genes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no genes that can be compressed.";
                return false;
            }

            return true;
        }
    }
}