using Verse;

namespace OMW_Samhaphage
{
    /// <summary>
    /// Centralized logger for The Samhaphage. 
    /// </summary>
    public class Logger
    {
        private readonly string prefix;
        private readonly string subPrefix;

        public Logger(string subPrefix)
        {
            this.subPrefix = subPrefix;
            this.prefix = $"[SAMHAPHAGE-{subPrefix}] ";
        }

        public void Message(string text)
        {
            Log.Message(prefix + text);
        }

        public void Warning(string text)
        {
            Log.Warning(prefix + text);
        }

        public void Error(string text)
        {
            Log.Error(prefix + text);
        }

        public void Debug(string text)
        {
            if (IsDebugEnabled())
            {
                Log.Message(prefix + "[DEBUG] " + text);
            }
        }

        private bool IsDebugEnabled()
        {
            if (OMW_Mod.settings == null) return false;
            return subPrefix switch
            {
                "Abilities" => OMW_Mod.settings.logAbilities,
                "Anomaly" => OMW_Mod.settings.logAnomaly,
                "CompAbilityEffect" => OMW_Mod.settings.logCompAbilityEffect,
                "Genes" => OMW_Mod.settings.logGenes,
                "Resonance" => OMW_Mod.settings.logResonance,
                "Hediffs" => OMW_Mod.settings.logHediffs,
                "Jobs" => OMW_Mod.settings.logJobs,
                "UI" => OMW_Mod.settings.logUI,
                _ => false,
            };
        }
    }
}