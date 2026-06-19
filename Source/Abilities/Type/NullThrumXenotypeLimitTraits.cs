namespace OMW_Samhaphage
{
        public class NullThrumXenotypeLimitTraits(NullThrumLimitPreset setting) : NullThrumXenotypeLimit(setting)
        {
            // Going to do this as percentage of total available traits
            // single tiered traits 27
            // spectrum traits 37
            // sexuality traits 3
            // total 67: almost nice
            // 67/22 gives a minimum of 3 traits for vanilla
            // 67/13 gives a minimum of 5 traits for vanilla
            public override void SetLimitDefaults(NullThrumLimitPreset settings)
            {
                if (settings == NullThrumLimitPreset.LimitHigh)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 25;
                    samhaphage = 40;
                    sovereign_stillness = 60;
                }
                else if (settings == NullThrumLimitPreset.LimitMedium)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 20;
                    samhaphage = 30;
                    sovereign_stillness = 45;
                }
                else if (settings == NullThrumLimitPreset.LimitLow)
                {
                    fluxspawn = 13;
                    echovessel = 13;
                    cradlemold = 13;
                    hallowbound = 15;
                    samhaphage = 25;
                    sovereign_stillness = 35;
                }
                else if (settings == NullThrumLimitPreset.LimitNone)
                {
                    fluxspawn = 1000;
                    echovessel = 1000;
                    cradlemold = 1000;
                    hallowbound = 1000;
                    samhaphage = 1000;
                    sovereign_stillness = 1000;
                }
            }
        }
    }

