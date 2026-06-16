using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public struct NullThrumAbilities
    {
        public List<NullThrumAbilityProps> listGeneMulti = new List<NullThrumAbilityProps>();
        public List<NullThrumAbilityProps> listGeneFixed = new List<NullThrumAbilityProps>();
        public List<NullThrumAbilityProps> listTraitMulti = new List<NullThrumAbilityProps>();
        public List<NullThrumAbilityProps> listOther = new List<NullThrumAbilityProps>();


        // flat gain: apply scoured mind and silent servitude once per pawn
        public NullThrumAbilityProps flatten = new NullThrumAbilityProps(
            NullThrumAbilityType.Flatten,
            NullThrumResourceType.ResourceTypePawn,
            NullThrumResonanceType.ResonanceTypeCredit, 
            NullThrumMathType.MathTypeOffset, 
            3.0f);

        // multiplier gain, select: destroy a disabled gene
        public NullThrumAbilityProps scrub = new NullThrumAbilityProps(
            NullThrumAbilityType.Scrub,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeMultiplier, 4.0f);

        public NullThrumAbilityProps scrubCarcinoma = new NullThrumAbilityProps(
            NullThrumAbilityType.Scrub,
            NullThrumResourceType.ResourceTypeCarcinoma,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeOffset, 5.0f);

        // multiplier credit, destroy gene
        public NullThrumAbilityProps nullify = new NullThrumAbilityProps(
            NullThrumAbilityType.Nullify,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeMultiplier, 2.5f);

        // multiplier cost, select: move single xenogene to endogene
        public NullThrumAbilityProps retune = new NullThrumAbilityProps(
            NullThrumAbilityType.Retune,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeMultiplier, 0.5f);
        // multiplier cost, all: moves all xenogenes to endogenes
        public NullThrumAbilityProps compress = new NullThrumAbilityProps(
            NullThrumAbilityType.Compress,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeMultiplier, 0.1f);
        // no control: randomly exchange xenogenes with victim
        public NullThrumAbilityProps crosstalk = new NullThrumAbilityProps(
            NullThrumAbilityType.Crosstalk,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeMultiplier, 0.4f);

        // multiplier cost: steal cosmetic gene
        public NullThrumAbilityProps sample = new NullThrumAbilityProps(
            NullThrumAbilityType.Sample,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeMultiplier, 0.3f);
        // multiplier cost: steal gene
        public NullThrumAbilityProps harrow = new NullThrumAbilityProps(
            NullThrumAbilityType.Harrow,
            NullThrumResourceType.ResourceTypeGene,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeMultiplier, 2.0f);

        // kills victim while converting genes to resonance
        public NullThrumAbilityProps attenuate = new NullThrumAbilityProps(
            NullThrumAbilityType.Attenuate,
            NullThrumResourceType.ResourceTypeGeneAndTrait,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeMultiplier, 0.75f);

        // flat cost, psylink: add psylink
        public NullThrumAbilityProps unmute = new NullThrumAbilityProps(
            NullThrumAbilityType.Unmute,
            NullThrumResourceType.ResourceTypePsylink,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 30.0f);
        // flat gain, psylink: destroy psylink
        public NullThrumAbilityProps mute = new NullThrumAbilityProps(
            NullThrumAbilityType.Mute,
            NullThrumResourceType.ResourceTypePsylink,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeOffset, 20.0f);

        // trait: steal a trait from victim
        public NullThrumAbilityProps bootleg = new NullThrumAbilityProps(
            NullThrumAbilityType.Bootleg,
            NullThrumResourceType.ResourceTypeTrait,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 1.8f);

        // trait: destroy a trait to gain resonance
        public NullThrumAbilityProps excise = new NullThrumAbilityProps(
            NullThrumAbilityType.Excise,
            NullThrumResourceType.ResourceTypeTrait,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeMultiplier, 2.0f);

        // flat cost: change xenotype
        public NullThrumAbilityProps transpose = new NullThrumAbilityProps(
            NullThrumAbilityType.Transpose,
            NullThrumResourceType.ResourceTypeChangeXenotype,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 3.0f);

        // flat cost: change xenotype
        public NullThrumAbilityProps hallowbound = new NullThrumAbilityProps(
            NullThrumAbilityType.Hallowbound,
            NullThrumResourceType.ResourceTypePawn,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 1.0f);

        // flat cost, kills pawn: parasite infestation
        public NullThrumAbilityProps infest = new NullThrumAbilityProps(
            NullThrumAbilityType.Infest,
            NullThrumResourceType.ResourceTypePawn,
            NullThrumResonanceType.ResonanceTypeSacrificeVictim,
            NullThrumMathType.MathTypeNone, 0.0f);

        // flat cost: stun enemy
        public NullThrumAbilityProps stun = new NullThrumAbilityProps(
            NullThrumAbilityType.Stun,
            NullThrumResourceType.ResourceTypeAbility,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 1.0f);

        // flat cost: resurrect a corpse
        public NullThrumAbilityProps resurrect = new NullThrumAbilityProps(
            NullThrumAbilityType.Resurrect,
            NullThrumResourceType.ResourceTypeCorpse,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 1.0f);

        // sacrifice caster, flat cost: change xenotype
        public NullThrumAbilityProps enwomb = new NullThrumAbilityProps(
            NullThrumAbilityType.Enwomb,
            NullThrumResourceType.ResourceTypePawn,
            NullThrumResonanceType.ResonanceTypeDebit,
            NullThrumMathType.MathTypeOffset, 1.0f);

        public NullThrumAbilityProps amplify = new NullThrumAbilityProps(
            NullThrumAbilityType.Amplify,
            NullThrumResourceType.ResourceTypeChangeXenotype,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeOffset, 75f);

        public NullThrumAbilityProps render = new NullThrumAbilityProps(
            NullThrumAbilityType.Render,
            NullThrumResourceType.ResourceTypeCorpse,
            NullThrumResonanceType.ResonanceTypeCredit,
            NullThrumMathType.MathTypeOffset, 30.0f);        

        public NullThrumAbilities()
        {
            listGeneFixed.Add(flatten);
            listGeneMulti.Add(scrub);
            listGeneMulti.Add(nullify);
            listGeneMulti.Add(retune);
            listGeneMulti.Add(harrow);
            listGeneFixed.Add(transpose);
            listGeneFixed.Add(infest);
            listGeneFixed.Add(enwomb);
            listGeneMulti.Add(compress);
            listGeneMulti.Add(attenuate);
            listGeneMulti.Add(sample);
            listGeneMulti.Add(crosstalk);
            listTraitMulti.Add(bootleg);
            listTraitMulti.Add(excise);
            listOther.Add(unmute);
            listOther.Add(mute);
            listOther.Add(resurrect);
            listOther.Add(hallowbound);
            listOther.Add(stun);
            listOther.Add(scrubCarcinoma);
            listOther.Add(amplify);
            listOther.Add(render);
        }
    }
}