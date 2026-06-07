using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionExcise : NullThrumSelectionTrait
    {
        public SelectionExcise(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.excise;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest)
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

    public class ThingApplyExcise : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.excise;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Excise a trait from {victim.LabelShort} and convert its frequency into resonance.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Excise");

        public bool ApplyExcise(Pawn victim, Pawn caster, SelectionExcise selector, List<TraitPlus> selectedList)
        {
            bool activated = false;
            foreach (TraitPlus plus in selectedList)
            {
                if (plus.trait != null && (victim.story?.traits?.allTraits.Contains(plus.trait) ?? false))
                {
                    // Gain resonance for removing the trait (like Nullify)
                    selector.ResonanceCredit(plus);
                    victim.story?.traits?.RemoveTrait(plus.trait);
                    Log.Debug($"Excised {plus.trait.LabelCap} from {victim.LabelShort}");
                    activated = true;
                }
            }
            return activated;
        }

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            SelectionExcise selector = new SelectionExcise(caster, victim, caster);
            if (selector.traits.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no traits that can be excised.", MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selector, (selectedList) =>
            {
                bool activated = false;
                if (ApplyExcise(victim, caster, selector, selectedList))
                {
                    OMWGenes.ApplyDissonance(victim, caster);
                    OMWGenes.Refresh(victim);
                    activated = true;
                }
                doOnComplete(activated);                
            }));
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;

            Pawn victim = corpse.InnerPawn;
            SelectionExcise selector = new SelectionExcise(caster, victim, caster);
            if (selector.traits.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no traits that can be excised.", MessageTypeDefOf.RejectInput);
                return;
            }

            string msg = $"{victim.LabelShort}'s corpse was destroyed after being excised for their traits and and attenuated for their genes.";
            System.Action sacrificeAction = () =>
            {
                Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selector, (selectedList) =>
                {
                    if (ApplyExcise(victim, caster, selector, selectedList))
                    {
                        // only attenuate corpses
                        ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
                        SelectionAttenuate selectorAttenuate = attenuate.CanApplyAttenuate(victim, caster);
                        if (selectorAttenuate != null)
                        {
                            attenuate.ApplyAttenuate(victim, caster, selectorAttenuate);
                        }                        
                        KillUtility.CorpseDestroy(corpse);
                        Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                    }
                    // Needs to be false so doesn't get stuck on a loop
                    doOnComplete(false);                    
                }));
            };

            ShowCorpseConfirmation(victim, sacrificeAction);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason)) return false;
            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }
            return true;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown";
            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }
            return true;
        }
    }
}