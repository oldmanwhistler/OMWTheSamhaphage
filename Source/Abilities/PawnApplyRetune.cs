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

        public override string Name => "Retune";

        // More expensive because it is stealing genes
        protected override float ResonanceTotalMultiplier => 0.5f;

        protected override NullThrumResonanceType ResonanceType => NullThrumResonanceType.ResonanceTypeDebit;

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            // Retune is moving Xenogenes to Endogenes on source
            HashSet<GeneDef> alreadyHas = source.genes.Endogenes
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.Xenogenes
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenesResonance.Contains(g.def) && // ignore blacklisted
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
        PawnApplyFlatten Flatten = new PawnApplyFlatten();

        public override string AbilityName => "Retune";

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harmonize {victim.LabelShort}'s genetic frequency, integrating their xenogenes into their endogenic sequence.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Retune");

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            ThingApplyScrub scrub = new ThingApplyScrub();
            
            // Use the callback to ensure Retune window only opens AFTER Scrub window is closed.
            return scrub.ApplyPawn(victim, caster, () => OpenRetuneWindow(victim, caster));
        }

        private void OpenRetuneWindow(Pawn victim, Pawn caster)
        {
            SelectionRetune selector = new SelectionRetune(caster, victim, victim);

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be retuned.", MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (selector.ResonanceDebit(plus))
                    {
                        victim.genes.RemoveGene(plus.gene);
                        victim.genes.AddGene(plus.gene.def, false);
                        Log.Debug($"Retuned {plus.gene.LabelCap} on {victim.LabelShort}");
                        activated = true;
                    }
                }

                if (activated)
                {
                    selector.ApplyDissonance(victim, caster);
                }
            }));
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
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

            return true;
        }
    }
}