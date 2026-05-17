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

        public override string Name => "Crosstalk";

        protected override float ResonanceTotalMultiplier => 0.5f;
        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeDebit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            // Crosstalk targets genes that are overridden (inactive signals)
            return source.genes.Xenogenes
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def)) 
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
        public override string AbilityName => "Crosstalk";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Induce a genetic crosstalk with {victim.LabelShort}, using resonance to harvest xenogenes at random.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Crosstalk");

        public override bool ApplyPawn(Pawn victim, Pawn caster) => ApplyPawn(victim, caster, null);

        public bool ApplyPawn(Pawn victim, Pawn caster, System.Action onComplete)
        {
            if (victim == null || caster == null) return false;

           if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            SelectionCrosstalk selector1 = new SelectionCrosstalk(caster, victim, caster);
            SelectionCrosstalk selector2 = new SelectionCrosstalk(caster, caster, victim);

            if ((selector1.genes.Count == 0) && (selector2.genes.Count == 0))
            {
                Messages.Message($"{victim.LabelShort} has no xenogenetic frequencies to harvest.",
                    MessageTypeDefOf.RejectInput);
                onComplete?.Invoke();
                return false;
            }

            bool activated = false;
            List<GeneDef> genesFromSource = new List<GeneDef>();
            List<GeneDef> genesFromDest = new List<GeneDef>();

            while (selector1.genes.Count > 0 || selector2.genes.Count > 0)
            {
                if (selector1.genes.Count > 0)
                {
                    GenePlus plus1 = selector1.genes.RandomElement();
                    selector1.genes.Remove(plus1);

                    if (selector1.ResonanceDebit(plus1))
                    {
                        victim.genes.RemoveGene(plus1.gene);
                        genesFromSource.Add(plus1.gene.def);
                        activated = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (selector2.genes.Count > 0)
                {
                    GenePlus plus2 = selector2.genes.RandomElement();
                    selector2.genes.Remove(plus2);

                    if (selector2.ResonanceDebit(plus2))
                    {
                        caster.genes.RemoveGene(plus2.gene);
                        genesFromDest.Add(plus2.gene.def);
                        activated = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (activated)
            {
                Log.Message($"Crosstalk attempting to switch {genesFromSource.Count} xenogenes from {victim.LabelShort} with {genesFromDest.Count} xenogenes from {caster.LabelShort}");
                OMWGenes.PrependXenogenes(caster, genesFromSource);
                OMWGenes.PrependXenogenes(victim, genesFromDest);
                Log.Message($"Crosstalk exchanged {genesFromSource.Count} xenogenes from {victim.LabelShort} with {genesFromDest.Count} xenogenes from {caster.LabelShort}");
                selector1.ApplyDissonance(victim, caster);
                PawnApplyRetune retune = new PawnApplyRetune();
                retune.ApplyPawn(caster, caster);            
            }

            return true;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return false;
            return ApplyPawn(corpse.InnerPawn, caster);
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

            return true;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            if (corpse == null) { reason = "Target is null."; return false; }
            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}