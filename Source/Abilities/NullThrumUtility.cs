using System;
using Verse;

namespace OMW_Samhaphage
{
    public enum NullThrumAbilityType
    {
        Flatten,
        Scrub,
        Retune,
        Harrow,
        Transpose,
        Infest,
        Enwomb,
        Unmute,
        Mute,
        Compress,
        Attenuate,
        Sample,
        Bootleg,
        Crosstalk,
        Resurrect,
        Stun,
        Hallowbound
    }

    public static class NullThrumUtility
    {
        /// <summary>
        /// Returns an out-of-character technical description of what the ability does mechanically.
        /// </summary>

        public static string ToString(NullThrumAbilityType ability)
        {
            switch (ability)
            {
                case NullThrumAbilityType.Flatten: return "Flatten";
                case NullThrumAbilityType.Scrub: return "Scrub";
                case NullThrumAbilityType.Retune: return "Retune";
                case NullThrumAbilityType.Harrow: return "Harrow";
                case NullThrumAbilityType.Transpose: return "Transpose";
                case NullThrumAbilityType.Infest: return "Infest";
                case NullThrumAbilityType.Enwomb: return "Enwomb";
                case NullThrumAbilityType.Unmute: return "Unmute";
                case NullThrumAbilityType.Mute: return "Mute";
                case NullThrumAbilityType.Compress: return "Compress";
                case NullThrumAbilityType.Attenuate: return "Attenuate";
                case NullThrumAbilityType.Sample: return "Sample";
                case NullThrumAbilityType.Bootleg: return "Bootleg";
                case NullThrumAbilityType.Crosstalk: return "Crosstalk";
                case NullThrumAbilityType.Resurrect: return "Resurrect";
                case NullThrumAbilityType.Stun: return "Stun";
                case NullThrumAbilityType.Hallowbound: return "Hallowbound";
                default: return "Unknown";
            }
        }         
        public static string DescriptionOOC(NullThrumAbilityType ability, string caster = "CASTER", string victim = "VICTIM")
        {
            switch (ability)
            {
                case NullThrumAbilityType.Flatten:
                    return $"{victim} loses all negative memories, becomes a Psychopath, and is infected with a hediff that reduces Will and Resistance.";
                case NullThrumAbilityType.Scrub:
                    return $"{caster} gains resonance from consuming {victim}'s carcinomas. {caster} may selectively destroys overridden (disabled) genes to gain resonance.";
                case NullThrumAbilityType.Retune:
                    return $"{caster} spends resonance to integrate selected xenogenes into their permanent endogenic sequence.";
                case NullThrumAbilityType.Harrow:
                    return $"{caster} Selectively extracts active genes from a victim and adds them to {caster}'s own genome.";
                case NullThrumAbilityType.Transpose:
                    return $"{caster} forcefully changes the {victim}'s current xenotype to the xenotype specified by the ability.";
                case NullThrumAbilityType.Infest:
                    return $"{caster} infests {victim} with parasitic embryos that lethally birth a litter of new Fluxspawn. Lethal to {victim}.";
                case NullThrumAbilityType.Enwomb:
                    return $"{caster} transforms {victim} into a Cradlemold factory. Lethal to {victim}, turning them into a permanent population engine for the brood.";
                case NullThrumAbilityType.Unmute:
                    return $"{caster} uses resonance to grant {victim} a psylink level.";
                case NullThrumAbilityType.Mute:
                    return $"{caster} removes {victim}'s psylink level to gain resonance.";
                case NullThrumAbilityType.Compress:
                    return $"{caster} integrates {victim}'s xenogenes into {victim}'s endogenes.";
                case NullThrumAbilityType.Attenuate:
                    return $"{caster} lethally dissolves a pawn or corpse to gain resonance. Lethal to {victim}.";
                case NullThrumAbilityType.Sample:
                    return $"{caster} steals cosmetic/appearance genes from {victim}.";
                case NullThrumAbilityType.Bootleg:
                    return $"{caster} steals selected traits from {victim}. Lethal to {victim}.";
                case NullThrumAbilityType.Crosstalk:
                    return $"{caster} swaps random xenogenes with {victim}.";
                case NullThrumAbilityType.Resurrect:
                    return $"{caster} resurrects a corpse as a specific xenotype. Lethal if {caster} is a Fluxspawn.";
                case NullThrumAbilityType.Stun:
                    return $"Stuns the target.";
                case NullThrumAbilityType.Hallowbound:
                    return $"Transposes the target into a Hallowbound.";
                default:
                    return "Unknown ability type.";
            }
        }
    }
}