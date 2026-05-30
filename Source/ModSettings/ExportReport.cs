using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RimWorld;
using UnityEngine;
using System.Xml.Linq; // Added for XML generation
using System.Linq; // Added for LINQ operations
using Verse;

namespace OMW_Samhaphage
{

    public static class ExportReport
    {
        private static string Prefix = "[SAMHAPHAGE-REPORTS]";
        private static void Gene()
        {
            try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath,
                    "OMW_Samhaphage_Report_Resonance_Genes.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine(
                    "DEFNAME,LABEL,COMP,META,ARCH,MVF,PV,RES,CATEGORY,CATEGORYDEF,GENESET,ABILITIES,DESCRIPTION");

                foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
                {
                    // skip over the stuff I'm not going to allow people to copy
                    BlacklistGene blEntry = OMW_BlacklistGenes.BlacklistedGenes.FirstOrDefault(x => x.geneDef == gene);
                    if (blEntry != null) continue;

                    string label = gene.label?.Replace("\"", "\"\"") ?? "";
                    string cat = gene.displayCategory?.label?.Replace("\"", "\"\"") ?? "None";
                    string catDef = gene.displayCategory?.defName ?? "None";
                    float powerValue = ResonanceUtility.CalculateGenePowerValue(gene);
                    float totalResonanceValue = ResonanceUtility.GeneResonanceValue(gene);

                    string abilities = (gene.abilities != null && gene.abilities.Count > 0)
                        ? string.Join("|", gene.abilities.ConvertAll(a => a.defName))
                        : "";
                    string desc = gene.description?.Replace("\n", " ").Replace("\r", "").Replace(",", " ")
                        .Replace("\"", "") ?? "";

                    sb.AppendLine(
                        $"\"{gene.defName}\",\"{label}\",{gene.biostatCpx},{gene.biostatMet},{gene.biostatArc},{gene.marketValueFactor},{powerValue},{totalResonanceValue},\"{cat}\",\"{catDef}\",{gene.canGenerateInGeneSet},\"{abilities}\",\"{desc}\"");
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported resonance report to {path}");
                Messages.Message($"{Prefix} Exported resonance report to {path}", MessageTypeDefOf.TaskCompletion,
                    false);
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export genes: " + ex.Message);
            }
        }

        private static void Trait()
        {
            try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath,
                    "OMW_Samhaphage_Report_Resonance_Traits.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine(
                    "DEFNAME,LABEL,PV,RES,DEGREE,MVO,OFFSET_SUM,FACTOR_SUM,SKILL_SUM,HUNGER,PAIN_OFFSET,PAIN_FACTOR,STATOFFSETS,STATFACTORS,SKILLGAINS,DESCRIPTION");

                foreach (TraitDef trait in DefDatabase<TraitDef>.AllDefs)
                {
                    foreach (TraitDegreeData degree in trait.degreeDatas)
                    {
                        string label = degree.label?.Replace("\"", "\"\"") ?? "";
                        float marketValueOffset = degree.marketValueFactorOffset;
                        float powerValue = ResonanceUtility.CalculateTraitPowerValue(degree);
                        float totalResonanceValue = ResonanceUtility.TraitResonanceValue(degree);

                        float offsetSum = ResonanceUtility.CalculateTraitOffsetSum(degree);
                        float factorSum = ResonanceUtility.CalculateTraitFactorSum(degree);
                        int skillSum = degree.skillGains.NullOrEmpty() ? 0 : degree.skillGains.Sum(s => 10 * s.amount);

                        string statsO = degree.statOffsets.NullOrEmpty()
                            ? ""
                            : string.Join("|", degree.statOffsets.ConvertAll(s => $"{s.stat.defName}:{s.value}"));
                        string statsF = degree.statFactors.NullOrEmpty()
                            ? ""
                            : string.Join("|", degree.statFactors.ConvertAll(s => $"{s.stat.defName}:{s.value}"));
                        string skills = degree.skillGains.NullOrEmpty()
                            ? ""
                            : string.Join("|", degree.skillGains.ConvertAll(s => $"{s.skill.defName}:{5 * s.amount}"));
                        string desc = degree.description?.Replace("\n", " ").Replace("\r", "").Replace("\"", "\"\"") ??
                                      "";

                        float hungerFactor = ResonanceUtility.TraitNormalize(degree.hungerRateFactor);
                        float painOffset = ResonanceUtility.TraitNormalize(degree.painOffset);
                        float painFactor = ResonanceUtility.TraitNormalize(degree.painFactor);

                        sb.AppendLine(
                            $"\"{trait.defName}\",\"{label}\",{powerValue},{totalResonanceValue},{degree.degree},{marketValueOffset},{offsetSum},{factorSum},{skillSum},{hungerFactor},{painOffset},{painFactor},\"{statsO}\",\"{statsF}\",\"{skills}\",\"{desc}\"");
                    }
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported trait resonance report to {path}");
                Messages.Message($"{Prefix} Exported resonance report to {path}", MessageTypeDefOf.TaskCompletion,
                    false);
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export traits: " + ex.Message);
            }
        }

        private static void Resonance()
        {
            try
            {
                string path = Path.Combine(GenFilePaths.SaveDataFolderPath,
                    "OMW_Samhaphage_Report_Resonance_Both.csv");
                StringBuilder sb = new StringBuilder();

                // Header row
                sb.AppendLine("TYPE,DEFNAME,PV,RES,DESCRIPTION");
                foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
                {
                    float powerValue = ResonanceUtility.CalculateGenePowerValue(gene);
                    float totalResonanceValue = ResonanceUtility.GeneResonanceValue(gene);
                    string desc = gene.description?.Replace("\n", " ").Replace("\r", "").Replace(",", " ")
                        .Replace("\"", "") ?? "";

                    sb.AppendLine(
                        $"GENE,\"{gene.defName}\",{powerValue},{totalResonanceValue},\"{desc}\"");
                }

                foreach (TraitDef trait in DefDatabase<TraitDef>.AllDefs)
                {
                    foreach (TraitDegreeData degree in trait.degreeDatas)
                    {
                        float powerValue = ResonanceUtility.CalculateTraitPowerValue(degree);
                        float totalResonanceValue = ResonanceUtility.TraitResonanceValue(degree);
                        string desc = degree.description?.Replace("\n", " ").Replace("\r", "").Replace("\"", "\"\"") ??
                                      "";

                        sb.AppendLine(
                            $"TRAIT, \"{trait.defName}\",{powerValue},{totalResonanceValue},\"{desc}\"");
                    }
                }

                File.WriteAllText(path, sb.ToString());
                Log.Message($"{Prefix} Exported trait resonance report to {path}");
                Messages.Message($"{Prefix} Exported resonance report to {path}", MessageTypeDefOf.TaskCompletion,
                    false);
            }
            catch (System.Exception ex)
            {
                Log.Error($"{Prefix} Error: Failed to export traits: " + ex.Message);
            }
        }


        public static void ExportReportsResonance()
        {
            Gene();
            Trait();
            Resonance();
        }
    }
}