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
        BlGenePack,
        BlWretch,
        BlPrereq,
        BlImplanter,
        BlStringMatch,
        BlMisc,
        BlAscension,
        BlMetamorph,
        BlTrait,
        BlSamhaphage
    }

    public class BlacklistGene
    {
        public GeneDef geneDef;
        public HashSet<BlacklistType> blacklistType;
        public string blacklistReason;

        public BlacklistGene(GeneDef geneDef)
        {
            this.geneDef = geneDef;
            this.blacklistType = new HashSet<BlacklistType>();
        }
        public void Add(BlacklistType type)
        {
            blacklistType.Add(type);
        }

        public void SetReason()
        {
            blacklistReason = "";
            if (blacklistType.Count == 0) return;

            List<string> blacklistTypeStr = new List<string>();
            
            foreach (BlacklistType type in blacklistType)
            {
                blacklistTypeStr.Add(type.ToString());
            }
            blacklistReason = string.Join(", ", blacklistTypeStr);
        }
    }

    [StaticConstructorOnStartup]
    public static class OMW_BlacklistGenes
    {
        public static readonly string Prefix = "[SAMHAPHAGE-BLACKLIST]";
        public static readonly HashSet<BlacklistGene> BlacklistedGenes = new HashSet<BlacklistGene>();

        public static readonly HashSet<GeneDef> BlacklistedGenesDontGenerate = new HashSet<GeneDef>();

        public static readonly HashSet<GeneDef> BlacklistedGenesDontCopy = new HashSet<GeneDef>();

        public static readonly HashSet<GeneDef> BlacklistedGenesDontMutate = new HashSet<GeneDef>();

        static OMW_BlacklistGenes()
        {
            RebuildBlacklist();
        }

        public static void RebuildBlacklist()
        {
            BlacklistedGenes.Clear();
            BlacklistedGenesDontCopy.Clear();
            BlacklistedGenesDontMutate.Clear();
            BlacklistedGenesDontGenerate.Clear();
           
            if (OMW_Mod.settings == null || OMW_Mod.settings.disableGeneBlacklist)
            {
                Log.Message($"{Prefix} Gene blacklist is disabled in mod settings. No genes will be blacklisted.");
                ExportCustomXenotype(); // Export an empty xenotype for Genetic Drift
                return;
            }

            HashSet<BlacklistType> blCanCopy = new HashSet<BlacklistType>();
            HashSet<BlacklistType> blCanMutate = new HashSet<BlacklistType>();
            HashSet<BlacklistType> blCanGenerate = new HashSet<BlacklistType>();
           
            blCanCopy.Add(BlacklistType.BlGenePack);
            blCanCopy.Add(BlacklistType.BlWretch);
            blCanCopy.Add(BlacklistType.BlTrait);
            blCanCopy.Add(BlacklistType.BlPrereq);
            blCanGenerate.Add(BlacklistType.BlImplanter);
            blCanGenerate.Add(BlacklistType.BlWretch);

            // AlphaGenes integration: respect the Wretch
            List<GeneDef> cachedBlacklist = new List<GeneDef>();
            List<string> cachedDefnameStrings = new List<string>();

            List<AlphaGenes.WretchBlacklistDef> allWretchBlacklistedGenes = DefDatabase<AlphaGenes.WretchBlacklistDef>.AllDefsListForReading;
            foreach (AlphaGenes.WretchBlacklistDef individualList in allWretchBlacklistedGenes)
            {
                if (!individualList.blackListedGenes.NullOrEmpty())
                    cachedBlacklist.AddRange(individualList.blackListedGenes);

                if (!individualList.blackListedDefNameStrings.NullOrEmpty())
                    cachedDefnameStrings.AddRange(individualList.blackListedDefNameStrings);
            }

            List<string> myBlacklistStrings = new List<string>();
            // core
            myBlacklistStrings.Add("ViolenceDisabled");
            myBlacklistStrings.Add("KindInstinct");
            myBlacklistStrings.Add("XenogermReimplanter");
            // WVC
            myBlacklistStrings.Add("WVC_Traitless");
            myBlacklistStrings.Add("WVC_Chimera_NullifiedLimit");
            myBlacklistStrings.Add("WVC_Chimera_GreatlyDecreasedLimit");
            // VRE
            myBlacklistStrings.Add("VRE_GermlineReimplanter");
            // AG
            myBlacklistStrings.Add("AG_InsectStinger");
            myBlacklistStrings.Add("AG_ParasiticStinger");
            myBlacklistStrings.Add("AG_InsectStingerEndogenes");
            myBlacklistStrings.Add("AG_ParasiticStingerEndogenes");
            // someone used Gene for Traits. Ban them all since they will break everything. See README.md
            myBlacklistStrings.Add("Gene_Trait_");
            

            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefs)
            {
                BlacklistGene bl = new BlacklistGene(geneDef);
                if (geneDef.canGenerateInGeneSet == false)
                {
                    bl.Add(BlacklistType.BlGenePack);
                }
                if (cachedBlacklist.Contains(geneDef) || cachedDefnameStrings.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistType.BlWretch);
                }
                if (myBlacklistStrings.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistType.BlStringMatch);
                }
                if (geneDef.prerequisite != null)
                {
                    bl.Add(BlacklistType.BlPrereq);
                }
                if (geneDef.displayCategory?.defName?.Contains("OMW_PerfectSilence") == true)
                {
                    bl.Add(BlacklistType.BlSamhaphage);
                }
                if (geneDef.displayCategory?.defName?.Contains("Reimplanter") == true)
                {
                    bl.Add(BlacklistType.BlImplanter);
                }
                if (geneDef.displayCategory?.defName?.Contains("Ascension") == true)
                {
                    bl.Add(BlacklistType.BlAscension);
                }
                if (geneDef.displayCategory?.defName?.Contains("Metamorph") == true)
                {
                    bl.Add(BlacklistType.BlMetamorph);
                }
                if (geneDef.exclusionTags?.Contains("AG_OnlyOnCharacterCreation") == true)
                {
                    bl.Add(BlacklistType.BlMisc);
                }
                if (geneDef.displayCategory?.defName?.Contains("Don't pick these") == true)
                {
                    bl.Add(BlacklistType.BlMisc);
                }

                if (geneDef.forcedTraits != null)
                {
                    foreach (GeneticTraitData traitData in geneDef.forcedTraits)
                    {
                        TraitDef traitDef = traitData.def;
                        if (traitDef == null) continue;

                        if (traitDef.conflictingTraits?.Count > 0)
                        {
                            bl.Add(BlacklistType.BlTrait);
                            break;
                        }

                        if (traitDef.conflictingPassions?.Count > 0)
                        {
                            bl.Add(BlacklistType.BlTrait);
                            break;
                        }

                        if (traitDef.requiredWorkTypes?.Count > 0)
                        {
                            bl.Add(BlacklistType.BlTrait);
                            break;
                        }

                        if (traitDef.disabledWorkTypes?.Count > 0)
                        {
                            bl.Add(BlacklistType.BlTrait);
                            break;
                        }
                    }
                }
                if (bl.blacklistType.Count > 0)
                {
                    bl.SetReason();

                    // all genes that can be blacklisted
                    BlacklistedGenes.Add(bl);

                    if (!bl.blacklistType.Overlaps(blCanCopy))
                    {
                        BlacklistedGenesDontCopy.Add(bl.geneDef);
                    }

                    if (!bl.blacklistType.Overlaps(blCanMutate))
                    {
                        BlacklistedGenesDontMutate.Add(bl.geneDef);
                    }

                    if (!bl.blacklistType.Overlaps(blCanGenerate))
                    {
                        BlacklistedGenesDontGenerate.Add(bl.geneDef);
                    }
                }
            }

            ExportCustomXenotype();
        }

        static void ExportCustomXenotype()
        {
            if (!ModsConfig.IsActive("masstell.geneticdrift16"))
            {
                return;
            }

            // --- XML Export for Blacklist Xenotype ---
            try
            {
                string versionDir = VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor;
                string autogenPath = Path.Combine(LoadedModManager.GetMod<OMW_Mod>().Content.RootDir, versionDir);
                autogenPath = Path.Combine(autogenPath, "Defs");
                autogenPath = Path.Combine(autogenPath, "XenotypeDefs");
                autogenPath = Path.Combine(autogenPath, "autogen");
                Directory.CreateDirectory(autogenPath); // Ensure directory exists
                string xmlPath = Path.Combine(autogenPath, "omw_gene_blacklist.xml");

                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Defs",
                        new XElement("XenotypeDef",
                            new XElement("defName", "omw_samhaphage_blacklist"),
                            new XElement("label", "blacklist for genetic drift"),
                            new XElement("descriptionShort", "DO NOT SPAWN. This is an autogenerated Xenotype for compatibility wih Genetic Drift."),
                            new XElement("description", "DO NOT SPAWN. This Xenotype is meant to be used as a blacklist with the 'Genetic Drift 1.6' mod. Go into the Genetic Drift 1.6 mod settings and configure OMW_Samhaphage_Gene_Blacklist as a blacklist Xenotype."),
                            new XElement("iconPath", "UI/Icons/Xenotypes/Baseliner"),
                            new XElement("inheritable", "false"),
                            new XElement("factionlessGenerationWeight", "0"),
                            new XElement("genes",
                                from gene in DefDatabase<GeneDef>.AllDefs
                                where OMW_BlacklistGenes.BlacklistedGenesDontGenerate.Any(bl => bl == gene)
                                select new XElement("li", 
                                    (gene.modContentPack != null && !gene.modContentPack.IsCoreMod && gene.modContentPack.PackageId.ToLower() != "ludeon.rimworld.biotech") ? new XAttribute("MayRequire", gene.modContentPack.PackageId) : null,
                                    gene.defName)
                            )
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

        public static void ExportBlacklistGeneReport()
        {
           try
            {

                string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "OMW_Samhaphage_Report_Blacklisted_Genes.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine("DEFNAME,LABEL,COMPLEXITY,METABOLISM,ARCHITE,CATEGORY,CATEGORYDEF,BLACKLISTED,ABILITIES,DESCRIPTION");

                foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
                {
                    string label = gene.label?.Replace("\"", "\"\"") ?? "";
                    string cat = gene.displayCategory?.label?.Replace("\"", "\"\"") ?? "None";
                    string catDef = gene.displayCategory?.defName ?? "None";

                    BlacklistGene blEntry = BlacklistedGenes.FirstOrDefault(x => x.geneDef == gene);
                    string bl = blEntry?.blacklistReason ?? "no blacklist";

                    string abilities = (gene.abilities != null && gene.abilities.Count > 0) 
                        ? string.Join("|", gene.abilities.ConvertAll(a => a.defName)) 
                        : "";
                    string desc = gene.description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "") ?? "";

                    sb.AppendLine($"\"{gene.defName}\",\"{label}\",{gene.biostatCpx},{gene.biostatMet},{gene.biostatArc},\"{cat}\",\"{catDef}\",{bl},\"{abilities}\",\"{desc}\"");
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