using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionRetune : NullThrumSelectionGene
    {        
        public SelectionRetune(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.retune;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override Dictionary<GeneDef,string> GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            Dictionary<GeneDef,string> blocked = new Dictionary<GeneDef,string>();
            return blocked;
        }

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, Dictionary<GeneDef,string> blocked)
        {
            // Retune is moving Xenogenes to Endogenes on source
            HashSet<GeneDef> alreadyHas = source.genes.Endogenes
                .Where(g => !g.Overridden)
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.Xenogenes
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenesDontCopy.Contains(g.def) && // ignore blacklisted
                            !alreadyHas.Contains(g.def) &&
                            !this.GeneIsWorthless(g)) // ignore cosmetic genes
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }


    public class PawnApplyRetune : NullThrumAbilityPawnOnly
    {
        private PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionRetune selectorRetune;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.retune;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harmonize {victim.LabelShort}'s genetic frequency, integrating their xenogenes into their endogenic sequence.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Retune");

        public static void DoAbility(Pawn victim, Pawn caster, System.Action OnComplete)
        {
            PawnApplyRetune ability = new PawnApplyRetune();
            string reason;
            if (ability.CanApplyOnPawn(victim, caster, out reason))
            {
                // Use the callback to ensure next window only opens AFTER ability window is closed.
                ability.ApplyPawn(victim, caster, OnComplete);
            }
            else
            {
                OnComplete.Invoke();
            }
        }

        public override void ApplyPawn(Pawn victim, Pawn caster) => ApplyPawn(victim, caster, null);
        
        public void ApplyPawn(Pawn victim, Pawn caster, System.Action onAbilityComplete)
        {
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            ThingApplyScrub.DoAbility(victim, caster, () => OpenRetuneWindow(victim, caster, onAbilityComplete));
        }

        private void OpenRetuneWindow(Pawn victim, Pawn caster, System.Action onAbilityComplete)
        {
            // scrub could have changed the genes so make sure there is a reason to retune
            string reason;
            if (!CanApplyOnPawn(victim, caster, out reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                onAbilityComplete.Invoke();
                return;
            }

            // Pass null for the window close action if we are handling completion 
            // inside the selection callback to avoid double-invocation. 
            // Alternatively, only invoke completion if selection didn't occur.
            bool selectionMade = false;

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selectorRetune, () => { if(!selectionMade) onAbilityComplete?.Invoke(); }, (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene) && selectorRetune.ResonanceDebit(plus))
                    {
                        // Atomic Move: Manual list manipulation avoids the Remove/Add Harmony cascade spam
                        victim.genes.Xenogenes.Remove(plus.gene);
                        victim.genes.Endogenes.Insert(0, plus.gene);
                        Log.Debug($"Retuned {plus.gene.LabelCap} on {victim.LabelShort}");
                        activated = true;
                    }
                }

                if (activated)
                {
                    OMWGenes.ApplyDissonance(victim, caster);
                    OMWGenes.Refresh(victim);
                }

                selectionMade = true;
                onAbilityComplete?.Invoke();
            }));
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!victim.Dead)
            {
                if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
                {
                    return false;
                }
            }

            if (!ResonanceUtility.HasAvailable(caster))
            {
                reason = $"{caster.LabelShort} does not have enough available resonance.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {            
                reason = $"{victim.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            // Ensure the selector is initialized and valid
            selectorRetune = new SelectionRetune(caster, victim, victim);
            if (selectorRetune.genes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no genes that can be retuned.";
                return false;
            }

            return true;
        }
    }
}