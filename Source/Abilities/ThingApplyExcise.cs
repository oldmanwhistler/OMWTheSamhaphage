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

        protected override NullThrumSelectionTraitBlocked TraitsBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionTraitBlocked blocked = new();

            if (source?.story?.traits == null || dest?.story?.traits == null)
                return blocked;
            
            foreach (Trait trait in source.story.traits.allTraits)
            {
                bool isBlocked = false;
                if (trait.sourceGene != null)
                {
                    blocked.Append(trait.def, "From Gene");
                    isBlocked = true;
                }

                if (OMW_BlacklistTraits.BlacklistedTraitsDontRemove.Contains(trait.def))
                {
                    blocked.Append(trait.def, "Don't Remove");
                    isBlocked = true;
                }

                if (!isBlocked)
                {
                    Log.Debug($"not blocking {trait.def}");
                }
            }
            return blocked;
        }

        protected override List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionTraitBlocked blocked)
        {
            return source?.story?.traits?.allTraits.Where(t => !blocked.Has(t.def)).ToList() ?? new List<Trait>();
        }

        protected override List<TraitDef> ConflictTraitDefs(Pawn source, Pawn dest)
        {
            return new List<TraitDef>();
        }
    }

    public class ThingApplyExcise : NullThrumAbilityPawnCorpse
    {
        private PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionExcise selectorExcise;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.excise;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Excise a trait from {victim.LabelShort} and convert its frequency into resonance.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Excise");
        public override bool IsLethal => true;
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

            Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selectorExcise, onCompleteAction(), (selectedList) =>
            {
                if (ApplyExcise(victim, caster, selectorExcise, selectedList))
                {
                    OMWGenes.ApplyDissonance(victim, caster);
                    OMWGenes.Refresh(victim);
                }
                doOnComplete();                
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
                Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selector, onCompleteAction(), (selectedList) =>
                {
                    if (ApplyExcise(victim, caster, selector, selectedList))
                    {
                        KillUtility.ApplyRenderOrAttenuate(victim, caster);
                        KillUtility.CorpseDestroy(corpse);
                        Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                    }        
                    // don't go back to OpenWindow cuz it's a sacrifice
                }));
            };

            ShowCorpseConfirmation(victim, sacrificeAction);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown";
            if (!victim.Dead)
            {
                if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason)) return false;
            }

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            selectorExcise = new SelectionExcise(caster, victim, caster);
            if (selectorExcise.traits.Count == 0)
            {
                reason = $"{victim.LabelShort} has no traits that can be excised.";
                return false;
            }

            return true;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown";
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

            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}