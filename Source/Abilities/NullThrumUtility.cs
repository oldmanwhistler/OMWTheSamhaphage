using System;
using Verse;

namespace OMW_Samhaphage
{
    public enum NullThrumResonanceType
    {
        // Selectors classes
        ResonanceTypeCredit,
        ResonanceTypeDebit,
        // Not a selector class
        ResonanceTypeOther
    }
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

    public enum NullThrumDescriptionMode
    {
        DescriptionIntro,
        DescriptionSimple,
        DescriptionLore
    }


    public static class NullThrumUtility
    {
        public static NullThrumDescriptionMode descMode = NullThrumDescriptionMode.DescriptionIntro;

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

        public static NullThrumResonanceType ResonanceType(NullThrumAbilityType ability)
        {
            switch (ability)
            {
                case NullThrumAbilityType.Flatten: return NullThrumResonanceType.ResonanceTypeCredit;
                case NullThrumAbilityType.Scrub: return NullThrumResonanceType.ResonanceTypeCredit;
                case NullThrumAbilityType.Retune: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Harrow: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Transpose: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Infest: return NullThrumResonanceType.ResonanceTypeCredit;
                case NullThrumAbilityType.Enwomb: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Unmute: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Mute: return NullThrumResonanceType.ResonanceTypeCredit;
                case NullThrumAbilityType.Compress: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Attenuate: return NullThrumResonanceType.ResonanceTypeCredit;
                case NullThrumAbilityType.Sample: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Bootleg: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Crosstalk: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Resurrect: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Stun: return NullThrumResonanceType.ResonanceTypeDebit;
                case NullThrumAbilityType.Hallowbound: return NullThrumResonanceType.ResonanceTypeDebit;
                default: return NullThrumResonanceType.ResonanceTypeOther;
            }
        }         

        /// <summary>
        /// Returns the description in one of three modes.   
        /// </summary>        
        public static string Description(NullThrumAbilityType ability, string caster = "CASTER",
            string victim = "VICTIM")
        {
            switch (descMode)
            {
                case NullThrumDescriptionMode.DescriptionIntro:
                    if (Current.ProgramState == ProgramState.Playing && Find.World != null)
                    {
                        var tracker = Find.World.GetComponent<NullThrumTracker>();
                        if (tracker != null)
                        {
                            tracker.IncrementCount(ability);
                            if (tracker.GetCount(ability) >= 400)
                                return DescriptionLore(ability, caster, victim);
                        }
                    }
                    return DescriptionSimple(ability, caster, victim);
                case NullThrumDescriptionMode.DescriptionSimple:
                    return DescriptionSimple(ability, caster, victim);
                case NullThrumDescriptionMode.DescriptionLore:
                    return DescriptionLore(ability, caster, victim);
                default:
                    return DescriptionSimple(ability, caster, victim);
            }
        }

        /// <summary>
        /// Returns an out-of-character technical description of what the ability does mechanically.
        /// </summary>        
        public static string DescriptionSimple(NullThrumAbilityType ability, string caster = "CASTER",
            string victim = "VICTIM")
        {
            switch (ability)
            {
                case NullThrumAbilityType.Flatten:
                    return
                        $"{victim} loses all negative memories, becomes a Psychopath, and is infected with a hediff that reduces Will and Resistance.";
                case NullThrumAbilityType.Scrub:
                    return
                        $"{caster} gains resonance from consuming {victim}'s carcinomas. {caster} may selectively destroys overridden (disabled) genes to gain resonance.";
                case NullThrumAbilityType.Retune:
                    return
                        $"{caster} spends resonance to integrate selected xenogenes into their permanent endogenic sequence.";
                case NullThrumAbilityType.Harrow:
                    return
                        $"{caster} Selectively extracts active genes from a victim and adds them to {caster}'s own genome.";
                case NullThrumAbilityType.Transpose:
                    return
                        $"{caster} forcefully changes the {victim}'s current xenotype to the xenotype specified by the ability.";
                case NullThrumAbilityType.Infest:
                    return
                        $"{caster} infests {victim} with parasitic embryos that lethally birth a litter of new Fluxspawn. Lethal to {victim}.";
                case NullThrumAbilityType.Enwomb:
                    return
                        $"{caster} transforms {victim} into a Cradlemold factory. Lethal to {victim}, turning them into a permanent population engine for the brood.";
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

        public static string DescriptionLore(NullThrumAbilityType ability, string caster = "CASTER",
            string victim = "VICTIM")
        {
            switch (ability)
            {
                case NullThrumAbilityType.Flatten:
                    return
                        $"The psychological peaks of {victim} are ironed out. They no longer resist the biological overhaul, their chaotic 'noise' pressed into a single, obedient frequency. Room is made for the Absolute Frequency.";
                case NullThrumAbilityType.Scrub:
                    return
                        $"The vessel of {victim} is scoured. Disabled genes and biological 'dross' are purged, distilling latent resonance from non-functional genomic debris. {caster} refines {victim} into a clarified, resonant vessel.";
                case NullThrumAbilityType.Retune:
                    return
                        $"The gift of biological alignment is offered to {victim}. {caster} broadcasts a precise harmonic that softens {victim}'s genetic boundaries, allowing new xenogenes to be flawlessly integrated into their biological sequence.";
                case NullThrumAbilityType.Harrow:
                    return
                        $"The righteous reclamation of biological data. {caster} violently siphons specific genetic traits from {victim}, leaving behind a tattered, simplified, and 'edited' genomic wreck. {victim}'s unique biological history is forcibly archived.";
                case NullThrumAbilityType.Transpose:
                    return
                        $"The shifting of biological keys. {caster} silences {victim}'s existing genetic 'melody' and overwrites it with a stored frequency from the hive’s archive, forcing their cellular structure to align with a pre-recorded genomic template.";
                case NullThrumAbilityType.Infest:
                    return
                        $"{caster} reduces {victim} to a biological substrate, meant to nurture the hivemind's most frantic mutation engines. {victim} is blessed with the frantic, fertile frequency that will eventually drown out the noise of the world. This act is lethal to {victim}.";
                case NullThrumAbilityType.Enwomb:
                    return
                        $"{victim} is stripped of personhood and recommissioned as a biological nursery for the hivemind's expansion. {caster} transforms {victim} into infrastructure, a living biological matrix for the brood. This act is lethal to {victim}.";
                case NullThrumAbilityType.Unmute:
                    return
                        $"The biological mufflers and evolutionary static suppressing {victim}'s true resonance are stripped away. {caster} reveals {victim}'s psychic potential, allowing them to finally hear the single, perfect note of the Thrum.";
                case NullThrumAbilityType.Mute:
                    return
                        $"The internal psychic roar of {victim} is silenced. {caster} clamps down on {victim}'s chaotic psylink, compressing its wild energy into a stable, silent, and harvestable pool of Resonance. {victim}'s mind will finally be quiet.";
                case NullThrumAbilityType.Compress:
                    return
                        $"The finality of the archive. {caster} crushes {victim}'s volatile xenogenes into their permanent endogenic sequence, removing biological 'redundancy' to lock the alien frequency into the marrow with irreversible density. {victim} is no longer a host carrying a gift; they are the gift.";
                case NullThrumAbilityType.Attenuate:
                    return
                        $"The total conversion of matter into signal. {caster} fades {victim}'s physical form, turning their biological 'volume' down to zero so that their genetic information can be broadcast back into the hive’s collective resonance. This lethal process harvests {victim}'s entire genetic profile.";
                case NullThrumAbilityType.Sample:
                    return
                        $"The theft of a visual frequency. {caster} rips the aesthetic data from {victim}'s silhouette and wraps it around themselves like a digital shroud, proving that {victim}'s original form was merely a temporary suggestion.";
                case NullThrumAbilityType.Bootleg:
                    return
                        $"The capturing of a dying echo. {caster} intercepts the jagged, distorted frequencies still vibrating within {victim} before they bleed into the background hiss of entropy. This raw, high-gain recording salvages potent genetic riffs from the silence of the grave. This act is lethal to {victim}.";
                case NullThrumAbilityType.Crosstalk:
                    return
                        $"A chaotic, two-way rupture in biological insulation. {caster} deliberately drops their internal shielding to create a predatory signal loop with {victim}. The frequencies of both vessels violently bleed into one another, resulting in an unpredictable, scrambled exchange of genetic material.";
                case NullThrumAbilityType.Resurrect:
                    return
                        $"A grotesque masterpiece of biological recycling. {caster} reconstructs {victim} from cooling remains, reweaving flesh and bone into a vessel capable of catching the Null-Thrum, making {victim} another hollow link in the chain of the Stillness.";
                case NullThrumAbilityType.Stun:
                    return
                        $"{caster} emits a disorienting frequency, momentarily disrupting {victim}'s internal harmony and leaving them vulnerable to the hive's will.";
                case NullThrumAbilityType.Hallowbound:
                    return
                        $"{caster} performs the ultimate act of biological perversion, transforming {victim} into a Hallowbound. {victim} becomes an infiltrator clad in stolen skin, a perfected servant of the hivemind.";
                default:
                    return "An unknown ability of the Null-Thrum.";
            }
        }

}
}