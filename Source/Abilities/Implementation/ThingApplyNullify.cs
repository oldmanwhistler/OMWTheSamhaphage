using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionNullify : NullThrumSelectionGene
    {
        public SelectionNullify(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.nullify;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked  GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if (source?.genes == null) return blocked;

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (OMW_BlacklistGenes.BlacklistedGenesDontRemove.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because don't remove");
                    blocked.Append(gene.def, "Don't Remove");
                    isBlocked = true;
                }
                else if (gene.def.biostatMet > 0)
                {
                    Log.Debug($"blocking {gene.def} because positive metabolism");
                    blocked.Append(gene.def, "Positive Metabolism");
                    isBlocked = true;
                }
                else if (gene.def.biostatMet == 0 && gene.def.biostatCpx <= 0 && gene.def.biostatArc <= 0)
                {
                    Log.Debug($"blocking {gene.def} because zero metabolism");
                    blocked.Append(gene.def, "Zero Metabolism");
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
            return source.genes.GenesListForReading
                .Where(g => !blocked.Has(g.def))
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }

    public class ThingApplyNullify : NullThrumAbilityPawnCorpse
    {
        private PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private SelectionNullify selectorNullify;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.nullify;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Nullify {victim.LabelShort} and destroy their genes to gain resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Nullify", false) ??
                                          BaseContent.BadTex;
        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            Log.Debug($"START::Nullify::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            ThingApplyScrub.DoAbility(victim, caster, () => OpenNullifyWindow(victim, caster));
        }

        private void OpenNullifyWindow(Pawn victim, Pawn caster)
        {
            Log.Debug($"START:Nullify::OpenNullifyWindow({victim.LabelShort}, {caster.LabelShort})");

            // previous abilities could have changed the genes so make sure things still apply
            string reason;
            if (!CanApplyOnPawn(victim, caster, out reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                doOnComplete();
                return;
            }

            bool activated = false;

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selectorNullify, onCompleteAction(), (selectedList) =>
            {
                foreach (GenePlus plus in selectedList)
                {
                    if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene))
                    {
                        selectorNullify.ResonanceCredit(plus);
                        victim.genes.RemoveGene(plus.gene);
                        Log.Debug($"Nullified {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }

                if (activated)
                {
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

            if (selectorNullify == null)
            {
                selectorNullify = new SelectionNullify(caster, victim, victim);
                if (selectorNullify.genes.Count == 0)
                {
                    reason = $"{victim.LabelShort} has no genes that can be nullified.";
                    return false;
                }
            }

            return true;
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
