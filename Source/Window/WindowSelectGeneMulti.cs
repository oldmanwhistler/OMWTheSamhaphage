using RimWorld;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public class Dialog_SelectMultipleGeneInstances : Window
    {
        private List<Gene> geneOptions;
        private List<Gene> referenceGenes; // The "Second List" to check conflicts against
        private HashSet<Gene> selectedGenes = new HashSet<Gene>();
        private System.Action<List<Gene>> onConfirm;
        private int maxSelection;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(450f, 700f);

        private string windowVerb;

        // Updated Constructor to accept the second list
        public Dialog_SelectMultipleGeneInstances(List<Gene> options, List<Gene> referenceList, int max, string verb,
            System.Action<List<Gene>> callback)
        {
            this.geneOptions = options
                .Where(g => !OMW_BlacklistGenes.BlacklistedGenes.Contains(g.def))
                .OrderBy(g => g.pawn.genes.HasXenogene(g.def))
                .ToList();
            this.referenceGenes = referenceList ?? new List<Gene>();
            this.maxSelection = max;
            this.windowVerb = verb;
            this.onConfirm = callback;

            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
        }

        // Helper to check if a gene conflicts with anything in the reference list
        private string GetConflictingGeneName(Gene gene)
        {
            if (referenceGenes.Count == 0) return null;

            foreach (Gene refGene in referenceGenes)
            {
                // Check if it's the exact same gene or part of a conflicting group (e.g. both are skin colors)
                if (gene.def == refGene.def || gene.def.ConflictsWith(refGene.def))
                {
                    return refGene.LabelCap; // Returns "Strong Melee", "Blue Skin", etc.
                }
            }

            return null;
        }

        private string GetOverridingGeneName(Gene gene)
        {
            if (geneOptions.Count == 0) return null;
            if (gene.Overridden == false) return null;

            foreach (Gene refGene in geneOptions)
            {
                // Check if it's the exact same gene or part of a conflicting group (e.g. both are skin colors)
                if (gene.def == refGene.def || gene.def.ConflictsWith(refGene.def))
                {
                    return refGene.LabelCap; // Returns "Strong Melee", "Blue Skin", etc.
                }
            }

            return null;
        }


        public override void DoWindowContents(Rect inRect)
        {
            // --- Header ---
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, 40f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, $"Select Genes to {windowVerb} ({selectedGenes.Count} / {maxSelection})");

            // Clear All Button
            Rect clearBtnRect = new Rect(inRect.width - 100f, inRect.y + 5f, 100f, 25f);
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(clearBtnRect, "Clear All"))
            {
                selectedGenes.Clear();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }

            float listStartY = headerRect.yMax + 5f;
            Widgets.DrawLineHorizontal(0f, listStartY, inRect.width);

            // --- ScrollView ---
            float footerHeight = 50f;
            Rect scrollRect = new Rect(0f, listStartY + 5f, inRect.width,
                inRect.height - listStartY - footerHeight - 10f);
            float viewHeight = (geneOptions.Count * 40f) + 60f;
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 26f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            bool drawnEndoHeader = false;
            bool drawnXenoHeader = false;

            foreach (Gene gene in geneOptions)
            {
                if (gene.pawn.genes.HasXenogene(gene.def) && !drawnXenoHeader)
                {
                    DrawCategoryHeader(ref curY, viewRect.width, "Xenogenes");
                    drawnXenoHeader = true;
                }
                else if (!gene.pawn.genes.HasXenogene(gene.def) && !drawnEndoHeader)
                {
                    DrawCategoryHeader(ref curY, viewRect.width, "Endogenes");
                    drawnEndoHeader = true;
                }

                Rect rowRect = new Rect(0f, curY, viewRect.width, 36f);
                bool isSelected = selectedGenes.Contains(gene);

                // Get the specific conflict name
                string conflictName = GetConflictingGeneName(gene);
                bool hasConflict = !conflictName.NullOrEmpty();

                string overriddenBy = GetOverridingGeneName(gene);

                if (isSelected) Widgets.DrawHighlightSelected(rowRect);
                else Widgets.DrawHighlightIfMouseover(rowRect);

                // Modified Tooltip to explain conflict
                TooltipHandler.TipRegion(rowRect, new TipSignal(() =>
                {
                    string tip = $"{gene.LabelCap}\n\n{gene.def.DescriptionFull}";
                    if (gene.Overridden)
                    {
                        tip += $"\n\n<color=#999999>(This gene is overridden by {overriddenBy})</color>";
                    }
                    if (hasConflict)
                    {
                        // Adds a red warning with the specific gene name
                        tip += $"\n\n<color=#ff6666>(This gene conflicts with {conflictName})</color>";
                    }
                    return tip;
                }, gene.GetHashCode()));

                Widgets.DefIcon(new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f), gene.def);
                Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 80f, rowRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;

                // Apply color
                if (gene.Overridden) GUI.color = Color.gray;
                if (hasConflict) GUI.color = Color.red;


                Widgets.Label(labelRect, gene.LabelCap);
                GUI.color = Color.white;

                Widgets.Checkbox(new Vector2(rowRect.xMax - 30f, rowRect.y + 6f), ref isSelected, 24f, false);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (isSelected)
                    {
                        selectedGenes.Remove(gene);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else if (selectedGenes.Count < maxSelection)
                    {
                        selectedGenes.Add(gene);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    else
                    {
                        Messages.Message("Max selection reached.", MessageTypeDefOf.RejectInput, false);
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                }

                curY += 40f;
            }

            Widgets.EndScrollView();

            // --- Footer Section ---
            float buttonWidth = (inRect.width / 2f) - 10f; // Split the width for two buttons
            float footerY = inRect.height - 40f;

            // Cancel Button (Left Side)
            Rect cancelRect = new Rect(0f, footerY, buttonWidth, 35f);
            if (Widgets.ButtonText(cancelRect, "Cancel"))
            {
                Close(); // Just close, don't invoke the callback
            }

            // Confirm Button (Right Side)
            Rect confirmRect = new Rect(inRect.width - buttonWidth, footerY, buttonWidth, 35f);

            // Optional: Change button color or label if nothing is selected
            GUI.color = selectedGenes.Count > 0 ? Color.white : Color.gray;

            if (Widgets.ButtonText(confirmRect, "Confirm Selection"))
            {
                if (selectedGenes.Count > 0)
                {
                    onConfirm?.Invoke(selectedGenes.ToList());
                    Close();
                }
                else
                {
                    // If they confirm with 0, we could treat it as a cancel or just close
                    Messages.Message("No genes selected.", MessageTypeDefOf.RejectInput, false);
                }
            }
            GUI.color = Color.white;
        }

        private void DrawCategoryHeader(ref float curY, float width, string label)
        {
            Rect rect = new Rect(0f, curY, width, 30f);
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(rect, label.ToUpper());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            curY += 30f;
            Widgets.DrawLineHorizontal(0f, curY - 2f, width);
        }
    }
}