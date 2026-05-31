using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionBootleg : NullThrumSelectionTrait
    {
        public SelectionBootleg(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) {}

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.bootleg;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest)
        {
            if (source?.story?.traits == null || dest?.story?.traits == null)
                return new List<Trait>();

            HashSet<TraitDef> alreadyHas = dest.story.traits.allTraits
                .Select(t => t.def)
                .ToHashSet();

            HashSet<TraitDef> conflicts = new HashSet<TraitDef>();

            foreach (Trait trait in source.story.traits.allTraits)
            {
                foreach (TraitDef traitDef in trait.def.conflictingTraits)
                {
                    conflicts.Add(traitDef);
                }
            }

            // No traits they have
            // No traits that conflict with traits they have
            // No traits from genes
            return source.story.traits.allTraits
                .Where(t => !alreadyHas.Contains(t.def) &&
                            !conflicts.Contains(t.def) &&
                            t.sourceGene == null
                            )
                .ToList();
        }

        protected override List<TraitDef> ConflictTraitDefs(Pawn source, Pawn dest)
        {
            if (dest?.story?.traits == null)
                return new List<TraitDef>();

            return dest.story.traits.allTraits.Select(t => t.def).ToList();
        }
    }

    public class ThingApplyBootleg : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.bootleg;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Bootleg {victim.LabelShort}'s personality traits, stripping them of their identity to bolster your own.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Bootleg");

        public SelectionBootleg CanApplyBootleg(Pawn victim, Pawn caster)
        {
            SelectionBootleg selector = new SelectionBootleg(caster, victim, caster);
            if (selector.traits.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no traits that can be bootlegged.", MessageTypeDefOf.RejectInput);
                return null;
            }
            return selector;
        }

        public bool ApplyBootleg(Pawn victim, Pawn caster, SelectionBootleg selector, List<TraitPlus> selectedList)
        {
            bool activated = false;
            foreach (TraitPlus plus in selectedList)
            {
                // half-price sale if the victim is alive.
                if (!victim.Dead) plus.value = plus.value / 2f;
                if (plus.trait != null && (victim.story?.traits?.allTraits.Contains(plus.trait) ?? false) && selector.ResonanceDebit(plus))
                {
                    victim.story?.traits?.RemoveTrait(plus.trait);
                    // Create a new trait instance for the caster to avoid reference bugs
                    caster.story.traits.GainTrait(plus.Copy(), suppressConflicts: true);
                    Log.Debug($"Bootlegged {plus.trait.LabelCap} from {victim.LabelShort}");
                    activated = true;
                }
            }
            return activated;
        }

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            SelectionBootleg selectorBootleg = CanApplyBootleg(victim, caster);
            if (selectorBootleg == null) return false;

            bool value = false;

            Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selectorBootleg, (selectedList) =>
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    if (ApplyBootleg(victim, caster, selectorBootleg, selectedList))
                    {
                        // State updates handled inside ApplyBootleg
                    }
                });
            }));

            return value;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return false;

            Pawn victim = corpse.InnerPawn;
            SelectionBootleg selectorBootleg = CanApplyBootleg(victim, caster);
            if (selectorBootleg == null) return false;

            ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
            SelectionAttenuate selectorAttenuate = attenuate.CanApplyAttenuate(victim, caster);            

            bool value = false;
            string msg = $"{victim.LabelShort}'s corpse was destroyed after being bootlegged for their personality.";
            System.Action sacrificeAction = () =>
            {
                Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selectorBootleg, (selectedList) =>
                {
                    if (ApplyBootleg(victim, caster, selectorBootleg, selectedList))
                    {
                        if (selectorAttenuate != null)
                        {
                            attenuate.ApplyAttenuate(victim, caster, selectorAttenuate);
                        }
                        OMWAnomaly.PawnToShamblerOrKillDestroy(victim, caster);
                        Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                        value = true;                        
                    }
                }));
            };

            OMW_UIHelpers.ShowCorpseConfirmation(victim, sacrificeAction);
            return value;
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