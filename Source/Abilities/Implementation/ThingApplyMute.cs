using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class ThingApplyMute : NullThrumAbilityPawnCorpse
    {
        PawnApplyFlatten Flatten = new PawnApplyFlatten();

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.mute;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        private float ResonanceGainPerLevel => AbilityProp.value;
        private float MaxResonanceThreshold => OMW_Mod.settings.resonanceMax;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Harvest the psychic potential of {victim.LabelShort}, stripping their psylink levels to replenish your resonance.\nGain ceases once resonance reaches {MaxResonanceThreshold}.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Mute", false) ??
                                          BaseContent.BadTex;

        public static void DoAbility(Pawn victim, Pawn caster, System.Action OnComplete)
        {
            ThingApplyMute ability = new ThingApplyMute();
            string reason;
            if (ability.CanApplyOnPawn(victim, caster, out reason))
            {
                ability.ApplyPawn(victim, caster);
            }

            OnComplete.Invoke();
        }        

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            Log.Debug($"START::Mute::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            if (victim == null || caster == null) return;

            OMWGenes.Refresh(victim);

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
                if (currentLevel <= 0)
                {                
                    OMWHediffs.RemoveHediff(victim, HediffDefOf.PsychicAmplifier);
                    break;
                }
                
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
                
                // Damage the victim's brain to represent the psychic trauma of being muted.
                int brainDamage = levelsTaken * 5; // Arbitrary damage value per level taken
                if (victim.health.hediffSet.GetBrain() != null)
                {
                    victim.TakeDamage(new DamageInfo(DamageDefOf.Cut, brainDamage, 0, -1, caster, victim.health.hediffSet.GetBrain()));
                    Log.Debug($"Applied {brainDamage} brain damage to {victim.LabelShort} due to muting.");
                }

                // Apply Genetic Dissonance to prevent repeated harvesting from the same vessel in a short time.
                OMWGenes.ApplyDissonance(victim, caster);
            }

            Log.Debug($"DONE::Mute::ApplyPawn({victim.LabelShort}, {caster.LabelShort})");
            doOnComplete();
        }

        public override void ApplyCorpse(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;
            ApplyPawn(corpse.InnerPawn, caster);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!ModsConfig.RoyaltyActive)
            {
                reason = "Missing Royalty DLC";
                return false;
            }
            if (!victim.Dead)
            {
                if (!Flatten.HasOrCanApplyOnPawn(victim, caster, out reason)) return false;
            }

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
            if (!corpse.InnerPawn.RaceProps.Humanlike)
            {
                reason = $"{corpse.InnerPawn.LabelShort} is not humanlike.";
                return false;
            }

            if (corpse.InnerPawn.health.hediffSet.GetBrain() == null)
            {
                reason = "Vessel is decapitated; the frequency cannot be anchored.";
                return false;
            }


            return CanApplyOnPawn(corpse.InnerPawn, caster, out reason);
        }
    }
}