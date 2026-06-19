using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionSample : NullThrumSelectionGene
    {
        public SelectionSample(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest)
        {
            
        }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.sample;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;


        protected override NullThrumSelectionGeneBlocked  GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if (source?.genes == null || dest?.genes == null) return blocked;

            HashSet<GeneDef> alreadyHas = dest.genes.GenesListForReading
                .Where(g => !g.Overridden)
                .Select(g => g.def)
                .ToHashSet();

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (alreadyHas.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because already possessed");
                    blocked.Append(gene.def, "Already have");
                    isBlocked = true;
                }
                else if (gene.Overridden)
                {
                    Log.Debug($"blocking {gene.def} because overridden");
                    blocked.Append(gene.def, "Overridden");
                    isBlocked = true;
                }
                else if (!GeneIsCosmetic(gene))
                {
                    Log.Debug($"blocking {gene.def} because not cosmetic");
                    blocked.Append(gene.def, "Not Cosmetic");
                    isBlocked = true;
                }
                if (!isBlocked)
                {
                    Log.Debug($"not blocking {gene.def}: {gene.Label}");
                }
            }
            return blocked;
        }

        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionGeneBlocked blocked)
        {
            return source.genes.GenesListForReading.Where(g => !blocked.Has(g.def)).ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null) return new List<GeneDef>();
            return dest.genes.GenesListForReading.Select(g => g.def).ToList();
        }
    }

    public class ThingApplySample : NullThrumAbilityPawnCorpse
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.sample;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Sample {victim.LabelShort} and steal their appearance to disguise yourself as one of their kind.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Sample", false) ??
                                          BaseContent.BadTex;
        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            SelectionSample selector = new SelectionSample(caster, victim, caster);

            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be stolen.",
                    MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, onCompleteAction(), (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene))
                    {
                        selector.ResonanceCredit(plus);
                        victim.genes.RemoveGene(plus.gene);
                        caster.genes.AddGene(plus.gene.def, true);
                        Log.Debug($"Stole gene {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }

                if (activated)
                {
                    OMWGenes.Refresh(caster);
                    // Retune the caster after harvesting to integrate new genes
                    PawnApplyRetune.DoAbility(caster, caster, onCompleteAction());
                }
                else
                {
                    doOnComplete();
                }                
            }));
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return;
            if (corpse.InnerPawn == null) return;

            ApplyPawn(corpse.InnerPawn, caster);
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

            if (ResonanceUtility.Total(caster) < 1)
            {
                reason = $"Not enough Resonance to sample {p.LabelShort} and copy their appearance.";
                return false;
            }

            return CanApplyLimitMetabolism(caster, out reason);
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

            if (ResonanceUtility.Total(caster) < 1)
            {
                reason = "Not enough Resonance to steal a face.";
                return false;
            }

            return true;
        }
    }
}
