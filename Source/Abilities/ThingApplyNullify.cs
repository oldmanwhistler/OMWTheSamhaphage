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


        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest)
        {
            return source.genes.GenesListForReading
                .Where(g => g.def.biostatMet < 0) // genes have to be "valuable"
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            return new List<GeneDef>();
        }        
    }

    public class ThingApplyNullify : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.nullify;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Nullify {victim.LabelShort} and destroy their genes to gain resonance.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Nullify");
        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            Log.Debug($"START::Nullify::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return false;

            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            ThingApplyScrub scrub = new ThingApplyScrub();
            // Chain Nullify window to open after Scrub is done
            return scrub.ApplyPawn(victim, caster, () => OpenNullifyWindow(victim, caster));
        }

        private void OpenNullifyWindow(Pawn victim, Pawn caster)
        {
            Log.Debug($"START:Nullify::OpenNullifyWindow({victim.LabelShort}, {caster.LabelShort})");
            SelectionNullify selector = new SelectionNullify(caster, victim, caster);
            if (selector.genes.Count == 0)
            {
                Messages.Message($"{victim.LabelShort} has no genes that can be Nullifyed.", MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selector, (selectedList) =>
            {
                bool activated = false;
                foreach (GenePlus plus in selectedList)
                {
                    selector.ResonanceCredit(plus);
                    victim.genes.RemoveGene(plus.gene);
                    Log.Debug($"Nullified {plus.gene.LabelCap} from {victim.LabelShort}");
                    activated = true;
                }

                if (activated)
                {
                    // Retune the caster after harvesting to integrate new genes
                    PawnApplyRetune retune = new PawnApplyRetune();
                    retune.ApplyPawn(caster, caster);
                    OMWGenes.ApplyDissonance(victim, caster);
                }
            }));
            Log.Debug($"DONE::Nullify::OpenNullifyWindow({victim.LabelShort}, {caster.LabelShort})");
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse == null || caster == null) return false;
            if (corpse.InnerPawn == null) return false;
            return ApplyPawn(corpse.InnerPawn, caster);
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason))
            {
                return false;
            }

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
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

            if (!ResonanceUtility.HasGene(caster))
            {
                reason = $"{caster.LabelShort} does not have a supply of resonance.";
                return false;
            }

            return true;
        }
    }
}
