using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public static class OMWGenes
    {
        private static bool debug = true;

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

            if (debug) Log.Message($"{pawn.LabelShort}.Refresh: Graphics and stats should now reflect current genes");
        }

        public static int CountXenogenes(Pawn pawn)
        {
            return pawn.genes?.Xenogenes.Count ?? 0;
        }

        public static int CountEndogenes(Pawn pawn)
        {
            return pawn.genes?.Endogenes.Count ?? 0;
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

        public static void RemoveDisabledGenes(Pawn pawn)
        {
            if (pawn?.genes == null) return;

            List<Gene> genes = pawn.genes.GenesListForReading;

            int count = 0;
            // We start at the last index and move toward 0
            for (int i = genes.Count - 1; i >= 0; i--)
            {
                Gene currentGene = genes[i];
                if (!pawn.genes.HasActiveGene(currentGene.def))
                {
                    count++;
                    pawn.genes.RemoveGene(currentGene);
                }
            }
            if (debug) Log.Message($"{pawn.LabelShort}.RemoveDisabledGenes: Removed {count} disabled genes");
        }

        private static void PrependXenotypeGenesToEndogenes(Pawn pawn, XenotypeDef xenotype)
        {
            List<GeneDef> genesToAdd = xenotype.AllGenes;
            PrependGenesToEndogenes(pawn, genesToAdd);
        }

        private static void PrependGenesToEndogenes(Pawn pawn, List<GeneDef> genesToAdd)
        {
            List<Gene> oldGenes = pawn.genes.GenesListForReading;

            // We start at the last index and move toward 0
            for (int i = oldGenes.Count - 1; i >= 0; i--)
            {
                Gene gene = oldGenes[i];
                genesToAdd.Add(gene.def);
                pawn.genes.RemoveGene(gene);
            }

            foreach (GeneDef gene in genesToAdd)
            {
                pawn.genes.AddGene(gene, false);
            }
        }        

        public static void XenogenesToEndogenes(Pawn pawn, bool removeDisabledFirst=true)        
        {
            if (pawn == null || pawn.genes == null) return;

            if (pawn.genes.Xenogenes.Count == 0) return;

            if (removeDisabledFirst)
            {
                RemoveDisabledGenes(pawn);
            }

            // 1. Snapshot the Xenogenes (copy to avoid modification-during-enumeration errors)
            List<GeneDef> genesToMove = new List<GeneDef>();
            foreach (Gene xenoGene in pawn.genes.Xenogenes)
            {
                genesToMove.Add(xenoGene.def);
            }

            // 2. Remove all Xenogenes
            pawn.genes.ClearXenogenes();

            PrependGenesToEndogenes(pawn, genesToMove);

            if (debug) Log.Message($"{pawn.LabelShort}.XenogenesToEndogenes: Moved {genesToMove.Count} xenogenes to endogenes");
        }


        public static bool RemoveXenogenes(Pawn source)
        {
            if (source == null || source.genes == null)
                return false;

            List<Gene> genesToRemove = new List<Gene>();
            foreach (Gene xenoGene in source.genes.Xenogenes)
            {
                genesToRemove.Add(xenoGene);
            }

            // We start at the last index and move toward 0
            foreach (Gene gene in genesToRemove)
            {
                source.genes.RemoveGene(gene);
            }            

            OMWGenes.Refresh(source);

            if (debug)
                Log.Message(
                    $"{source.LabelShort}.RemoveXenogenes: Removed {genesToRemove.Count} xenogenes");
            return true;
        }

        public static bool CopyXenogenes(Pawn source, Pawn destination)
        {
            if (source == null || destination == null || source.genes == null || destination.genes == null) return false;

            List<GeneDef> genesToCopy = new List<GeneDef>();
            foreach (Gene xenoGene in source.genes.Xenogenes)
            {
                genesToCopy.Add(xenoGene.def);
            }

            foreach (GeneDef geneDef in genesToCopy)
            {
                destination.genes.AddGene(geneDef, true);
            }

            source.genes.ClearXenogenes();
            OMWGenes.Refresh(source);
            OMWGenes.Refresh(destination);

            if (debug) Log.Message($"{source.LabelShort}.CopyXenogenes to {destination.LabelShort}: Copied {genesToCopy.Count} xenogenes");
            return true;
        }

        public static bool StealXenogenes(Pawn source, Pawn destination)
        {
            if (debug)
                Log.Message(
                    $"{destination.LabelShort}.TakeXenogenes from {destination.LabelShort}: Start");

            if (!CopyXenogenes(source, destination)) return false;
            if (!RemoveXenogenes(source)) return false;

            if (debug)
                Log.Message(
                    $"{destination.LabelShort}.TakeXenogenes from {destination.LabelShort}: Done");
            return true;
        }        

        public static void AddXenotype(Pawn pawn, XenotypeDef xenotype)
        {
            if (pawn == null || xenotype == null) return;

            // Clear existing xenogenes first if you want a clean override
            if (pawn?.genes != null)
            {
                pawn.genes.ClearXenogenes();
            }

            // Update the label so the UI shows the correct Xenotype name
            pawn.genes.SetXenotypeDirect(xenotype);

            int count = 0;            
            // Add all genes from the new xenotype as Xenogenes
            foreach (GeneDef geneDef in xenotype.AllGenes)
            {
                pawn.genes.AddGene(geneDef, xenogene: true);
                count++;
            }

            Refresh(pawn);
            if (debug) Log.Message($"{pawn.LabelShort}.AddXenotype: Added {count} genes from xenotype {xenotype.LabelCap}");
        }

        public static void RemoveXenotype(Pawn pawn, XenotypeDef xenotype)
        {
            if (pawn == null || xenotype == null) return;

            int count = 0;
            foreach (GeneDef geneDef in xenotype.AllGenes)
            {
                Gene gene = pawn.genes.GetGene(geneDef);
                if (gene != null)
                {
                    pawn.genes.RemoveGene(gene);
                    count++;
                }                
            }

            pawn.genes.SetXenotypeDirect(null);
            Refresh(pawn);
            if (debug) Log.Message($"{pawn.LabelShort}.RemoveXenotype: Removed {count} genes and xenotype {xenotype.LabelCap}");
        }


        public static void ChangeXenotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype)
        {
            if (pawn == null) return;

            // Guess the xenotype
            if (sourceXenotype == null) sourceXenotype = pawn.genes?.Xenotype;

            if (debug) Log.Message($"{pawn.LabelShort}.ChangeXenotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            if (sourceXenotype != null) RemoveXenotype(pawn, sourceXenotype);
            RemoveDisabledGenes(pawn);
            XenogenesToEndogenes(pawn);
            if (targetXenotype != null) AddXenotype(pawn, targetXenotype);
            RemoveDisabledGenes(pawn);
            if (debug)
                Log.Message(
                    $"{pawn.LabelShort}.ChangeXenotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
        }

        public static void ChangeEndotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype)
        {
            if (pawn == null) return;

            // Guess the xenotype
            if (sourceXenotype == null) sourceXenotype = pawn.genes?.Xenotype;
            
            if (debug)
                Log.Message(
                    $"{pawn.LabelShort}.ChangeEndotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            ChangeXenotype(pawn, sourceXenotype, targetXenotype);
            XenogenesToEndogenes(pawn);
            RemoveDisabledGenes(pawn);
            if (debug)
                Log.Message(
                    $"{pawn.LabelShort}.ChangeEndotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
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
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind)) return true;
            return false;
        }
    }
}