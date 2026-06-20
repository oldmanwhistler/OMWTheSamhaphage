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
        BlBanned,
        BlOutlandGenetics,
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

        public static readonly HashSet<GeneDef> SamhaphageGenes = new HashSet<GeneDef>();


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
            SamhaphageGenes.Clear();
           
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

            // ALWAYS EXCLUDE
            // BlacklistGeneType.BlSamhaphage - genes from the samhaphage xenos where removing them would really mess stuff up
            // BlacklistGeneType.BlImplanter - implantation genes which can make this race trivial
            // BlacklistGeneType.BlBanned - genes from other mods that aren't considered safe to use except for their specific race
            // BlacklistGeneType.BlMetamorph - WVC metamorph genes           

            // SOMETIMES EXCLUDE
            // BlacklistGeneType.BlGenePack -- prevent genes that aren't allowed to spawn in genepacks. This is usually how a mod says a gene can't be randomized.
            // BlacklistGeneType.BlWretch -- this a copy of AlphaGenes' blacklist for the wretch which contains genes that cause issues with random mutations
            // BlacklistGeneType.BlPrereq -- genes with pre-requisites on other genes. Will likely spawn disabled.
            // BlacklistGeneType.BlTrait -- genes that implement traits which can create pawngeneration loops and CTD if other traits or backstories create impossible to solve conditions.
            // BlacklistGeneType.BlOutlandGenetics -- outland genetics' ascension and morph system. Generates genes for every xeno in the game. Because these are randomly selected they usually are just unusable for Samhaphages.
            // BlacklistGeneType.BlReproduction -- baby making genes. Can break the reproductive aspect of the mod balance.
            // BlacklistGeneType.BlDontCopy -- prevent copying as it would trivialize game balance

            // The CanGenerate blacklist is only used with Genetic Drift            
            // blCanGenerate.Add(BlacklistGeneType.BlGenePack);
            // blCanGenerate.Add(BlacklistGeneType.BlWretch);
            // blCanGenerate.Add(BlacklistGeneType.BlPrereq);
            // blCanGenerate.Add(BlacklistGeneType.BlTrait);
            // blCanGenerate.Add(BlacklistGeneType.BlOutlandGenetics);           
            blCanGenerate.Add(BlacklistGeneType.BlReproduction);
            blCanGenerate.Add(BlacklistGeneType.BlDontCopy);
            blCanGenerate.Add(BlacklistGeneType.BlDontRemove);        

            // blCanMutate.Add(BlacklistGeneType.BlGenePack);
            // blCanMutate.Add(BlacklistGeneType.BlWretch);
            blCanMutate.Add(BlacklistGeneType.BlPrereq);
            // blCanMutate.Add(BlacklistGeneType.BlTrait);
            // blCanMutate.Add(BlacklistGeneType.BlOutlandGenetics);           
            blCanMutate.Add(BlacklistGeneType.BlReproduction);
            blCanMutate.Add(BlacklistGeneType.BlDontCopy);
            blCanMutate.Add(BlacklistGeneType.BlDontRemove);        

            blCanCopy.Add(BlacklistGeneType.BlGenePack);
            blCanCopy.Add(BlacklistGeneType.BlWretch);
            blCanCopy.Add(BlacklistGeneType.BlPrereq);
            blCanCopy.Add(BlacklistGeneType.BlTrait);
            blCanCopy.Add(BlacklistGeneType.BlOutlandGenetics);           
            // blCanCopy.Add(BlacklistGeneType.BlReproduction);
            // blCanCopy.Add(BlacklistGeneType.BlDontCopy);
            blCanCopy.Add(BlacklistGeneType.BlDontRemove);                    

            blCanRemove.Add(BlacklistGeneType.BlGenePack);
            blCanRemove.Add(BlacklistGeneType.BlWretch);
            blCanRemove.Add(BlacklistGeneType.BlPrereq);
            blCanRemove.Add(BlacklistGeneType.BlTrait);
            blCanRemove.Add(BlacklistGeneType.BlOutlandGenetics);           
            // blCanRemove.Add(BlacklistGeneType.BlReproduction);
            blCanRemove.Add(BlacklistGeneType.BlDontCopy);
            // blCanRemove.Add(BlacklistGeneType.BlDontRemove);        

            // AlphaGenes integration: wrespect the Wretch
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
            // WVC
            myDontCopy.Add("WVC_Traitless");
            myDontCopy.Add("WVC_Chimera_NullifiedLimit");
            myDontCopy.Add("WVC_Chimera_GreatlyDecreasedLimit");
            myDontCopy.Add("WVC_Aptitudes_GreatEqualizer"); // this will let you pass it around the colony and scrub any aptitudes
            myDontCopy.Add("WVC_Hive"); // being able to get in on skill sharing, thoughts etc is too powerful
            myDontCopy.Add("WVC_PsychicAbility_Hivemind");
            myDontCopy.Add("WVC_Morph");
            myDontCopy.Add("WVC_Chimera_GenelineHiveMind");
            // VRE
            // AG
            myDontCopy.Add("BS_CellPandemonium");
            // Gene for Traits. I need to test if this still breaks things.
            myDontCopy.Add("Gene_Trait_");
            myDontCopy.Add("Cannibal");
            myDontCopy.Add("BS_Diet_Carnivore");
            myDontCopy.Add("BS_OverrideDummyGene");
            myDontCopy.Add("BS_CannotWearClothingOrArmor");
            myDontCopy.Add("BS_NoEquip");




            List<string> myDontRemove = new List<string>();
            myDontRemove.Add("WVC_Hive"); // needed to make the fluxspawn work
            myDontRemove.Add("WVC_PsychicAbility_Hivemind");
            myDontRemove.Add("BS_CellPandemonium");
            myDontRemove.Add("BS_Diet_Carnivore");
            myDontRemove.Add("BS_CannotWearClothingOrArmor");
            myDontRemove.Add("BS_NoEquip");
            myDontRemove.Add("Cannibal");

            List<string> myPreggo = new List<string>();
            myPreggo.Add("RS_MultiPregnancy");
            myPreggo.Add("AG_AsexualFission");
            myPreggo.Add("BS_SlimeProliferation");
            myPreggo.Add("AG_InsectStinger");
            myPreggo.Add("AG_ParasiticStinger");
            myPreggo.Add("AG_AsexualFission");
            myPreggo.Add("WVC_StartGestation");
            myPreggo.Add("WVC_XenotypeGestator");
            myPreggo.Add("WVC_StorageGestator");
            myPreggo.Add("WVC_Dustogenic_ImmaculateConception");
            myPreggo.Add("BugParts_honeypot");


            List<string> myImplanter = new List<string>();
            myImplanter.Add("XenogermReimplanter");
            myImplanter.Add("VRE_GermlineReimplanter");
            myImplanter.Add("AG_InsectStingerEndogenes");
            myImplanter.Add("AG_ParasiticStingerEndogenes");
            myImplanter.Add("WVC_XenotypesAndGenes_RandomEndotypeForcer");
            myImplanter.Add("WVC_XenotypesAndGenes_RandomXenotypeForcer");


            foreach (GeneDef geneDef in DefDatabase<GeneDef>.AllDefs)
            {
                BlacklistGene bl = new BlacklistGene(geneDef);
                string modName = geneDef.modContentPack?.Name ?? geneDef.modContentPack?.PackageId ?? "Unknown";

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

                if (myPreggo.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlReproduction);
                } 
                if (myImplanter.Any(s => geneDef.defName.Contains(s)))
                {
                    bl.Add(BlacklistGeneType.BlImplanter);
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
                    bl.Add(BlacklistGeneType.BlOutlandGenetics);
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
                    bl.Add(BlacklistGeneType.BlBanned);
                }
                if (geneDef.displayCategory?.defName == "SZSpecial")
                {
                    bl.Add(BlacklistGeneType.BlBanned);
                }                
                if (geneDef.displayCategory?.defName == "BS_DO_NOT")
                {
                    bl.Add(BlacklistGeneType.BlBanned);
                }
                if (geneDef.displayCategory?.defName == "Body_Size")
                {
                    // all of these conflict with Soul Form
                    bl.Add(BlacklistGeneType.BlDontCopy);
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

                    if (bl.BlacklistGeneType.Contains(BlacklistGeneType.BlSamhaphage))
                    {
                        SamhaphageGenes.Add(geneDef);
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