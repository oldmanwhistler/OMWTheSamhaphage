namespace OMW_Samhaphage
{
    public class NullThrumXenotypeLimitMetabolism(NullThrumLimitPreset setting) : NullThrumXenotypeLimit(setting)
    {
        public override void SetLimitDefaults(NullThrumLimitPreset settings)
        {
            if (settings == NullThrumLimitPreset.LimitHigh)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -50;
                samhaphage = -300;
                sovereign_stillness = -1000;
            }
            else if (settings == NullThrumLimitPreset.LimitMedium)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -30;
                samhaphage = -200;
                sovereign_stillness = -500;
            }
            else if (settings == NullThrumLimitPreset.LimitLow)
            {
                fluxspawn = -5;
                echovessel = -5;
                cradlemold = 0;
                hallowbound = -10;
                samhaphage = -100;
                sovereign_stillness = -300;
            }
            else if (settings == NullThrumLimitPreset.LimitNone)
            {
                fluxspawn = -10000;
                echovessel = -10000;
                cradlemold = -10000;
                hallowbound = -10000;
                samhaphage = -10000;
                sovereign_stillness = -10000;
            }
        }
    }
}