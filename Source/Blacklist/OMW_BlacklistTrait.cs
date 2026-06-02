using Verse;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace OMW_Samhaphage
{
    public enum BlacklistTraitType
    {
        BlStringMatch,
        BlConflict,
        BlWork,
        BlMod
    }

    public class BlacklistTrait
    {
        public TraitDef traitDef;
        public HashSet<BlacklistTraitType> BlacklistTraitType;
        public string blacklistReason;

        public BlacklistTrait(TraitDef traitDef)
        {
            this.traitDef = traitDef;
            this.BlacklistTraitType = new HashSet<BlacklistTraitType>();
        }
        public void Add(BlacklistTraitType type)
        {
            BlacklistTraitType.Add(type);
        }

        public void SetReason()
        {
            blacklistReason = "";
            if (BlacklistTraitType.Count == 0) return;

            List<string> typeStrings = new List<string>();
            foreach (BlacklistTraitType type in BlacklistTraitType)
            {
                typeStrings.Add(type.ToString());
            }
            blacklistReason = string.Join(" ", typeStrings);
        }
    }

    [StaticConstructorOnStartup]
    public static class OMW_BlacklistTraits
    {
        public static readonly string Prefix = "[SAMHAPHAGE-TRAIT-BLACKLIST]";
        public static readonly HashSet<BlacklistTrait> BlacklistedTraits = new HashSet<BlacklistTrait>();

        public static readonly HashSet<TraitDef> BlacklistedTraitsDontCopy = new HashSet<TraitDef>();
        public static readonly HashSet<TraitDef> BlacklistedTraitsDontRemove = new HashSet<TraitDef>();


        static OMW_BlacklistTraits()
        {
            RebuildBlacklist();
        }

        public static void RebuildBlacklist()
        {
            BlacklistedTraits.Clear();
            BlacklistedTraitsDontCopy.Clear();
            BlacklistedTraitsDontRemove.Clear();            

            if (OMW_Mod.settings == null || OMW_Mod.settings.disableTraitBlacklist)
            {
                Log.Message($"{Prefix} Trait blacklist is disabled in mod settings. No traits will be blacklisted.");
                return;
            }

            HashSet<BlacklistTraitType> blCanCopy = new HashSet<BlacklistTraitType>();
            HashSet<BlacklistTraitType> blCanRemove = new HashSet<BlacklistTraitType>();
            blCanRemove.Add(BlacklistTraitType.BlConflict);
            blCanRemove.Add(BlacklistTraitType.BlWork);

            List<string> myBlacklistStrings = new List<string>();
            myBlacklistStrings.Add("Isekai_Rank_"); // Isekai Leveling
            myBlacklistStrings.Add("HVT_Awakened"); // Hauts Added Traits' Awakened Psychics
            myBlacklistStrings.Add("HVT_Test");

            List<string> myBlacklistMods = new List<string>();
            myBlacklistMods.Add("Shadow Monarch");
            
            foreach (TraitDef traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                BlacklistTrait bl = new BlacklistTrait(traitDef);
                string modName = traitDef.modContentPack?.Name ?? traitDef.modContentPack?.PackageId ?? "Unknown";
                
                if (myBlacklistStrings.Any(s => traitDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistTraitType.BlStringMatch);
                }
                
                if (myBlacklistStrings.Any(s => s == modName))
                {
                    bl.Add(BlacklistTraitType.BlMod);
                }
                    
                // Blacklist traits with excessive conflicts to prevent UI/Mechanic clutter
                if (!traitDef.conflictingTraits.NullOrEmpty() && traitDef.conflictingTraits.Count > 2)
                {
                    bl.Add(BlacklistTraitType.BlConflict);
                }

                // Blacklist traits that restrict or mandate specific work types
                if (!traitDef.disabledWorkTypes.NullOrEmpty() || !traitDef.requiredWorkTypes.NullOrEmpty())
                {
                    bl.Add(BlacklistTraitType.BlWork);
                }

                if (bl.BlacklistTraitType.Count > 0)
                {
                    bl.SetReason();
                    BlacklistedTraits.Add(bl);

                    if (!bl.BlacklistTraitType.Overlaps(blCanCopy))
                    {
                        BlacklistedTraitsDontCopy.Add(bl.traitDef);
                    }

                    if (!bl.BlacklistTraitType.Overlaps(blCanRemove))
                    {
                        BlacklistedTraitsDontRemove.Add(bl.traitDef);
                    }                    
                }
            }
        }

        public static void ExportBlacklistTraitReport()
        {
           try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "OMW_Samhaphage_Report_Blacklisted_Traits.csv");
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("DEFNAME,LABEL,MOD,BLACKLISTED,DESCRIPTION");

                foreach (TraitDef trait in DefDatabase<TraitDef>.AllDefs)
                {
                    string label = trait.label?.Replace("\"", "\"\"") ?? "";
                    BlacklistTrait blEntry = BlacklistedTraits.FirstOrDefault(x => x.traitDef == trait);
                    string bl = blEntry?.blacklistReason ?? "no blacklist";
                    string desc = trait.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ?? "";
                    string modName = trait.modContentPack?.Name ?? trait.modContentPack?.PackageId ?? "Unknown";

                    sb.AppendLine($"\"{trait.defName}\",\"{label}\",\"{modName}\",\"{bl}\",\"{desc}\"");
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported trait blacklist report to {path}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export trait report: " + ex.Message);
            }            
        }
    }
}