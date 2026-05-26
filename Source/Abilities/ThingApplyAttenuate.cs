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

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.attenuate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;


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
        private SelectionAttenuate selector = null;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.attenuate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;


        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Attenuate {victim.LabelShort} of their genes.\nConverts victim's genes to resonance.";
        }
        
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Attenuate");

        public SelectionAttenuate CanApplyAttenuate(Pawn victim, Pawn caster)
        {
            if (selector != null) return selector;
            selector = new SelectionAttenuate(caster, victim, caster);
            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Attenuated.", MessageTypeDefOf.RejectInput);
                return null;
            }            
            return selector;
        }

        public bool ApplyAttenuate(Pawn victim, Pawn caster, SelectionAttenuate selector)
        {
            bool activated = false;
            foreach (GenePlus plus in selector.genes)
            {
                selector.ResonanceCredit(plus);
                victim.genes.RemoveGene(plus.gene);
                activated = true;
            }
            return activated;
        }

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (selector == null) selector = CanApplyAttenuate(victim, caster);
            if (selector == null) return false;

            bool value = false;
            string msg = $"{victim.LabelShort} has died being attenuated for their resonance.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (ApplyAttenuate(victim, caster, selector))
                {
                    OMWAnomaly.PawnToShamblerOrKillDestroy(victim, caster);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                    value = true;
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowLethalConfirmation(victim, sacrificeAction);
            return value;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;
            if (corpse.InnerPawn == null) return false;

            Pawn victim = corpse.InnerPawn;

            if (selector == null) selector = CanApplyAttenuate(victim, caster);
            if (selector == null) return false;

            string msg = $"{victim.LabelShort} corpse was destroyed after being attenuated for their resonance.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                if (ApplyAttenuate(victim, caster, selector))
                {
                    // Use the Corpse-specific method to handle resurrection and initialization properly
                    OMWAnomaly.CorpseToShamblerOrDestroy(corpse);
                    Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                }
            };

            // Open the confirmation dialog
            OMW_UIHelpers.ShowCorpseConfirmation(victim, sacrificeAction);
            return false;
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (CanApplyAttenuate(victim, caster) == null)
            {
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

            if (CanApplyAttenuate(corpse.InnerPawn, caster) == null)
            {
                return false;
            }            

            return true;
        }
    }
}
