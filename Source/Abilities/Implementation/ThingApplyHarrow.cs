using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionHarrow : NullThrumSelectionGene
    {
        public SelectionHarrow(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.harrow;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if ((source == null) || (source.genes == null)) return blocked;

            HashSet<GeneDef> alreadyHas = dest.genes?.GenesListForReading.Select(g => g.def).ToHashSet() ?? new HashSet<GeneDef>();

            HashSet<GeneDef> sourceActive = source.genes?.GenesListForReading.Where(g => !g.Overridden).Select(g => g.def).ToHashSet() ?? new HashSet<GeneDef>();

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (!isBlocked && gene.Overridden)
                {
                    // this gene is active
                    if (sourceActive.Contains(gene.def))
                    {
                        // if there is active and overridden, then we can harrow the active one
                    }
                    else
                    {
                        Log.Debug($"blocking {gene.def} because overridden");
                        blocked.Append(gene.def, "Overridden");
                        isBlocked = true;
                    }
                }
                if (!isBlocked && OMW_BlacklistGenes.BlacklistedGenesDontCopy.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because don't copy");
                    blocked.Append(gene.def, "Don't Copy");
                    isBlocked = true;
                }

                if (!isBlocked && GeneIsCosmetic(gene))
                {
                    Log.Debug($"blocking {gene.def} because cosmetic");
                    blocked.Append(gene.def, "Cosmetic");
                    isBlocked = true;
                }

                if (!isBlocked && alreadyHas.Contains(gene.def))
                {
                    foreach (Gene geneDest in dest.genes?.GenesListForReading.Where(g => g.def == gene.def))
                    {
                        if (!geneDest.Overridden)
                        {
                            Log.Debug($"blocking {gene.def} because already has it as an active gene");
                            blocked.Append(gene.def, "Already Has");
                        }
                    }
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
            return source.genes.GenesListForReading
                .Where(g => !blocked.Has(g.def) && !g.Overridden)
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null)
                return new List<GeneDef>();

            return dest.genes.GenesListForReading.Where(g => !g.Overridden)
                .Select(g => g.def).ToList();
        }        
    }

    public class ThingApplyHarrow : NullThrumAbilityPawnCorpse
    {
        private PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionHarrow selectorHarrow;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.harrow;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harrow {victim.LabelShort} and harvest their genes using resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Harrow", false) ??
                                          BaseContent.BadTex;
        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            Log.Debug($"START::Harrow::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            ThingApplyScrub.DoAbility(victim, caster, () => OpenHarrowWindow(victim, caster));
        }

        private void OpenHarrowWindow(Pawn victim, Pawn caster)
        {
            Log.Debug($"START:Harrow::OpenHarrowWindow({victim.LabelShort}, {caster.LabelShort})");
            // previous abilities could have changed the genes so make sure things still apply
            string reason;
            if (!CanApplyOnPawn(victim, caster, out reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                doOnComplete();
                return;
            }
            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selectorHarrow, onCompleteAction(),(selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene) && selectorHarrow.ResonanceDebit(plus))
                    {
                        Log.Debug($"START: Harrowing by removing {plus.gene.LabelCap} from {victim.LabelShort}");
                        victim.genes.RemoveGene(plus.gene);
                        Log.Debug($"START: Harrowing by adding {plus.gene.LabelCap} to {caster.LabelShort}");
                        caster.genes.AddGene(plus.gene.def, true);
                        Log.Debug($"DONE: Harrowing {plus.gene.LabelCap}");
                        activated = true;
                    }
                    else
                    {
                        Log.Debug($"SKIPPING: Harrowing {plus.gene.LabelCap} from {victim.LabelShort} cuz {caster.LabelShort} doesn't have enough resonance.");
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

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            if (ResonanceUtility.Total(caster) <= 2)
            {
                reason = $"{caster.LabelShort} does not have enough resonance to {this.AbilityName}.";
                return false;
            }

            if (selectorHarrow == null) selectorHarrow = new SelectionHarrow(caster, victim, caster);
            if (selectorHarrow.genes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no genes that can be harrowed.";
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

            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}
