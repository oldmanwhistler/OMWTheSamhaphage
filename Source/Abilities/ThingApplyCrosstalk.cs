using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionCrosstalk : NullThrumSelectionGene
    {
        public SelectionCrosstalk(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.crosstalk;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked  GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if (source?.genes == null) return blocked;

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (!source.genes.Xenogenes.Contains(gene))
                {
                    Log.Debug($"blocking {gene.def} because germline");
                    blocked.Append(gene.def, "Germline");
                    isBlocked = true;
                }
                else if (OMW_BlacklistGenes.BlacklistedGenesDontCopy.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because don't copy");
                    blocked.Append(gene.def, "Don't Copy");
                    isBlocked = true;
                }
                else if (OMW_BlacklistGenes.BlacklistedGenesDontRemove.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because don't remove");
                    blocked.Append(gene.def, "Don't Remove");
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

    public class ThingApplyCrosstalk : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        SelectionCrosstalk selector1;
        SelectionCrosstalk selector2;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.crosstalk;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Induce a genetic crosstalk with {victim.LabelShort}, using resonance to harvest xenogenes at random.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Crosstalk");

        public override void ApplyPawn(Pawn victim, Pawn caster) => ApplyPawn(victim, caster, null);

        public void ApplyPawn(Pawn victim, Pawn caster, System.Action onAbilityComplete)
        {
            if (victim == null || caster == null) return;

            if (onAbilityComplete == null) onAbilityComplete = onCompleteAction();

            OMWGenes.Refresh(victim);
            OMWGenes.Refresh(caster);

           if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            if ((selector1.genes.Count == 0) && (selector2.genes.Count == 0))
            {
                Messages.Message($"{victim.LabelShort} has no xenogenetic frequencies to harvest.",
                    MessageTypeDefOf.RejectInput);
                onAbilityComplete?.Invoke();
                return;
            }

            bool activated = false;

            // shuffle the selectors in place because crosstalk should be very random which is what makes it cheap
            selector1.Shuffle();
            selector2.Shuffle();

            List<GeneDef> genesFromSource = new List<GeneDef>();
            List<GeneDef> genesFromDest = new List<GeneDef>();

            int amount = Mathf.Min(selector1.genes.Count, selector2.genes.Count);
            int max = Mathf.Max(selector1.genes.Count, selector2.genes.Count);

            // Go through the entire list until you've gotten the correct amount for each source or run out of resonance.
            // Because you will skip over some of the genes because they are too expensive w.r.t. resonance you need to
            // go over the entire list.
            for(int ii=0; ii<max; ii++)
            {
                GenePlus plus1 = null;
                if (selector1.genes.Count > ii) plus1 = selector1.genes[ii];
                if ((genesFromSource.Count < amount) && plus1.gene != null && victim.genes.GenesListForReading.Contains(plus1.gene) && selector1.ResonanceDebit(plus1))
                {
                    victim.genes.RemoveGene(plus1.gene);
                    genesFromSource.Add(plus1.gene.def);
                    activated = true;                        
                }
                GenePlus plus2 = null;
                if (selector2.genes.Count > ii) plus2 = selector2.genes[ii];
                if ((genesFromDest.Count < amount) && plus2.gene != null && caster.genes.GenesListForReading.Contains(plus2.gene) && selector2.ResonanceDebit(plus2))
                {
                    caster.genes.RemoveGene(plus2.gene);
                    genesFromDest.Add(plus2.gene.def);
                    activated = true;
                }
            }
            if (activated)
            {
                Log.Debug($"Crosstalk attempting to switch {genesFromSource.Count} xenogenes from {victim.LabelShort} with {genesFromDest.Count} xenogenes from {caster.LabelShort}");
                OMWGenes.PrependXenogenes(caster, genesFromSource);
                OMWGenes.PrependXenogenes(victim, genesFromDest);
                Log.Debug($"Crosstalk exchanged {genesFromSource.Count} xenogenes from {victim.LabelShort} with {genesFromDest.Count} xenogenes from {caster.LabelShort}");
                OMWGenes.ApplyDissonance(victim, caster);
                OMWGenes.Refresh(caster);                
                PawnApplyRetune.DoAbility(victim, caster, onAbilityComplete);            
            }
            else
            {
                onAbilityComplete.Invoke();
            }
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;
            ApplyPawn(corpse.InnerPawn, caster);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            if (victim.genes == null)
            {
                reason = $"{victim.LabelShort} has no genes.";
                return false;                
            }

            if (victim.genes.Xenogenes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no xenogenes.";
                return false;
            }

            if (caster.genes.Xenogenes.Count == 0)
            {
                reason = $"{caster.LabelShort} has no xenogenes.";
                return false;
            }

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }


            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            if (ResonanceUtility.Total(caster) <= 3)
            {
                reason = $"{caster.LabelShort} does not have enough resonance to {this.AbilityName}.";
                return false;
            }

            if (selector1 == null) selector1 = new SelectionCrosstalk(caster, victim, caster);
            if (selector2 == null) selector2 = new SelectionCrosstalk(caster, caster, victim);

            if (selector1.genes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no xenogenes for crosstalk.";
                return false;
            }

            if (selector2.genes.Count == 0)
            {
                reason = $"{caster.LabelShort} has no xenogenes for crosstalk.";
                return false;
            }


            return CanApplyLimitMetabolism(caster, out reason);
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            if (corpse == null) { reason = "Target is null."; return false; }
            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}