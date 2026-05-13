using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionAttenuate : NullThrumSelectionGene
    {
        public SelectionAttenuate(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) {}
        
        public override string Name => "Attenuate";
        // Cheap because it is destroying genes
        protected override float ResonanceTotalMultiplier => 1f;

        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeCredit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            return source.genes.GenesListForReading.ToList();            
        }
    
        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }


    public class ThingApplyAttenuate : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override string AbilityName => "Attenuate";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Attenuate {victim.LabelShort} of their genes.\nConverts victim's genes to resonance.";
        }
        
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Attenuate");

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            SelectionAttenuate selector = new SelectionAttenuate(caster, victim, caster);

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Attenuated.", MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;
            string msg = $"{victim.LabelShort} has died being attenuated for their resonance.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                foreach (GenePlus plus in selector.genes)
                {
                    selector.ResonanceCredit(plus);
                    victim.genes.RemoveGene(plus.gene);
                    activated = true;
                }
                if (activated) {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(victim, caster);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(victim, sacrificeAction);

            return activated;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;
            if (corpse.InnerPawn == null) return false;

            Pawn victim = corpse.InnerPawn;

            SelectionAttenuate selector = new SelectionAttenuate(caster, victim, caster);

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Attenuated.", MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;
            string msg = $"{victim.LabelShort} corpse was destroyed after being attenuated for their resonance.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                foreach (GenePlus plus in selector.genes)
                {
                    selector.ResonanceCredit(plus);
                    victim.genes.RemoveGene(plus.gene);
                    activated = true;
                }
                if (activated) {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(victim, caster);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowCorpseConfirmation(victim, sacrificeAction);

            return activated;
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }

            if (victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{victim.LabelShort} is affected by Genetic Dissonance";
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
