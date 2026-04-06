using RimWorld;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{

    // This is vibe-coded from Google Gemini
    public class Dialog_SelectMultipleGeneInstances : Window
    {
        private List<Gene> geneOptions;
        private HashSet<Gene> selectedGenes = new HashSet<Gene>();
        private System.Action<List<Gene>> onConfirm;
        private int maxSelection;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(450f, 700f);

        public Dialog_SelectMultipleGeneInstances(List<Gene> options, int max, System.Action<List<Gene>> callback)
        {
            this.geneOptions = options;
            this.maxSelection = max;
            this.onConfirm = callback;

            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            Text.Font = GameFont.Medium;
            listing.Label($"Select Active Genes ({selectedGenes.Count} / {maxSelection})");
            Text.Font = GameFont.Small;
            listing.GapLine();
            listing.End();

            float footerHeight = 50f;
            Rect scrollRect = new Rect(0f, listing.CurHeight, inRect.width,
                inRect.height - listing.CurHeight - footerHeight);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, geneOptions.Count * 38f);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (Gene gene in geneOptions)
            {
                Rect rowRect = new Rect(0f, curY, viewRect.width, 36f); // Slightly taller for padding
                bool isSelected = selectedGenes.Contains(gene);

                // 1. Draw the Background / Hover States
                if (isSelected)
                {
                    Widgets.DrawHighlightSelected(rowRect);
                }
                else
                {
                    Widgets.DrawHighlightIfMouseover(rowRect);
                }

                // Attach the Tooltip
                // This triggers when the mouse pauses over the rowRect
                TooltipHandler.TipRegion(rowRect, new TipSignal(() =>
                    $"{gene.LabelCap}\n\n{gene.def.description}", gene.GetHashCode()));
                
                // Draw the Content (Icon + Label)
                // We offset the icon slightly from the edge
                Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f);
                Widgets.DefIcon(iconRect, gene.def);

                Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 80f, rowRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;

                // Dim the text if the gene is overridden (standard RimWorld UI behavior)
                if (gene.Overridden) GUI.color = Color.gray;
                Widgets.Label(labelRect, gene.LabelCap);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                // Draw the Checkbox (Visual only, the button handles the logic)
                Widgets.Checkbox(new Vector2(rowRect.xMax - 30f, rowRect.y + 6f), ref isSelected, 24f, false);

                // The "Big Button" - This covers the whole row, including the icon
                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (isSelected)
                    {
                        selectedGenes.Remove(gene);
                    }
                    else if (selectedGenes.Count < maxSelection)
                    {
                        selectedGenes.Add(gene);
                        // Play the standard "click" sound for feedback
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                    else
                    {
                        // Optional: Play a "reject" sound if they try to exceed the limit
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                }

                curY += 40f; // Row height + spacing
            }

            Widgets.EndScrollView();

            // Footer Action
            if (Widgets.ButtonText(new Rect(0f, inRect.height - 40f, inRect.width, 35f), "Confirm Selection"))
            {
                onConfirm?.Invoke(selectedGenes.ToList());
                Close();
            }
        }
    }
}