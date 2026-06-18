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
        BlDontCopy,
        BlDontRemove,
        BlMod
    }

    public class BlacklistTrait
    {
        public TraitDef traitDef;
        public TraitDegreeData data;
        public HashSet<BlacklistTraitType> BlacklistTraitType;
        public string blacklistReason;

        public BlacklistTrait(TraitDef traitDef, TraitDegreeData data)
        {
            this.traitDef = traitDef;
            this.data = data;
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

            List<string> myBlacklistStrings = new List<string>();
            myBlacklistStrings.Add("Isekai_Rank_"); // Isekai Leveling
            myBlacklistStrings.Add("HVT_Awakened"); // Hauts Added Traits' Awakened Psychics
            myBlacklistStrings.Add("HVT_Test");

            List<string> myBlacklistDontCopy = new List<string>();
            List<string> myBlacklistDontRemove = new List<string>();
            foreach (string str in myBlacklistStrings)
            {
                myBlacklistDontCopy.Add(str);
                myBlacklistDontRemove.Add(str);
            }

            List<string> myBlacklistMods = new List<string>();
            myBlacklistMods.Add("Shadow Monarch");
            
            blCanCopy.Add(BlacklistTraitType.BlDontRemove);
            blCanRemove.Add(BlacklistTraitType.BlDontCopy);



            foreach (TraitDef traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                string modName = traitDef.modContentPack?.Name ?? traitDef.modContentPack?.PackageId ?? "Unknown";
                foreach (TraitDegreeData data in traitDef.degreeDatas)
                {

                    BlacklistTrait bl = new BlacklistTrait(traitDef, data);
                    if (myBlacklistDontCopy.Any(s => traitDef.defName.Contains(s)))
                    {
                        bl.Add(BlacklistTraitType.BlDontCopy);
                    }
                    if (myBlacklistDontRemove.Any(s => traitDef.defName.Contains(s)))
                    {
                        bl.Add(BlacklistTraitType.BlDontRemove);
                    }

                    if (myBlacklistMods.Any(s => modName.Contains(s)))
                    {
                        bl.Add(BlacklistTraitType.BlMod);
                    }

                    if (bl.BlacklistTraitType.Count > 0)
                    {
                        bl.SetReason();
                        BlacklistedTraits.Add(bl);

                        if (!bl.BlacklistTraitType.IsSubsetOf(blCanCopy))
                        {
                            BlacklistedTraitsDontCopy.Add(bl.traitDef);
                        }

                        if (!bl.BlacklistTraitType.IsSubsetOf(blCanRemove))
                        {
                            BlacklistedTraitsDontRemove.Add(bl.traitDef);
                        }
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

                sb.AppendLine("DEFNAME,MOD_NAME,TRAIT_LABEL,DEGREE,DEGREE_LABEL,TRAIT_DESC,DEGREE_DESC");

                foreach (TraitDef traitDef in DefDatabase<TraitDef>.AllDefs)
                {
                    string modName = traitDef.modContentPack?.Name ?? traitDef.modContentPack?.PackageId ?? "Unknown";
                    string traitLabel = traitDef.label?.Replace("\"", "\"\"") ?? "";
                    string traitDefName = traitDef.defName;
                    string traitDesc =
                        traitDef.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ??
                        "";
                    foreach (TraitDegreeData data in traitDef.degreeDatas)
                    {
                        string degreeLabel = data.label?.Replace("\"", "\"\"") ?? "";
                        string degree = data.degree.ToString();
                        BlacklistTrait blEntry = BlacklistedTraits.FirstOrDefault(x => x.data == data);
                        string bl = blEntry?.blacklistReason ?? "no blacklist";
                        string degreeDesc =
                            data.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ??
                            "";


                        sb.AppendLine($"\"{traitDefName}\",\"{modName}\",\"{traitLabel}\",\"{degree}\",\"{degreeLabel}\",\"{bl}\",\"{traitDesc}\",\"{degreeDesc}\"");
                    }
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported trait blacklist report to {path}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export trait report: " + ex.Message);
            }            
        }

        public static void FixColonistsWithRepeatedTraits()
        {
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
            {
                if (pawn.story?.traits == null) continue;

                List<TraitDef> keptTraits = new List<TraitDef>();
                List<Trait> toRemove = new List<Trait>();

                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    if (trait.sourceGene != null) continue;
                    
                    bool isDuplicate = keptTraits.Contains(trait.def);
                    bool isConflicting = false;

                    if (!isDuplicate)
                    {
                        foreach (TraitDef kept in keptTraits)
                        {
                            if (trait.def.ConflictsWith(kept) || kept.ConflictsWith(trait.def))
                            {
                                isConflicting = true;
                                break;
                            }
                        }
                    }

                    if (isDuplicate || isConflicting)
                    {
                        toRemove.Add(trait);
                    }
                    else
                    {
                        keptTraits.Add(trait.def);
                    }
                }

                foreach (Trait trait in toRemove)
                {
                    pawn.story.traits.RemoveTrait(trait);
                    Log.Message($"{Prefix} Removed redundant or conflicting trait '{trait.Label}' from {pawn.LabelShort}");
                }
            }
        }
    }
}