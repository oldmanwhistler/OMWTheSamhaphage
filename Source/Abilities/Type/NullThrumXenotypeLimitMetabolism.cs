namespace OMW_Samhaphage
{
    public class NullThrumXenotypeLimitMetabolism(NullThrumDifficultyPreset setting) : NullThrumXenotypeLimit(setting)
    {
        public override void SetLimitDefaults(NullThrumDifficultyPreset settings)
        {
            disabled_value = -10000;
            if (settings == NullThrumDifficultyPreset.DifficultyHigh)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -50;
                samhaphage = -300;
                sovereign_stillness = -1000;
            }
            else if (settings == NullThrumDifficultyPreset.DifficultyMedium)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -30;
                samhaphage = -200;
                sovereign_stillness = -500;
            }
            else if (settings == NullThrumDifficultyPreset.DifficultyLow)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -10;
                samhaphage = -100;
                sovereign_stillness = -300;
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