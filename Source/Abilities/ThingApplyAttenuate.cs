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

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def) && // ignore blacklisted
                        !this.GeneIsWorthless(g)) // ignore cosmetic genes
                .ToList();            
        }
    
        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }


    public class ThingApplyAttenuate : NullThrumAbilityPawnOnly
    {
        public override string AbilityName => "Attenuate";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Attenuate {victim.LabelShort} of their genes.\nConverts victim's genes to resonance.";
        }
        
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Attenuate");

        public override bool ApplyPawn(Pawn victim, Pawn caster = null)
        {
            if (victim == null || caster == null) return false;

            verb = new SelectionAttenuate(caster, victim, null);

            if (verb.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Attenuated.", MessageTypeDefOf.RejectInput);
                return false;
            }

            bool activated = false;
            string msg = $"{victim.LabelShort} has died being attenuated for their resonance.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                foreach (GenePlus plus in verb.genes)
                {
                    verb.ResonanceCredit(plus);
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

            if (!OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} does not have a scoured mind.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance";
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
