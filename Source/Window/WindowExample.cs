using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

// Google Gemini example

namespace OMW_Samhaphage
{
public class Dialog_GeneSelection : Window
    {
        private List<Gene> availableGenes;
        private Pawn collector;
        private Vector2 scrollPosition;

        // Set fixed size for the window
        public override Vector2 InitialSize => new Vector2(500f, 400f);

        public Dialog_GeneSelection(Pawn collector, List<Gene> genes)
        {
            this.collector = collector;
            this.availableGenes = genes;
            this.forcePause = true; // Pause game while selecting
            this.closeOnClickedOutside = false;
            this.doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 35f), "Select Genetic Essence");
            Text.Font = GameFont.Small;

            Rect outRect = new Rect(0, 40f, inRect.width, inRect.height - 100f);
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, availableGenes.Count * 35f);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float num = 0f;

            foreach (Gene gene in availableGenes)
            {
                Rect rowRect = new Rect(0, num, viewRect.width, 30f);
                if (Widgets.ButtonText(rowRect, gene.LabelCap))
                {
                    ApplyGeneEffect(gene);
                    Close();
                }
                num += 35f;
            }

            Widgets.EndScrollView();
        }

        private void ApplyGeneEffect(Gene gene)
        {
            Messages.Message($"Successfully integrated {gene.Label} into the hive mind.", MessageTypeDefOf.PositiveEvent);
            // Add your logic here to give the gene to the collector or process essence
        }
    }
}
