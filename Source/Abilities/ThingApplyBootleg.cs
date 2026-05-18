using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionBootleg : NullThrumSelectionTrait
    {
        public SelectionBootleg(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override string Name => "Bootleg";

        // Traits are spiritually heavy; stealing them requires significant resonance.
        protected override float ResonanceTotalMultiplier => 10.0f;
        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeDebit;

        protected override List<Trait> TraitsToSelectFrom(Pawn source, Pawn dest)
        {
            if (source?.story?.traits == null || dest?.story?.traits == null)
                return new List<Trait>();

            HashSet<TraitDef> alreadyHas = dest.story.traits.allTraits
                .Select(t => t.def)
                .ToHashSet();

            return source.story.traits.allTraits
                .Where(t => !alreadyHas.Contains(t.def))
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
        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Bootleg;

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
                if (selector.ResonanceDebit(plus))
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

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            SelectionBootleg selectorBootleg = CanApplyBootleg(victim, caster);
            if (selectorBootleg == null) return false;

            ThingApplyAttenuate attenuate = new ThingApplyAttenuate();            
            SelectionAttenuate selectorAttenuate = attenuate.CanApplyAttenuate(victim, caster);

            bool value = false;
            string msg = $"{victim.LabelShort} has died while being bootlegged.";

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

            OMW_UIHelpers.ShowLethalConfirmation(victim, sacrificeAction);
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
            return CanApplyOnPawn(corpse?.InnerPawn, caster, out reason);
        }
    }
}