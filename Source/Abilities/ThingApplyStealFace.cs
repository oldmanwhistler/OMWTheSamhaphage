using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public class VerbStealFace : NullThrumVerbBase
    {
        public VerbStealFace(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            
        }

        public override string Name => "Steal Face";
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

    public class ThingApplyStealFace : NullThrumAbilityBase
    {
        public override string VerbName => "Steal Face";
        
        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;     

            verb = new VerbStealFace(caster, victim, caster);

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
                    if (verb.PayResonance(plus))
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

        public override FloatMenuOption NewFloatMenuOptionPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new FloatMenuOption($"Steal Face {pawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't Steal Face {pawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }

        public override FloatMenuOption NewFloatMenuOptionCorpse(LocalTargetInfo targetInfo, Corpse corpse, Pawn caster = null)
        {
            string reason;

            if (CanApplyOnCorpse(corpse, caster, out reason))
            {
                return new FloatMenuOption($"Steal Face {corpse.InnerPawn.LabelShort}", () => Job(targetInfo, caster));
            }
            else
            {
                return new FloatMenuOption($"Can't Steal Face {corpse.InnerPawn.LabelShort} because {reason}", null) { Disabled = true };
            }
        }
    }
}
