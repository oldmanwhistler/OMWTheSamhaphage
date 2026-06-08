using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public static class OMWGenes
    {
        static Logger Log = new Logger("Genes");

        public static void Refresh(Pawn pawn)
        {
            if (pawn == null) return;

            pawn.needs?.AddOrRemoveNeedsAsAppropriate();
            pawn.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            pawn.health?.hediffSet?.DirtyCache();
            pawn.skills?.DirtyAptitudes();
            pawn.Notify_DisabledWorkTypesChanged();
            // This forces the game to re-evaluate the pawn's graphics and stats based on their current genes
            pawn.Drawer?.renderer?.SetAllGraphicsDirty();

            Log.Debug($"{pawn.LabelShort}.Refresh: Graphics and stats should now reflect current genes");
        }

        public static int CountXenogenes(Pawn pawn)
        {
            return pawn.genes?.Xenogenes.Count ?? 0;
        }

        public static int CountEndogenes(Pawn pawn)
        {
            return pawn.genes?.Endogenes.Count ?? 0;
        }

        public static int CalculateComplexity(XenotypeDef xenotype)
        {
            int complexity = 0;
            foreach (GeneDef geneDef in xenotype.AllGenes)
            {
                complexity += geneDef.biostatCpx;
            }

            return complexity;
        }

        public static int CalculateComplexity(Pawn pawn)
        {
            if (pawn?.genes == null) return 0;

            int complexity = 0;
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                complexity += gene.def.biostatCpx;
            }
            return complexity;
        }

        public static int CalculateMetabolism(Pawn pawn)
        {
            if (pawn?.genes == null) return 0;

            int metabolism = 0;
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                metabolism += gene.def.biostatMet;
            }
            return metabolism;
        }
   

        public static void XenogenesToEndogenes(Pawn pawn)        
        {
            if (pawn?.genes == null) return;

            List<Gene> xenoList = pawn.genes.Xenogenes;
            List<Gene> endoList = pawn.genes.Endogenes;

            if (xenoList.Count == 0) return;

            // Move the actual Gene objects directly between lists.
            // This bypasses the Add/Remove lifecycle (PostAdd/PostRemove).
            for (int i = xenoList.Count - 1; i >= 0; i--)
            {
                Gene gene = xenoList[i];
                xenoList.RemoveAt(i);
                endoList.Insert(0, gene); // Prepending as per your original logic
            }

            Refresh(pawn);
            Log.Debug($"{pawn.LabelShort}.XenogenesToEndogenes: Manually moved xenogenes to endogenes.");
        }

        public static void PrependXenogenes(Pawn pawn, List<GeneDef> genesToAdd)
        {
            if (pawn?.genes == null || genesToAdd.NullOrEmpty()) return;

            List<Gene> xenoList = pawn.genes.Xenogenes;

            // 1. Add new genes normally (they append to the end)
            foreach (GeneDef geneDef in genesToAdd)
            {
                pawn.genes.AddGene(geneDef, xenogene: true);
            }

            // 2. Reorder: Move the newly added genes from the back to the front.
            // This preserves the existing Gene instances and their internal state.
            for (int i = 0; i < genesToAdd.Count; i++)
            {
                Gene added = xenoList[xenoList.Count - 1];
                xenoList.RemoveAt(xenoList.Count - 1);
                xenoList.Insert(0, added);
            }

            Refresh(pawn);
            Log.Debug($"{pawn.LabelShort}.PrependXenogenes: Prepended {genesToAdd.Count} xenogenes.");
        }
        
        public static void AddXenotype(Pawn pawn, XenotypeDef targeXenotype)
        {
            if (pawn?.genes == null || targeXenotype == null) return;

            List<Gene> xenoList = pawn.genes.Xenogenes;

            int count = 0;
            // 2. Add the genes from the xenotype
            foreach (GeneDef geneDef in targeXenotype.AllGenes)
            {
                if (!pawn.genes.HasActiveGene(geneDef))
                {
                    pawn.genes.AddGene(geneDef, xenogene: true);
                    count++;
                }
            }

            if (count > 0)
            {
                int numAdded = count;
                for (int i = 0; i < numAdded; i++)
                {
                    Gene added = xenoList[xenoList.Count - 1];
                    xenoList.RemoveAt(xenoList.Count - 1);
                    xenoList.Insert(0, added);
                }

                pawn.genes.SetXenotypeDirect(targeXenotype);
                Refresh(pawn);
                Log.Debug(
                    $"{pawn.LabelShort}.AddXenotype: Applied xenotype {targeXenotype.defName} while preserving existing gene state.");
            }
            else
            {
                Log.Debug(
                    $"{pawn.LabelShort}.AddXenotype: tried to apply xenotype {targeXenotype.defName} but no genes were added.");
            }
        }

        private static void RemoveXenotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype)
        {
            if (pawn == null || sourceXenotype == null) return;

            int count = 0;
            foreach (GeneDef geneDef in sourceXenotype.AllGenes)
            {
                Gene gene = pawn.genes.GetGene(geneDef);
                if (gene != null)
                {
                    // only remove the source xenotype genes that the target xenotype doesn't have
                    // this is to avoid retriggering the PostAdd / PostRemove for the genes.
                    // Defensive: check for null targetXenotype
                    if (targetXenotype == null || !targetXenotype.AllGenes.Contains(gene.def))
                    {
                        pawn.genes.RemoveGene(gene);
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                // this sets the xenotype name
                pawn.genes.SetXenotypeDirect(null);
                Refresh(pawn);
                Log.Debug($"{pawn.LabelShort}.RemoveXenotype: Removed {count} genes and xenotype {sourceXenotype.LabelCap}");
            }
            else {
                Log.Debug($"{pawn.LabelShort}.RemoveXenotype: no genes to remove from xenotype {sourceXenotype.LabelCap} because they are all in {targetXenotype.LabelCap}");                                
            }
        }


        public static bool CanChangeXenotype(Pawn pawn, XenotypeDef targetXenotype, out string reason)
        {
            reason = "unknown reason";
            
            if (pawn == null) return false;
            if (targetXenotype == null)
            {
                reason = "targetXenotype is null";
                return false;
            }

            if (pawn.genes == null)
            {
                reason = $"{pawn.LabelShort} has pawn.genes == null";
                return false;
            }

            if (pawn.genes?.Xenotype == targetXenotype)
            {
                reason = $"{pawn.LabelShort} is already {targetXenotype}";
                return false;
            }

            if (targetXenotype == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                reason = $"There already is a Sovereign Stillness and there can only be one.";
                return !OMWXenotypes.IsSovereignStillnessInPlayerFaction();
            }
            return true;
        }
        
        public static bool ChangeXenotype(Pawn pawn, XenotypeDef targetXenotype, bool removeSourceXenotype = true)
        {
            string reason;
            if (!CanChangeXenotype(pawn, targetXenotype, out reason))
            {
                Log.Error($"{pawn.LabelShort}.ChangeXenotype not possible: {reason}");
                return false;
            }

            XenotypeDef sourceXenotype = pawn.genes?.Xenotype;

            Log.Debug($"{pawn.LabelShort}.ChangeXenotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            
            if (removeSourceXenotype && (sourceXenotype != null)) RemoveXenotype(pawn, sourceXenotype, targetXenotype);
            XenogenesToEndogenes(pawn);
            if (targetXenotype != null) AddXenotype(pawn, targetXenotype);

            Log.Debug(
                $"{pawn.LabelShort}.ChangeXenotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            return true;
        }

        public static bool ChangeEndotype(Pawn pawn, XenotypeDef targetXenotype)
        {
            string reason;
            if (!CanChangeXenotype(pawn, targetXenotype, out reason))
            {
                Log.Error($"{pawn.LabelShort}.ChangeXenotype not possible: {reason}");
                return false;
            }


            XenotypeDef sourceXenotype = pawn.genes?.Xenotype;

            Log.Debug($"{pawn.LabelShort}.ChangeEndotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            ChangeXenotype(pawn, targetXenotype);
            XenogenesToEndogenes(pawn);
            
            Log.Debug($"{pawn.LabelShort}.ChangeEndotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            return true;
        }        

        public static bool HasNullThrum(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_NullThrum)) return true;
            return false;
        }

        public static bool HasScouredMind(Pawn pawn)
        {
            if (pawn == null) return false;
            // don't flatten dead pawns
            if (pawn.Dead) return true;
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind)) return true;
            return false;
        }

        // GeneticDissonance prevents repeatedly using the same abilities on the same pawn
        public static void ApplyDissonance(Pawn victim, Pawn caster)
        {
            if (OMW_Mod.settings.disableDissonance) return;
            Log.Debug($"ApplyDissonance({victim.LabelShort}, {caster.LabelShort})");
            if (!victim.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                Hediff hediffDissonance = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_GeneticDissonance, caster);
                victim.health.AddHediff(hediffDissonance);
            }
        }
    }
}