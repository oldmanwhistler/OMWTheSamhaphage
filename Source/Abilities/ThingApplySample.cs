using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class VerbSample : NullThrumVerbBase
    {
        public VerbSample(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            
        }

        public override string Name => "Sample";
        // Stealing trash genes
        protected override float ResonanceTotalMultiplier => 0.75f;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            List<Gene> alreadyHas = dest.genes.GenesListForReading ?? new List<Gene>();
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def) && // ignore blacklisted
                            !alreadyHas.Contains(g) && // ignore genes the caster already has
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
        public override string VerbName => "Sample";

        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            return $"Sample {victim.LabelShort} and steal their appearance to disguise yourself as one of their kind.";
        }
        public override Texture2D Icon => BaseContent.BadTex;
        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            verb = new VerbSample(caster, victim, caster);

            if (verb.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be stolen.",
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
                        caster.genes.AddGene(plus.gene.def, true);
                        Log.Message($"Stole gene {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }
            }));

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

            if (ResonanceUtility.Total(caster) < 1)
            {
                reason = "Not enough Resonance to steal a face.";
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
