namespace OMW_Samhaphage
{
        public class NullThrumXenotypeMultiplierComplexity(NullThrumDifficultyPreset setting) : NullThrumXenotypeMultiplier(setting)
        {
            public override void SetMultiplierDefaults(NullThrumDifficultyPreset settings)
            {
                disabled_value = 1.0f;

                if (settings == NullThrumDifficultyPreset.DifficultyHigh)
                {
                    fluxspawn = 0.7f;
                    echovessel = 0.75f;
                    cradlemold = 0.8f;
                    hallowbound = 0.85f;
                    samhaphage = 0.9f;
                    sovereign_stillness = 1.0f;                    
                }
                else if (settings == NullThrumDifficultyPreset.DifficultyMedium)
                {
                    fluxspawn = 0.5f;
                    echovessel = 0.6f;
                    cradlemold = 0.65f;
                    hallowbound = 0.75f;
                    samhaphage = 0.85f;
                    sovereign_stillness = 1.0f;                    
                }
                else if (settings == NullThrumDifficultyPreset.DifficultyLow)
                {
                    fluxspawn = 0.4f;
                    echovessel = 0.45f;
                    cradlemold = 0.5f;
                    hallowbound = 0.6f;
                    samhaphage = 0.8f;
                    sovereign_stillness = 1.0f;
                }
                else if (settings == NullThrumDifficultyPreset.DifficultyNone)
                {
                    fluxspawn = disabled_value;
                    echovessel = disabled_value;
                    cradlemold = disabled_value;
                    hallowbound = disabled_value;
                    samhaphage = disabled_value;
                    sovereign_stillness = disabled_value;
                }
            }
        }
    }

