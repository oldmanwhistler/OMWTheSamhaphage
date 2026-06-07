using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class SelectionScrub : NullThrumSelectionGene
    {
        public SelectionScrub(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) {}

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.scrub;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;


        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            return source.genes.GenesListForReading
                .Where(g => g.Overridden) // must be overridden to be scrubbed
                .ToList();            
        }
    
        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }

// ### Scrub (Harvest)

// Collect carcinomas and disabled genes as resonance.

// - Requires a scoured mind / blocked by dissonance.
// - Victim loses carcinomas and Caster gains resonance.
// - Caster can pay resonance to destroy disabled genes on Victim.
// - Applies dissonance to Victim.

    public class ThingApplyScrub : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.scrub;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public NullThrumAbilityProps AbilityPropCarcinoma => OMW_Mod.settings.abilityValue.scrubCarcinoma;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Scrub {victim.LabelShort} of their carcinomas and useless genes.\nConverts carcinomas to resonance and opens a menu to destroy deactivated genes.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Scrub");

        public bool RemoveCarcinomas(Pawn victim, Pawn caster)
        {
            Log.Debug($"Scrub::RemoveCarcinomas({victim.LabelShort}, {caster.LabelShort})");

            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in victim.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            if (carcinomas.Count == 0)
            {
                Log.Debug($"{victim.LabelShort} doesn't have any carcinomas to remove.");
                return false;
            }

            float amount = carcinomas.Count * AbilityPropCarcinoma.value;
            ResonanceUtility.Incr("from removing carcinomas", caster, amount);

            foreach (Hediff carcinoma in carcinomas)
            {
                victim.health.RemoveHediff(carcinoma);
            }

            return true;
        }

        public override void ApplyPawn(Pawn victim, Pawn caster) => ApplyPawn(victim, caster, null);

        /// <summary>
        /// Specialized ApplyPawn that allows a callback for chaining abilities.
        /// </summary>
        public void ApplyPawn(Pawn victim, Pawn caster, System.Action onAbilityComplete)
        {
            Log.Debug($"START::Scrub::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

            SelectionScrub selector = new SelectionScrub(caster, victim, victim);

            RemoveCarcinomas(victim, caster);

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            } 

            if (selector.genes.Count == 0)
            {
                Log.Debug($"DONE1::Scrub::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
                // If no genes to scrub, just gain the carcinoma resonance and finish.
                if (onAbilityComplete != null)
                {
                    onAbilityComplete?.Invoke();
                }
                else
                {
                    doOnComplete(true);
                }
                return;
            }

            Log.Debug($"Scrub::Going to open scrub for {victim.LabelShort}");

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, selectedList =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    if (plus.gene != null && victim.genes.GenesListForReading.Contains(plus.gene))
                    {
                        selector.ResonanceCredit(plus);
                        victim.genes.RemoveGene(plus.gene);
                        Log.Debug($"Destroyed {plus.gene.LabelCap} from {victim.LabelShort}");
                        activated = true;
                    }
                }
                if (activated)
                {
                    OMWGenes.Refresh(victim);
                }

                Log.Debug($"DONE2::Scrub::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
                if (onAbilityComplete == null) doOnComplete(true);
            }, onAbilityComplete));
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

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
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

            return true;
        }
    }
}
