using System.Collections.Generic;
using System.Net.Sockets;
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

        public static void XenogenesToEndogenes(Pawn pawn)        
        {
            if (pawn == null || pawn.genes == null) return;

            if (pawn.genes.Xenogenes.Count == 0) return;

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

        public static void PrependXenogenes(Pawn pawn, List<GeneDef> genesToAdd)
        {
            if (pawn == null || genesToAdd.Count == 0) return;

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: pawn has {pawn.genes.Xenogenes.Count}");

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: step 1");

            List<Gene> genesToRemove = new List<Gene>();
            foreach (Gene xenoGene in pawn.genes.Xenogenes)
            {
                genesToRemove.Add(xenoGene);
            }
            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: step 2");

            foreach (Gene gene in genesToRemove)
            {
                pawn.genes.RemoveGene(gene);
            }

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: step 3");

            int count = 0;
            foreach (GeneDef geneDef in genesToAdd)
            {
                pawn.genes.AddGene(geneDef, xenogene: true);
                count++;
            }

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: step 4");

            foreach (Gene gene in genesToRemove)
            {
                pawn.genes.AddGene(gene.def, xenogene: true);
            }

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: step 5");

            if (debug) Log.Message($"{pawn.LabelShort}.PrependXenogenes: Added {count} genes to the front of xenogenes");
            Refresh(pawn);
        }
        
        public static void AddXenotype(Pawn pawn, XenotypeDef xenotype)
        {
            if (pawn == null || xenotype == null) return;

            // Update the label so the UI shows the correct Xenotype name
            pawn.genes.SetXenotypeDirect(xenotype);

            List<Gene> genesToRemove = new List<Gene>();
            foreach (Gene xenoGene in pawn.genes.Xenogenes)
            {
                genesToRemove.Add(xenoGene);
            }

            foreach (Gene gene in genesToRemove)
            {
                pawn.genes.RemoveGene(gene);
            }     

            int count = 0;            
            // Add all genes from the new xenotype as Xenogenes
            foreach (GeneDef geneDef in xenotype.AllGenes)
            {
                pawn.genes.AddGene(geneDef, xenogene: true);
                count++;
            }
            foreach (Gene gene in genesToRemove)
            {
                pawn.genes.AddGene(gene.def, xenogene: true);
            }     

            Refresh(pawn);
            if (debug) Log.Message($"{pawn.LabelShort}.AddXenotype: Added {count} genes from xenotype {xenotype.LabelCap} to the front of xenogenes");
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

            // this sets the xenotype name
            pawn.genes.SetXenotypeDirect(null);
            Refresh(pawn);
            if (debug) Log.Message($"{pawn.LabelShort}.RemoveXenotype: Removed {count} genes and xenotype {xenotype.LabelCap}");
        }


        public static bool CanChangeXenotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype, bool sendMsg=false)
        {
            if (pawn == null) return false;

            // Guess the xenotype
            if (sourceXenotype == null) sourceXenotype = pawn.genes?.Xenotype;

            if (sourceXenotype == targetXenotype)
            {
                if (sendMsg)
                {
                    Messages.Message($"{pawn.LabelShort} is already targetXenotype", MessageTypeDefOf.RejectInput);
                }
                return false;
            }
            return true;
        }

        public static bool ChangeXenotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype)
        {
            if (sourceXenotype == null) sourceXenotype = pawn.genes?.Xenotype;
            if (!CanChangeXenotype(pawn, sourceXenotype, targetXenotype, true)) return false;
            // Guess the xenotype

            if (debug) Log.Message($"{pawn.LabelShort}.ChangeXenotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            if (sourceXenotype != null) RemoveXenotype(pawn, sourceXenotype);
            XenogenesToEndogenes(pawn);
            if (targetXenotype != null) AddXenotype(pawn, targetXenotype);
            if (debug)
                Log.Message(
                    $"{pawn.LabelShort}.ChangeXenotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            return true;
        }

        public static bool ChangeEndotype(Pawn pawn, XenotypeDef sourceXenotype, XenotypeDef targetXenotype)
        {
            if (!CanChangeXenotype(pawn, sourceXenotype, targetXenotype, true)) return false;
            // Guess the xenotype
            if (sourceXenotype == null) sourceXenotype = pawn.genes?.Xenotype;           
            if (debug) Log.Message($"{pawn.LabelShort}.ChangeEndotype: Start changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
            ChangeXenotype(pawn, sourceXenotype, targetXenotype);
            XenogenesToEndogenes(pawn);
            if (debug)
                Log.Message(
                    $"{pawn.LabelShort}.ChangeEndotype: Done changing from {sourceXenotype?.LabelCap ?? "null"} to {targetXenotype?.LabelCap ?? "null"}");
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
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_ScouredMind)) return true;
            return false;
        }

    }
}