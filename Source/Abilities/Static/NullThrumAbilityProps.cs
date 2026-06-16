using Verse;

namespace OMW_Samhaphage
{
    public struct NullThrumAbilityProps
    {
        public NullThrumAbilityType abilityType;
        public NullThrumResourceType resourceType;

        public NullThrumResonanceType resonanceType;
        public NullThrumMathType mathType;
        public float value;
        public float min;
        public float max;

        public NullThrumAbilityProps(NullThrumAbilityType abilityType, NullThrumResourceType
            resourceType, NullThrumResonanceType resonanceType, NullThrumMathType mathType, float value)
        {
            this.resonanceType = resonanceType;
            this.abilityType = abilityType;
            this.resourceType = resourceType;
            this.mathType = mathType;
            this.value = value;

            if (this.resonanceType != NullThrumUtility.ResonanceType(this.abilityType))
            {
                Log.Error(
                    $"NullThrumAbilityProps for {NullThrumUtility.ToString(abilityType)} does not match the ResonanceType case statement in NullThrumUtility.ResonanceType({NullThrumUtility.ToString(abilityType)})");
            }

            switch (this.resourceType)
            {
                case NullThrumResourceType.ResourceTypeTrait:
                    this.min = 0f;
                    this.max = 50f;
                    break;
                case NullThrumResourceType.ResourceTypePsylink:
                    this.min = 0f;
                    this.max = 50f;
                    break;
                default:
                    this.min = 0f;
                    this.max = 10f;
                    break;
            }
        }

        public override string ToString()
        {
            string ability = NullThrumUtility.ToString(abilityType);
            string resonance = NullThrumUtility.ToString(resonanceType);
            string resource = NullThrumUtility.ToString(resourceType);

            return this.mathType switch
            {
                NullThrumMathType.MathTypeMultiplier => $"{ability}: {value} x {resource} value resonance {resonance} per {resource}",
                NullThrumMathType.MathTypeOffset => $"{ability}: {value} resonance {resonance} per {resource}",
                NullThrumMathType.MathTypeNone => $"{ability}: {resource}",
                _ => $"{ability}",
            };
        }
    }
}