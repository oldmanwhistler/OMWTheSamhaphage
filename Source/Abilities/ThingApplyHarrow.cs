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


        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            HashSet<GeneDef> alreadyHas = dest.genes.GenesListForReading
                .Select(g => g.def)
                .ToHashSet();
            return source.genes.GenesListForReading
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenesDontCopy.Contains(g.def) && // ignore blacklisted
                            !alreadyHas.Contains(g.def) && // ignore genes the caster already has                            
                            !this.GeneIsWorthless(g)) // ignore cosmetic genes
 
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

// ### Harrow (Theft)

// Reclaims and archives specific genes from the host.

// - Requires a scoured mind / blocked by dissonance.
// - Caster can pay resonance to take genes from Victim.
// - Applies dissonance to Victim.

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
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Harrow");
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

            selectorHarrow = new SelectionHarrow(caster, victim, caster);
            if (selectorHarrow.genes.Count == 0)
            {
                reason = $"{victim.LabelShort} has no genes that can be harrowed.";
                return false;   
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
