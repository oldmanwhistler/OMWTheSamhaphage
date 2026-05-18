using RimWorld;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public class WindowSelectGenesForNullThrumAbility : Window
    {
        private NullThrumSelectionGene selector;
        private WindowState windowState;
        private HashSet<GenePlus> selectedGenes = new HashSet<GenePlus>();
        private System.Action<List<GenePlus>> onConfirm;
        private System.Action onDismiss;
        private Vector2 scrollPosition;
        private float selectionMaxCost;

        public override Vector2 InitialSize => new Vector2(450f, 700f);

        public WindowSelectGenesForNullThrumAbility(NullThrumSelectionGene selector, System.Action<List<GenePlus>> callback, System.Action onDismiss = null)
        {
            this.selector = selector;
            this.onConfirm = callback;
            this.onDismiss = onDismiss;

            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;
            this.selectionMaxCost = selector.SelectionMaxCost();
        }

        public float SelectionCurCost()
        {
            float tmp = 0f;
            foreach (GenePlus plus in selectedGenes)
            {
                tmp += plus.value;    
            }
            return tmp;
        }        
        public override void DoWindowContents(Rect inRect)
        {
            this.windowState = new WindowState();
            // --- Header ---
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, 40f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, $"Select Genes to {this.selector.Name}");            

            float listStartY = headerRect.yMax + 5f;
            Widgets.DrawLineHorizontal(0f, listStartY, inRect.width);

            // --- ScrollView ---
            float footerHeight = 50f;
            Rect scrollRect = new Rect(0f, listStartY + 5f, inRect.width,
                inRect.height - listStartY - footerHeight - 10f);
            float viewHeight = (this.selector.genes.Count * 40f) + 60f;
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 26f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            bool drawnEndoHeader = false;
            bool drawnXenoHeader = false;

            foreach (GenePlus plus in this.selector.genes)
            {
                if (plus.isXenogene && !drawnXenoHeader)
                {
                    DrawCategoryHeader(ref curY, viewRect.width, "Xenogenes");
                    drawnXenoHeader = true;
                }
                else if (!plus.isXenogene && !drawnEndoHeader)
                {
                    DrawCategoryHeader(ref curY, viewRect.width, "Endogenes");
                    drawnEndoHeader = true;
                }

                Rect rowRect = new Rect(0f, curY, viewRect.width, 36f);
                bool isSelected = selectedGenes.Contains(plus);

                if (isSelected) Widgets.DrawHighlightSelected(rowRect);
                else Widgets.DrawHighlightIfMouseover(rowRect);

                // Modified Tooltip to explain conflict
                TooltipHandler.TipRegion(rowRect, new TipSignal(() =>
                {
                    return plus.ToString();
                }, plus.GetHashCode()));

                Widgets.DefIcon(new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f), plus.gene.def);
                Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 130f, rowRect.height);
                Rect valueRect = new Rect(rowRect.xMax - 85f, rowRect.y, 50f, rowRect.height);

                Text.Anchor = TextAnchor.MiddleLeft;

                // Apply color
                if (plus.gene.Overridden) GUI.color = Color.gray;
                if (plus.HasConflict()) GUI.color = Color.red;

                Widgets.Label(labelRect, plus.gene.LabelCap);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(valueRect, plus.value.ToString("F1"));
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.Checkbox(new Vector2(rowRect.xMax - 30f, rowRect.y + 6f), ref isSelected, 24f, false);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (isSelected)
                    {
                        selectedGenes.Remove(plus);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else if (this.SelectionCurCost() < this.selectionMaxCost)
                    {
                        selectedGenes.Add(plus);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    else
                    {
                        Messages.Message($"Max cost of {this.selectionMaxCost} resonance reached.", MessageTypeDefOf.RejectInput, false);
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                }

                curY += 40f;
            }

            Widgets.EndScrollView();

            // --- Footer Section ---
            float spacing = 10f;
            float confirmWidth = inRect.width * 0.5f;
            float otherWidth = (inRect.width - confirmWidth - (spacing * 2f)) / 2f;
            float footerY = inRect.height - 40f;
            Text.Font = GameFont.Small;

            // Clear All Button
            Rect clearRect = new Rect(0f, footerY, otherWidth, 35f);
            if (Widgets.ButtonText(clearRect, "Clear All"))
            {
                selectedGenes.Clear();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }            

            // Cancel/Skip Button
            Rect cancelRect = new Rect(otherWidth + spacing, footerY, otherWidth, 35f);
            if (Widgets.ButtonText(cancelRect, onDismiss != null ? "Skip" : "Cancel"))
            {
                Close();
            }           
            
            // Confirm Button
            Rect confirmRect = new Rect(inRect.width - confirmWidth, footerY, confirmWidth, 35f);

            GUI.color = selectedGenes.Count > 0 ? Color.white : Color.gray;

            if (Widgets.ButtonText(confirmRect, $"Confirm Selection for {100f * this.SelectionCurCost():F1}%"))
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
            this.windowState.Restore();
        }

        public override void PostClose()
        {
            base.PostClose();
            onDismiss?.Invoke();
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