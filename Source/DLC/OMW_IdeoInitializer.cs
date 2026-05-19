using System;
using System.IO;
using Verse;

namespace OMW_Samhaphage
{
    [StaticConstructorOnStartup]
    public static class OMW_IdeoInitializer
    {
        static OMW_IdeoInitializer()
        {
            if (!ModsConfig.IdeologyActive) return;
                
            const string fileName = "The Absolute Frequency.rid";
            
            // Get the root directory of the mod.
            string modRoot = LoadedModManager.GetMod<OMW_Mod>().Content.RootDir;
            
            // Construct the source path: /1.6/Ideo/The Absolute Frequency.rid
            string sourcePath = Path.Combine(modRoot, "1.6", "Ideo", fileName);

            if (!File.Exists(sourcePath))
            {
                Log.Error($"[OMW_Samhaphage] couldn't find Ideology template {fileName}");
                return;
            }

            // Get the game's Ideo directory in the local user profile save data.
            string destFolder = Path.Combine(GenFilePaths.SaveDataFolderPath, "Ideos");
            string destPath = Path.Combine(destFolder, fileName);

            // Copy only if the file doesn't already exist at the destination.
            if (!File.Exists(destPath))
            {
                try
                {
                    File.Copy(sourcePath, destPath);
                    Log.Message($"[OMW_Samhaphage] Successfully deployed Ideo template: {fileName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"[OMW_Samhaphage] Failed to copy custom Ideo template: {ex.Message}");
                }
            }
        }
    }
}