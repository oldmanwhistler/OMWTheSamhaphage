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
    public enum BlacklistGeneType
    {
        BlGenePack,
        BlWretch,
        BlPrereq,
        BlImplanter,
        BlDontCopy,
        BlDontRemove,        
        BlMisc,
        BlAscension,
        BlMetamorph,
        BlTrait,
        BlSamhaphage,
        BlReproduction
    }

    public class BlacklistGene
    {
        public GeneDef geneDef;
        public HashSet<BlacklistGeneType> BlacklistGeneType;
        public string blacklistReason;

        public BlacklistGene(GeneDef geneDef)
        {
            this.geneDef = geneDef;
            this.BlacklistGeneType = new HashSet<BlacklistGeneType>();
        }
        public void Add(BlacklistGeneType type)
        {
            BlacklistGeneType.Add(type);
        }

        public void SetReason()
        {
            blacklistReason = "";
            if (BlacklistGeneType.Count == 0) return;

            List<string> BlacklistGeneTypeStr = new List<string>();
            
            foreach (BlacklistGeneType type in BlacklistGeneType)
            {
                BlacklistGeneTypeStr.Add(type.ToString());
            }
            blacklistReason = string.Join(" ", BlacklistGeneTypeStr);
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

        public static readonly HashSet<GeneDef> BlacklistedGenesDontRemove = new HashSet<GeneDef>();

        public static readonly HashSet<GeneDef> PreggoGenes = new HashSet<GeneDef>();

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
            BlacklistedGenesDontRemove.Clear();
            PreggoGenes.Clear();
           
            if (OMW_Mod.settings == null || OMW_Mod.settings.disableGeneBlacklist)
            {
                Log.Message($"{Prefix} Gene blacklist is disabled in mod settings. No genes will be blacklisted.");
                ExportCustomXenotype(); // Export an empty xenotype for Genetic Drift
                return;
            }

            HashSet<BlacklistGeneType> blCanCopy = new HashSet<BlacklistGeneType>();
            HashSet<BlacklistGeneType> blCanMutate = new HashSet<BlacklistGeneType>();
            HashSet<BlacklistGeneType> blCanGenerate = new HashSet<BlacklistGeneType>();
            HashSet<BlacklistGeneType> blCanRemove = new HashSet<BlacklistGeneType>();


            // For generate/mutate we want to avoid traits since they can cause pawn generation failures
            // because of worktags etc. Randomizing genes-for-traits causes a lot of crash-to-desktop problems.

            blCanGenerate.Add(BlacklistGeneType.BlImplanter);
            blCanGenerate.Add(BlacklistGeneType.BlReproduction);

            blCanMutate.Add(BlacklistGeneType.BlReproduction);
            blCanMutate.Add(BlacklistGeneType.BlPrereq);
            blCanMutate.Add(BlacklistGeneType.BlGenePack);
            // blWretch is generated from AlphaGenes' rules for the Random Mutation gene.
            blCanMutate.Add(BlacklistGeneType.BlWretch);

            blCanCopy.Add(BlacklistGeneType.BlGenePack);
            blCanCopy.Add(BlacklistGeneType.BlWretch);
            blCanCopy.Add(BlacklistGeneType.BlTrait);
            blCanCopy.Add(BlacklistGeneType.BlPrereq);

            blCanRemove.Add(BlacklistGeneType.BlAscension);
            blCanRemove.Add(BlacklistGeneType.BlMetamorph);
            blCanRemove.Add(BlacklistGeneType.BlDontCopy);
            blCanRemove.Add(BlacklistGeneType.BlPrereq);
            blCanRemove.Add(BlacklistGeneType.BlGenePack);
            blCanRemove.Add(BlacklistGeneType.BlWretch);
            blCanRemove.Add(BlacklistGeneType.BlTrait);

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

            List<string> myDontCopy = new List<string>();
            // core
            myDontCopy.Add("ViolenceDisabled");
            myDontCopy.Add("KindInstinct");
            myDontCopy.Add("XenogermReimplanter");
            // WVC
            myDontCopy.Add("WVC_Traitless");
            myDontCopy.Add("WVC_Chimera_NullifiedLimit");
            myDontCopy.Add("WVC_Chimera_GreatlyDecreasedLimit");
            myDontCopy.Add("WVC_Aptitudes_GreatEqualizer"); // this will let you pass it around the colony and scrub any aptitudes
            myDontCopy.Add("WVC_XenotypesAndGenes_RandomEndotypeForcer");
            myDontCopy.Add("WVC_XenotypesAndGenes_RandomXenotypeForcer");
            myDontCopy.Add("WVC_Hive"); // being able to get in on skill sharing, thoughts etc is too powerful
            myDontCopy.Add("WVC_Morph");
            myDontCopy.Add("WVC_Chimera_GenelineHiveMind");
            myDontCopy.Add("WVC_StartGestation");
            myDontCopy.Add("WVC_XenotypeGestator");
            myDontCopy.Add("WVC_StorageGestator");
            // VRE
            myDontCopy.Add("VRE_GermlineReimplanter");
            // AG
            myDontCopy.Add("AG_InsectStinger");
            myDontCopy.Add("AG_ParasiticStinger");
            myDontCopy.Add("AG_InsectStingerEndogenes");
            myDontCopy.Add("AG_ParasiticStingerEndogenes");
            myDontCopy.Add("AG_AsexualFission");
            myDontCopy.Add("BS_CellPandemonium");

            // Gene for Traits. I need to test if this still breaks things.
            myDontCopy.Add("Gene_Trait_");

            List<string> myDontRemove = new List<string>();
            myDontRemove.Add("WVC_Hive"); // needed to make the fluxspawn work
            myDontRemove.Add("BS_CellPandemonium");
            myDontRemove.Add("BS_Diet_Carnivore");
            myDontRemove.Add("BS_CannotWearClothingOrArmor");
            myDontRemove.Add("BS_NoEquip");

            List<string> myPreggoStrings = new List<string>();
            myPreggoStrings.Add("RS_MultiPregnancy");
            myPreggoStrings.Add("AG_AsexualFission");
            myPreggoStrings.Add("BS_SlimeProliferation");


            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefs)
            {
                BlacklistGene bl = new BlacklistGene(geneDef);
                if (geneDef.canGenerateInGeneSet == false)
                {
                    bl.Add(BlacklistGeneType.BlGenePack);
                }
                if (cachedBlacklist.Contains(geneDef) || cachedDefnameStrings.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlWretch);
                }
                if (myDontCopy.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlDontCopy);
                }

                if (myDontRemove.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlDontRemove);
                }

                if (myPreggoStrings.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlReproduction);
                }                
                if (geneDef.prerequisite != null)
                {
                    bl.Add(BlacklistGeneType.BlPrereq);
                }
                if (geneDef.displayCategory?.defName?.Contains("OMW_PerfectSilence") == true)
                {
                    bl.Add(BlacklistGeneType.BlSamhaphage);
                }
                if (geneDef.displayCategory?.defName?.Contains("Reimplanter") == true)
                {
                    bl.Add(BlacklistGeneType.BlImplanter);
                }
                if (geneDef.displayCategory?.defName?.Contains("Ascension") == true)
                {
                    bl.Add(BlacklistGeneType.BlAscension);
                }
                if (geneDef.displayCategory?.defName?.Contains("Metamorph") == true)
                {
                    bl.Add(BlacklistGeneType.BlMetamorph);
                }
                if (geneDef.displayCategory?.defName?.Contains("Reproduction") == true)
                {
                    bl.Add(BlacklistGeneType.BlReproduction);
                }
                if (geneDef.exclusionTags?.Contains("AG_OnlyOnCharacterCreation") == true)
                {
                    bl.Add(BlacklistGeneType.BlMisc);
                }
                if (geneDef.displayCategory?.defName?.Contains("BS_DO_NOT") == true)
                {
                    bl.Add(BlacklistGeneType.BlMisc);
                }

                if (geneDef.forcedTraits != null)
                {
                    foreach (GeneticTraitData traitData in geneDef.forcedTraits)
                    {
                        TraitDef traitDef = traitData.def;
                        if (traitDef == null) continue;

                        if (traitDef.conflictingTraits?.Count > 0)
                        {
                            bl.Add(BlacklistGeneType.BlTrait);
                            break;
                        }

                        if (traitDef.conflictingPassions?.Count > 0)
                        {
                            bl.Add(BlacklistGeneType.BlTrait);
                            break;
                        }

                        if (traitDef.requiredWorkTypes?.Count > 0)
                        {
                            bl.Add(BlacklistGeneType.BlTrait);
                            break;
                        }

                        if (traitDef.disabledWorkTypes?.Count > 0)
                        {
                            bl.Add(BlacklistGeneType.BlTrait);
                            break;
                        }
                    }
                }
                if (bl.BlacklistGeneType.Count > 0)
                {
                    bl.SetReason();

                    // all genes that can be blacklisted
                    BlacklistedGenes.Add(bl);

                    if (bl.BlacklistGeneType.Contains(BlacklistGeneType.BlReproduction))
                    {
                        PreggoGenes.Add(geneDef);
                    }

                    if (!bl.BlacklistGeneType.IsSubsetOf(blCanRemove))
                    {
                        BlacklistedGenesDontRemove.Add(geneDef);
                    }

                    if (!bl.BlacklistGeneType.IsSubsetOf(blCanCopy))
                    {
                        BlacklistedGenesDontCopy.Add(geneDef);
                    }

                    if (!bl.BlacklistGeneType.IsSubsetOf(blCanMutate))
                    {
                        BlacklistedGenesDontMutate.Add(geneDef);
                    }

                    if (!bl.BlacklistGeneType.IsSubsetOf(blCanGenerate))
                    {
                        BlacklistedGenesDontGenerate.Add(geneDef);
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
                sb.AppendLine("DEFNAME,LABEL,MOD,COMPLEXITY,METABOLISM,ARCHITE,CATEGORY,CATEGORYDEF,BLACKLISTED,HASHSETS,ABILITIES,DESCRIPTION");

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
                    string modName = gene.modContentPack?.Name ?? gene.modContentPack?.PackageId ?? "Unknown";

                    List<string> hashset = new List<string>();

                    if (BlacklistedGenesDontGenerate.Contains(gene))
                    {
                        hashset.Add("DontGenerate");
                    }
                    if (BlacklistedGenesDontCopy.Contains(gene))
                    {
                        hashset.Add("DontCopy");
                    }
                    if (BlacklistedGenesDontMutate.Contains(gene))
                    {
                        hashset.Add("DontMutate");
                    }
                    if (BlacklistedGenesDontRemove.Contains(gene))
                    {
                        hashset.Add("DontRemove");
                    }
                    if (PreggoGenes.Contains(gene))
                    {
                        hashset.Add("Preggo");
                    }
                    string hashsetStr = string.Join("|", hashset);
                    
                    sb.AppendLine($"\"{gene.defName}\",\"{label}\",\"{modName}\",{gene.biostatCpx},{gene.biostatMet},{gene.biostatArc},\"{cat}\",\"{catDef}\",{bl},{hashsetStr},\"{abilities}\",\"{desc}\"");
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