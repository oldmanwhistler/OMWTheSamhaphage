namespace OMW_Samhaphage
{
        public class NullThrumXenotypeLimitTraits(NullThrumDifficultyPreset setting) : NullThrumXenotypeLimit(setting)
        {
            // Going to do this as percentage of total available traits
            // single tiered traits 27
            // spectrum traits 37
            // sexuality traits 3
            // total 67: almost nice
            // 67/22 gives a minimum of 3 traits for vanilla
            // 67/13 gives a minimum of 5 traits for vanilla
            public override void SetLimitDefaults(NullThrumDifficultyPreset settings)
            {
                disabled_value = 1000;

                if (settings == NullThrumDifficultyPreset.DifficultyHigh)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 25;
                    samhaphage = 40;
                    sovereign_stillness = 60;
                }
                else if (settings == NullThrumDifficultyPreset.DifficultyMedium)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 20;
                    samhaphage = 30;
                    sovereign_stillness = 45;
                }
                else if (settings == NullThrumDifficultyPreset.DifficultyLow)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 15;
                    samhaphage = 25;
                    sovereign_stillness = 35;
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

