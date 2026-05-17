using Verse;
using System.Collections.Generic;
using AlphaGenes;
using System.IO;
using System.Text;
using RimWorld;
using UnityEngine;
using System.Xml.Linq; // Added for XML generation
using System.Linq; // Added for LINQ operations

namespace OMW_Samhaphage
{
    public enum BlacklistType
    {
        blGenePack,
        blWretch,
        blPrereq,
        blImplanter,
        blNewCharacter
    }

    public class BlacklistGene
    {
        public GeneDef geneDef;
        public HashSet<BlacklistType> blacklistType;

        public BlacklistGene(GeneDef geneDef)
        {
            this.geneDef = geneDef;
            this.blacklistType = new HashSet<BlacklistType>();
        }
        public void Add(BlacklistType type)
        {
            blacklistType.Add(type);
        }
    }

    [StaticConstructorOnStartup]
    public static class OMW_BlacklistGenes
    {
        public static readonly string Prefix = "[SAMHAPHAGE-BLACKLIST]";
        private static readonly HashSet<BlacklistGene> BlacklistedGenes = new HashSet<BlacklistGene>();

        public static readonly HashSet<GeneDef> BlacklistedGenesResonance = new HashSet<GeneDef>();

        public static readonly HashSet<GeneDef> BlacklistedGenesMutation = new HashSet<GeneDef>();

        private static List<GeneDef> cachedBlacklist;
        private static List<string> cachedDefnameStrings;

        static OMW_BlacklistGenes()
        {
            RebuildBlacklist();
        }

        public static void RebuildBlacklist()
        {
            BlacklistedGenes.Clear();
            BlacklistedGenesResonance.Clear();
            BlacklistedGenesMutation.Clear();

            cachedBlacklist = new List<GeneDef>();
            cachedDefnameStrings = new List<string>();

            List<AlphaGenes.WretchBlacklistDef> allWretchBlacklistedGenes = DefDatabase<AlphaGenes.WretchBlacklistDef>.AllDefsListForReading;
            foreach (AlphaGenes.WretchBlacklistDef individualList in allWretchBlacklistedGenes)
            {
                if (!individualList.blackListedGenes.NullOrEmpty())
                    cachedBlacklist.AddRange(individualList.blackListedGenes);

                if (!individualList.blackListedDefNameStrings.NullOrEmpty())
                    cachedDefnameStrings.AddRange(individualList.blackListedDefNameStrings);
            }

            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefs)
            {
                BlacklistGene bl = new BlacklistGene(geneDef);
                if (geneDef.canGenerateInGeneSet == false)
                {
                    bl.Add(BlacklistType.blGenePack);
                }
                if (cachedBlacklist.Contains(geneDef) || cachedDefnameStrings.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistType.blWretch);
                }
                if (geneDef.prerequisite != null)
                {
                    bl.Add(BlacklistType.blPrereq);
                }
                if (geneDef.displayCategory?.defName?.Contains("Reimplanter") == true)
                {
                    bl.Add(BlacklistType.blImplanter);
                }
                if (geneDef.exclusionTags?.Contains("AG_OnlyOnCharacterCreation") == true)
                {
                    bl.Add(BlacklistType.blNewCharacter);
                }
                if (bl.blacklistType.Count > 0)
                {
                    BlacklistedGenes.Add(bl);
                }
            }

            ExportCustomXenotype();
            ExportBlacklistReport();
        }

        static void ExportCustomXenotype()
        {
            // --- XML Export for Blacklist Xenotype ---
            try
            {
                string xenotypesFolderPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "Xenotypes");
                Directory.CreateDirectory(xenotypesFolderPath); // Ensure directory exists
                string xmlPath = Path.Combine(xenotypesFolderPath, "OMW_Samphaphage_Gene_Blacklist.xtp");

                var activeMods = ModLister.AllInstalledMods.Where(m => m.Active && m.SteamAppId > 0).ToList();

                XDocument doc = new XDocument(
                    new XElement("savedXenotype",
                        new XElement("meta",
                            new XElement("gameVersion", VersionControl.CurrentVersionString),
                            new XElement("modIds"),
                            new XElement("modSteamIds"),
                            new XElement("modNames")
                        ),
                        new XElement("xenotype",
                            new XElement("name", "OMW_Samphaphage_Gene_Blacklist"),
                            new XElement("inheritable", "False"),
                            new XElement("genes",
                                from gene in DefDatabase<GeneDef>.AllDefs
                                where !OMW_BlacklistGenes.BlacklistedGenes.Any(bl => bl.geneDef == gene)
                                select new XElement("li", gene.defName)
                            ),
                            new XElement("iconDef", "Basic")
                        )
                    )
                );

                doc.Save(xmlPath);
                Log.Message($"[SAMHAPHAGE-BLACKLIST] Exported blacklist xenotype XML to {xmlPath}");
            }
            catch (System.Exception ex)
            {
                Log.Error("[SAMHAPHAGE-BLACKLIST] Error: Failed to export blacklist xenotype XML: " + ex.Message);
            }
        }

        static void ExportBlacklistReport()
        {
           try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "OMW_Samhaphage_Report_Blacklisted_Genes.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine("DEFNAME,LABEL,COMPLEXITY,METABOLISM,ARCHITE,CATEGORY,CATEGORYDEF,BLACKLISTED,GENEPACK,WRETCH,PREREQ,IMPLANTER,NEWCHARACTER,ABILITIES,DESCRIPTION");

                foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
                {
                    string label = gene.label?.Replace("\"", "\"\"") ?? "";
                    string cat = gene.displayCategory?.label?.Replace("\"", "\"\"") ?? "None";
                    string catDef = gene.displayCategory?.defName ?? "None";

                    BlacklistGene blEntry = BlacklistedGenes.FirstOrDefault(x => x.geneDef == gene);
                    bool isBl = blEntry != null;
                    bool blGP = blEntry?.blacklistType.Contains(BlacklistType.blGenePack) ?? false;
                    bool blW = blEntry?.blacklistType.Contains(BlacklistType.blWretch) ?? false;
                    bool blP = blEntry?.blacklistType.Contains(BlacklistType.blPrereq) ?? false;
                    bool blI = blEntry?.blacklistType.Contains(BlacklistType.blImplanter) ?? false;
                    bool blNC = blEntry?.blacklistType.Contains(BlacklistType.blNewCharacter) ?? false;

                    string abilities = (gene.abilities != null && gene.abilities.Count > 0) 
                        ? string.Join("|", gene.abilities.ConvertAll(a => a.defName)) 
                        : "";
                    string desc = gene.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ?? "";

                    sb.AppendLine($"\"{gene.defName}\",\"{label}\",{gene.biostatCpx},{gene.biostatMet},{gene.biostatArc},\"{cat}\",\"{catDef}\",{isBl},{blGP},{blW},{blP},{blI},{blNC},\"{abilities}\",\"{desc}\"");
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported blacklist report to {path}");
                Messages.Message($"{Prefix} Exported blacklist report to {path}", MessageTypeDefOf.TaskCompletion, false);
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export genes: " + ex.Message);
            }            
        }
    }
}