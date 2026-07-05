using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class PawnApplyUnmute : NullThrumAbilityPawnOnly
    {
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.unmute;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        private float ResonanceCost => AbilityProp.value;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Establish a psychic resonance within {victim.LabelShort}'s mind, granting them a psylink level through the frequency of the Null-Thrum.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Unmute", false) ??
                                          BaseContent.BadTex;

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            if (ResonanceUtility.HasAvailable(caster, ResonanceCost))
            {
                // ChangePsylinkLevel adds the PsychicAmplifier hediff and increments the level
                victim.ChangePsylinkLevel(1);
                ResonanceUtility.Decr(caster, ResonanceCost);

                MoteMaker.MakeAttachedOverlay(victim, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
                Log.Debug($"Unmuted {victim.LabelShort}: Psylink granted.");
            }
            doOnComplete();
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (!ModsConfig.RoyaltyActive)
            {
                reason = "Missing Royalty DLC";
                return false;
            }

            if (victim.HasPsylink)
            {
                reason = $"{victim.LabelShort} already possesses a psylink.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {            
                reason = $"{victim.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if (!ResonanceUtility.HasAvailable(caster, ResonanceCost))
            {
                reason = $"{caster.LabelShort} does not have enough resonance (needs {ResonanceCost}).";
                return false;
            }

            return true;
        }
    }
}