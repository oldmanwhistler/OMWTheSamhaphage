using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class ThingApplyMute : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();
        private float ResonanceGainPerLevel => OMW_Mod.settings.gainMute;
        private float MaxResonanceThreshold => OMW_Mod.settings.resonanceMax;

        public override NullThrumAbilityType AbilityType => NullThrumAbilityType.Mute;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harvest the psychic potential of {victim.LabelShort}, stripping their psylink levels to replenish your resonance.\nGain ceases once resonance reaches {MaxResonanceThreshold}.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Mute");

        public override bool ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            // Harvest abilities require the mind to be scoured first to remove identity interference.
            if (!OMWGenes.HasScouredMind(victim))
            {
                Flatten.ApplyPawn(victim, caster);
            }

            int levelsTaken = 0;
            // Drain psylink levels one by one until the victim is empty or the caster is saturated.
            while (victim.HasPsylink && ResonanceUtility.Total(caster) < MaxResonanceThreshold)
            {
                int currentLevel = victim.GetPsylinkLevel();
                
                // ChangePsylinkLevel(-1) reduces the level and handles hediff removal if it reaches 0.
                victim.ChangePsylinkLevel(-1);
                ResonanceUtility.Incr("from muting psychic frequency", caster, ResonanceGainPerLevel);
                levelsTaken++;

                // Failsafe to prevent infinite loops if psylink removal is blocked by another mod.
                if (victim.GetPsylinkLevel() >= currentLevel) break;
            }

            if (levelsTaken > 0)
            {
                MoteMaker.MakeAttachedOverlay(victim, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
                Log.Debug($"Muted {victim.LabelShort}: {levelsTaken} psylink levels harvested.");
                
                // Apply Genetic Dissonance to prevent repeated harvesting from the same vessel in a short time.
                Hediff hediffDissonance = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_GeneticDissonance, caster);
                victim.health.AddHediff(hediffDissonance);
                
                return true;
            }

            return false;
        }

        public override bool ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return false;
            return ApplyPawn(corpse.InnerPawn, caster);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason)) return false;

            if (!victim.HasPsylink)
            {
                reason = $"{victim.LabelShort} has no psychic frequency to mute.";
                return false;
            }

            if (ResonanceUtility.Total(caster) >= MaxResonanceThreshold)
            {
                reason = $"{caster.LabelShort} is already saturated with resonance (Limit: {MaxResonanceThreshold}).";
                return false;
            }

            return true;
        }

        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (corpse == null) return false;
            if (corpse?.InnerPawn == null) return false;
            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}