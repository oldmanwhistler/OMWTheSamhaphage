namespace OMW_Samhaphage
{
    public class NullThrumXenotypeLimitPercentage(NullThrumLimitPreset setting) : NullThrumXenotypeLimit(setting)
    {
        public override void SetLimitDefaults(NullThrumLimitPreset settings)
        {
            if (settings == NullThrumLimitPreset.LimitHigh)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 40;
                hallowbound = 80;
                samhaphage = 60;
                sovereign_stillness = 100;
            }
            else if (settings == NullThrumLimitPreset.LimitMedium)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 30;
                hallowbound = 60;
                samhaphage = 40;
                sovereign_stillness = 100;
            }
            else if (settings == NullThrumLimitPreset.LimitLow)
            {
                fluxspawn = 50;
                echovessel = 80;
                cradlemold = 20;
                hallowbound = 40;
                samhaphage = 20;
                sovereign_stillness = 100;
            }
            else if (settings == NullThrumLimitPreset.LimitNone)
            {
                fluxspawn = 100;
                echovessel = 100;
                cradlemold = 100;
                hallowbound = 100;
                samhaphage = 100;
                sovereign_stillness = 100;
            }
        }        
    }
}