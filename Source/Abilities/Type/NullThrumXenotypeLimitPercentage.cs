namespace OMW_Samhaphage
{
    public class NullThrumXenotypeLimitPercentage(NullThrumDifficultyPreset setting) : NullThrumXenotypeLimit(setting)
    {
        public override void SetLimitDefaults(NullThrumDifficultyPreset settings)
        {
            disabled_value = 100;

            if (settings == NullThrumDifficultyPreset.DifficultyHigh)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 40;
                hallowbound = 80;
                samhaphage = 60;
                sovereign_stillness = 100;
            }
            else if (settings == NullThrumDifficultyPreset.DifficultyMedium)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 30;
                hallowbound = 60;
                samhaphage = 40;
                sovereign_stillness = 100;
            }
            else if (settings == NullThrumDifficultyPreset.DifficultyLow)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 20;
                hallowbound = 40;
                samhaphage = 20;
                sovereign_stillness = 100;
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