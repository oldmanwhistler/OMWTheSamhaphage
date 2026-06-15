using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionAttenuateGenes : NullThrumSelectionGene
    {
        public SelectionAttenuateGenes(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) {}

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.attenuate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked  GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            return blocked;
        }

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionGeneBlocked blocked)
        {
            return source.genes.GenesListForReading.Where(g => !blocked.Has(g.def)).ToList();
        }
    
        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }

    public class SelectionAttenuateTraits : NullThrumSelectionTrait
    {
        public SelectionAttenuateTraits(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
        }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.attenuate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionTraitBlocked TraitsBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionTraitBlocked blocked = new();
            return blocked; 
        }

        protected override List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionTraitBlocked blocked)
        {
            if (source?.story?.traits == null)
                return new List<Trait>();

            // For Excise, we don't care about conflicts in the destination 
            // because we aren't adding the trait to the caster, just removing it.
            return source.story.traits.allTraits
                .Where(t => (t.sourceGene == null) &&
                            !OMW_BlacklistTraits.BlacklistedTraitsDontRemove.Contains(t.def)
                )
                .ToList();
        }

        protected override List<TraitDef> ConflictTraitDefs(Pawn source, Pawn dest)
        {
            return new List<TraitDef>();
        }
    }    


    public class ThingApplyAttenuate : NullThrumAbilityPawnCorpse
    {
        private SelectionAttenuateGenes selectorGenes = null;
        private SelectionAttenuateTraits selectorTraits = null;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.attenuate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        // FIXME only Lethal is this is a corpse
        public override bool IsLethal => true;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Attenuate {victim.LabelShort} of their genes.\nConverts victim's genes and traits to resonance.";
        }
        
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Attenuate");    

        private bool ApplyAttenuate(Pawn victim, Pawn caster)
        {
            bool activated = false;
            foreach (GenePlus plus in selectorGenes.genes)
            {
                selectorGenes.ResonanceCredit(plus);
                victim.genes.RemoveGene(plus.gene);
                activated = true;
            }

            foreach (TraitPlus plus in selectorTraits.traits)
            {
                selectorTraits.ResonanceCredit(plus);
                victim.story?.traits?.RemoveTrait(plus.trait);
                activated = true;
            }                

            return activated;
        }

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;
            string reason;
            
            if (!CanApplyAttenuate(victim, caster, out reason)) return;            
            
            ApplyAttenuate(victim, caster);
            doOnComplete();
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return;
            if (corpse.InnerPawn == null) return;

            Pawn victim = corpse.InnerPawn;

            string reason;

            if (!CanApplyAttenuate(victim, caster, out reason)) return;

            string msg =
                $"{victim.LabelShort}'s corpse was destroyed after being attenuated.";
            System.Action sacrificeAction = () =>
            {
                ApplyAttenuate(victim, caster);
                KillUtility.CorpseDestroy(corpse);
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);               
            };

            ShowCorpseConfirmation(victim, sacrificeAction);
        }

        private bool CanApplyAttenuate(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null || caster == null) return false;
            reason = $"{victim.LabelShort} is part of the Null-Thrum.";
            
            if (OMWGenes.HasNullThrum(victim)) return false;

            if (this.selectorGenes == null)
            {
                this.selectorGenes = new SelectionAttenuateGenes(caster, victim, caster);
            }

            if (this.selectorTraits == null)
            {
                this.selectorTraits = new SelectionAttenuateTraits(caster, victim, caster);
            }

            if ((selectorGenes.genes.Count == 0) && (selectorTraits.traits.Count == 0))
            {
                reason = $"{victim.LabelShort} has no genes or traits that can be Attenuated.";
                return false;
            }

            return true;
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            return CanApplyAttenuate(victim, caster, out reason);
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

            return CanApplyAttenuate(corpse.InnerPawn, caster, out reason);            
        }
    }
}
