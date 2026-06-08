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

            ThingApplyScrub scrub = new ThingApplyScrub();
            // Chain the compression logic to run only after the scrub window is closed
            scrub.ApplyPawn(victim, caster, () => ExecuteCompress(victim, caster));
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