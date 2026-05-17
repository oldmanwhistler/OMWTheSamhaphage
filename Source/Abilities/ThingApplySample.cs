using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionSample : NullThrumSelectionGene
    {
        public SelectionSample(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            
        }

        public override string Name => "Sample";
        // Stealing trash genes
        protected override float ResonanceTotalMultiplier => 0.5f;
        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeCredit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            List<Gene> alreadyHas = dest.genes.GenesListForReading ?? new List<Gene>();
            return source.genes.GenesListForReading
                .Where(g => !alreadyHas.Contains(g) && // ignore genes the caster already has
                            !g.Overridden && // can't steal a face if it's already overridden
                            this.GeneIsWorthless(g)) // want cosmetic genes only
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null) return new List<GeneDef>();
            return dest.genes.GenesListForReading.Select(g => g.def).ToList();
        }
    }

    public class ThingApplySample : NullThrumAbilityPawnCorpse
    {
        public override string AbilityName => "Sample";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Sample {victim.LabelShort} and steal their appearance to disguise yourself as one of their kind.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Sample");
        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            SelectionSample selector = new SelectionSample(caster, victim, caster);

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be stolen.",
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            bool value = false;
            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    selector.ResonanceCredit(plus);
                    victim.genes.RemoveGene(plus.gene);
                    caster.genes.AddGene(plus.gene.def, true);
                    Log.Debug($"Stole gene {plus.gene.LabelCap} from {victim.LabelShort}");
                    activated = true;
                }
                if (activated)
                {
                    PawnApplyRetune retune = new PawnApplyRetune();
                    retune.ApplyPawn(caster, caster);
                    value = true;
                }
            }));
            return value;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
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

            if (ResonanceUtility.Total(caster) < 1)
            {
                reason = $"Not enough Resonance to sample {p.LabelShort} and copy their appearance.";
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

            if (ResonanceUtility.Total(caster) < 1)
            {
                reason = "Not enough Resonance to steal a face.";
                return false;
            }

            return true;
        }
    }
}
