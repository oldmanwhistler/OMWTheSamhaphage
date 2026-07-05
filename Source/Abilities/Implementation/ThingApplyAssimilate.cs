using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionAssimilate : NullThrumSelectionGene
    {
        public SelectionAssimilate(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.assimilate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        protected override NullThrumSelectionGeneBlocked GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if ((source == null) || (source.genes == null)) return blocked;

            HashSet<GeneDef> alreadyHas = dest.genes?.GenesListForReading.Select(g => g.def).ToHashSet() ?? new HashSet<GeneDef>();

            List<GeneDef> sourceXenotype = source.genes?.Xenotype?.genes;
            if (sourceXenotype == null) sourceXenotype = new List<GeneDef>();

            foreach (Gene gene in source.genes?.GenesListForReading)
            {
                bool isBlocked = false;
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
                if (!isBlocked && sourceXenotype.Contains(gene.def))
                {
                    blocked.Append(gene.def, "Part of Xenotype");
                }
                if (!isBlocked && alreadyHas.Contains(gene.def))
                {
                    foreach (Gene geneDest in dest.genes?.GenesListForReading.Where(g => g.def == gene.def))
                    {
                        if (!geneDest.Overridden)
                        {
                            Log.Debug($"blocking {gene.def} because already has it as an active gene");
                            blocked.Append(gene.def, "Already Has");
                            isBlocked = true;
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

    public class ThingApplyAssimilate : NullThrumAbilityPawnCorpse
    {
        private SelectionAssimilate selectorAssimilate;

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.assimilate;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Assimilate {victim.LabelShort} and harvest their genes using resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Assimilate", false) ??
                                          BaseContent.BadTex;
        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            ShowCorpseConfirmation(victim, onDoApplyPawn(caster, victim));            
        }

        private System.Action onDoApplyPawn(Pawn caster, Pawn victim)
        {
            return () => DoApplyPawn(caster, victim);
        }

        private void DoApplyPawn(Pawn caster, Pawn victim)
        {
            Log.Debug($"{AbilityName} starting");
            OMWGenes.Refresh(victim);
            selectorAssimilate = new SelectionAssimilate(caster, victim, caster);
            if (selectorAssimilate.genes.Count == 0)
            {
                Log.Debug($"{AbilityName}:: {victim} does not have any genes that can be assimilated so just eat them.");

                FinishAssimilate(caster, victim);
                return;
            }
            
            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selectorAssimilate, onFinishAssimilateAction(caster, victim),
                (selectedList) =>
                {
                    foreach (GenePlus plus in selectedList)
                    {
                        if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene) &&
                            selectorAssimilate.ResonanceDebit(plus))
                        {
                            Log.Debug($"START: Assimilating by removing {plus.gene.LabelCap} from {victim.LabelShort}");
                            victim.genes.RemoveGene(plus.gene);
                            Log.Debug($"START: Assimilating by adding {plus.gene.LabelCap} to {caster.LabelShort}");
                            caster.genes.AddGene(plus.gene.def, true);
                            Log.Debug($"DONE: Assimilating {plus.gene.LabelCap}");
                        }
                        else
                        {
                            Log.Debug(
                                $"SKIPPING: Assimilating {plus.gene.LabelCap} from {victim.LabelShort} cuz {caster.LabelShort} doesn't have enough resonance.");
                        }
                    }

                    FinishAssimilate(caster, victim);
                }));            
        }

        private System.Action onFinishAssimilateAction(Pawn caster, Pawn victim)
        {
            return () => FinishAssimilate(caster, victim);
        }
        private void FinishAssimilate(Pawn caster, Pawn victim)
        {
            OMWGenes.Refresh(caster);
            string msg = $"{victim.LabelShort} was eaten by a groo.";
            ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
            attenuate.ApplyPawn(victim, caster);
            ThingApplyMute mute = new ThingApplyMute();
            mute.ApplyPawn(victim, caster);
            victim.Strip();
            KillUtility.PurgeBionics(victim);
            KillUtility.PawnKillDestroy(victim, caster);
            Messages.Message(msg, MessageTypeDefOf.NegativeEvent);
            ApplyHediff(caster);
            doOnComplete();
        }

        private void ApplyHediff(Pawn caster)
        {
            if (!caster.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_Assimilator))
            {
                Log.Debug(
                    $"{AbilityName}:: {caster} is getting the assimilator hediff.");

                caster.health.AddHediff(OMW_HediffDefOf.OMW_Assimilator);
            }
            Log.Debug(
                $"{AbilityName}:: {caster} is updating the eaten slug counter.");
            Hediff hediff = caster.health.hediffSet.GetFirstHediffOfDef(OMW_HediffDefOf.OMW_Assimilator);
            if (hediff == null)
            {
                Log.Error(
                    $"{AbilityName}: {caster.LabelShort} does not have the assimilator hediff. This should not be possible.");
                return;
            }
            HediffComp_Assimilator comp = hediff.TryGetComp<HediffComp_Assimilator>();
            comp.EatSlug();
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

            if ((victim.genes?.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (victim.genes?.Xenotype !=
                    OMW_XenotypeDefOf.omw_fluxspawn_brute) && (victim.genes?.Xenotype !=
                                                               OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                reason = $"{victim.LabelShort} is not a fluxspawn.";
                return false;
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
