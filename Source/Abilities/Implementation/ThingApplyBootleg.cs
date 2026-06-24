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

        protected override NullThrumSelectionTraitBlocked TraitsBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionTraitBlocked blocked = new();
            if (source?.story?.traits == null || dest?.story?.traits == null)
                return blocked;

            HashSet<TraitDef> alreadyHas = dest.story.traits.allTraits.Select(t => t.def).ToHashSet();

            foreach (Trait trait in source.story.traits.allTraits)
            {
                bool isBlocked = false;
                if (trait.sourceGene != null)
                {
                    blocked.Append(trait.def, "From Gene");
                    isBlocked = true;
                }

                if (OMW_BlacklistTraits.BlacklistedTraitsDontCopy.Contains(trait.def))
                {
                    blocked.Append(trait.def, "Don't Copy");
                    isBlocked = true;
                }

                if (alreadyHas.Contains(trait.def))
                {
                    blocked.Append(trait.def, "Already Has");
                    isBlocked = true;
                }

                foreach (TraitDef traitDef in trait.def.conflictingTraits)
                {
                    if (alreadyHas.Contains(traitDef))
                    {
                        blocked.Append(trait.def, $"Conflict {traitDef.defName}");
                        isBlocked = true;
                        break;
                    }
                }

                foreach (WorkTypeDef workType in trait.def.requiredWorkTypes)
                {
                    if (dest.WorkTypeIsDisabled(workType))
                    {
                        blocked.Append(trait.def, $"Requires {workType.label}");
                        isBlocked = true;
                        break;
                    }
                }

                foreach (SkillDef skill in trait.def.conflictingPassions)
                {
                    if (dest.skills.skills.Any(s => s.def == skill))
                    {
                        blocked.Append(trait.def, $"Conflict {skill.label}");
                        isBlocked = true;
                        break;
                    }
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
            if (dest?.story?.traits == null)
                return new List<TraitDef>();

            return dest.story.traits.allTraits.Select(t => t.def).ToList();
        }
    }

    public class ThingApplyBootleg : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionBootleg selectorBootleg;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.bootleg;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override bool IsLethal => true;
        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Bootleg {victim.LabelShort}'s personality traits, stripping them of their identity to bolster your own.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Bootleg", false) ??
                                          BaseContent.BadTex;

        private bool CanApplyBootleg(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;
            if (selectorBootleg == null) selectorBootleg = new SelectionBootleg(caster, victim, caster);
            if (selectorBootleg.traits.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no traits that can be bootlegged.", MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }

        private bool ApplyBootleg(Pawn victim, Pawn caster, SelectionBootleg selector, List<TraitPlus> selectedList)
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
                    caster.story.traits.GainTrait(plus.Copy(), suppressConflicts: false);
                    Log.Debug($"Bootlegged {plus.trait.LabelCap} from {victim.LabelShort}");
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

            if (!CanApplyBootleg(victim, caster)) return;

            Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selectorBootleg, onCompleteAction(), (selectedList) =>
            {
                ApplyBootleg(victim, caster, selectorBootleg, selectedList);
                doOnComplete();
            }));
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;

            Pawn victim = corpse.InnerPawn;
            if (!CanApplyBootleg(victim, caster)) return;

            string msg = $"{victim.LabelShort}'s corpse was destroyed after being bootlegged for their traits and attenuated for their genes.";
            System.Action sacrificeAction = () =>
            {
                Find.WindowStack.Add(new WindowSelectTraitsForNullThrumAbility(selectorBootleg, onCompleteAction(), (selectedList) =>
                {
                    if (ApplyBootleg(victim, caster, selectorBootleg, selectedList))
                    {
                        // only attenuate corpses
                        KillUtility.ApplyRenderOrAttenuate(victim, caster);
                        KillUtility.CorpseDestroy(corpse);
                        Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
                        
                    }
                }));
            };

            ShowCorpseConfirmation(victim, sacrificeAction);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (victim == null || caster == null)
            {
                reason = "Invalid pawn.";
                return false;
            }

            if (!victim.Dead)
            {
                if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason)) return false;
            }

            int casterTraitCount = TraitPlusUtility.CountTraits(caster);
            int traitLimit = OMW_Mod.settings.limitTraits.GetLimit(caster.genes?.Xenotype);
            if (casterTraitCount >= traitLimit)
            {
                reason = $"{caster.LabelShort} has {casterTraitCount} traits, reaching the limit of {traitLimit} in settings.";
                return false;
            }

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }


            if (!CanApplyBootleg(victim, caster))
            {
                reason = $"{victim.LabelShort} does not have any traits to bootleg.";
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