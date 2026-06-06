using RimWorld;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public class WindowSelectTraitsForNullThrumAbility : Window
    {
        static Logger Log = new Logger("UI");
        private NullThrumSelectionTrait selector;
        private WindowState windowState;
        private HashSet<TraitPlus> selectedTraits = new HashSet<TraitPlus>();
        private System.Action<List<TraitPlus>> onConfirm; // Callback for successful confirmation
        private Vector2 scrollPosition;
        private float selectionMaxCost;
        public override Vector2 InitialSize => new Vector2(450f, 700f);

        public WindowSelectTraitsForNullThrumAbility(NullThrumSelectionTrait selector, System.Action<List<TraitPlus>> callback)
        {
            this.selector = selector;
            this.onConfirm = callback;

            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;
            this.selectionMaxCost = selector.SelectionMaxCost();            
        }

        public float SelectionCurCost()
        {
            float tmp = 0f;
            foreach (TraitPlus plus in selectedTraits)
            {
                tmp += plus.value;    
            }
            return tmp;
        }

        public float SelectionMaxCost()
        {
            return this.selector.SelectionMaxCost();
        }
        
        public override void WindowUpdate()
        {
            base.WindowUpdate();
            this.selectionMaxCost = selector.SelectionMaxCost();
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.windowState = new WindowState();
            // --- Header ---
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, 40f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, $"Select Traits to {this.selector.Name}");

            Text.Font = GameFont.Small;
            string description = NullThrumUtility.Description(this.selector.AbilityType);
            float descHeight = Text.CalcHeight(description, inRect.width);
            Rect descRect = new Rect(inRect.x, headerRect.yMax, inRect.width, descHeight);
            Widgets.Label(descRect, description);

            // --- Live Resonance Meter ---
            float curCost = SelectionCurCost();
            Rect meterRect = new Rect(inRect.x, descRect.yMax + 10f, inRect.width, 26f);
            float fillPercent = selectionMaxCost > 0 ? Mathf.Clamp01(curCost / selectionMaxCost) : 0f;
            Color barColor = (curCost > selectionMaxCost) ? ColorLibrary.RedReadable : new Color(0.4f, 0.1f, 0.6f); // Deep purple resonance color
            Widgets.FillableBar(meterRect, fillPercent, SolidColorMaterials.NewSolidColorTexture(barColor), BaseContent.BlackTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(meterRect, $"Current Selection: {curCost:F1} / Available: {selectionMaxCost:F1}");
            Text.Anchor = TextAnchor.UpperLeft;

            float listStartY = meterRect.yMax + 10f;
            Widgets.DrawLineHorizontal(0f, listStartY, inRect.width);

            // --- ScrollView ---
            float footerHeight = 50f;
            Rect scrollRect = new Rect(0f, listStartY + 5f, inRect.width,
                inRect.height - listStartY - footerHeight - 10f);
            float viewHeight = ((this.selector.traits.Count + +this.selector.unselectableTraits.Count) * 40f) + 20f;
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 26f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (TraitPlus plus in this.selector.traits)
            {
                Rect rowRect = new Rect(0f, curY, viewRect.width, 36f);
                bool isSelected = selectedTraits.Contains(plus);

                if (isSelected) Widgets.DrawHighlightSelected(rowRect);
                else Widgets.DrawHighlightIfMouseover(rowRect);

                TooltipHandler.TipRegion(rowRect, new TipSignal(plus.ToString, plus.GetHashCode()));

                // Trait icons are not standard in vanilla, but Widgets.DefIcon will handle TraitDef
                Widgets.DefIcon(new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f), plus.trait.def);
                Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 130f, rowRect.height);
                Rect valueRect = new Rect(rowRect.xMax - 85f, rowRect.y, 50f, rowRect.height);

                Text.Anchor = TextAnchor.MiddleLeft;

                // Apply color for conflicts
                if (plus.HasConflict()) GUI.color = Color.red;

                Widgets.Label(labelRect, plus.trait.LabelCap);

                if (selector.ResonanceType == NullThrumResonanceType.ResonanceTypeCredit)
                    GUI.color = Color.green;
                else if (selector.ResonanceType == NullThrumResonanceType.ResonanceTypeDebit)
                    GUI.color = Color.red;
                else
                    GUI.color = Color.white;
                
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(valueRect, plus.value.ToString("F1"));
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.Checkbox(new Vector2(rowRect.xMax - 30f, rowRect.y + 6f), ref isSelected, 24f, false);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (isSelected)
                    {
                        selectedTraits.Remove(plus);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else if (this.SelectionCurCost() < this.selectionMaxCost)
                    {
                        selectedTraits.Add(plus);
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

            if (this.selector.unselectableTraits.Count > 0)
            {
                DrawCategoryHeader(ref curY, viewRect.width, "Unselectable");
                foreach (TraitPlus plus in this.selector.unselectableTraits)
                {
                    Rect rowRect = new Rect(0f, curY, viewRect.width, 36f);
                    Widgets.DrawHighlightIfMouseover(rowRect);

                    // Modified Tooltip to explain conflict
                    TooltipHandler.TipRegion(rowRect,
                        new TipSignal(() => { return plus.ToString(); }, plus.GetHashCode()));

                    Widgets.DefIcon(new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f), plus.trait.def);
                    Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 130f, rowRect.height);
                    Rect valueRect = new Rect(rowRect.xMax - 85f, rowRect.y, 50f, rowRect.height);

                    Text.Anchor = TextAnchor.MiddleLeft;

                    // Apply color
                    GUI.color = Color.gray;

                    Widgets.Label(labelRect, plus.trait.LabelCap);
                    Text.Anchor = TextAnchor.MiddleRight;
                    GUI.color = Color.white;

                    curY += 40f;
                }
            }            

            Widgets.EndScrollView();

            // --- Footer Section ---
            float spacing = 10f;
            float confirmWidth = inRect.width * 0.5f;
            float otherWidth = (inRect.width - confirmWidth - (spacing * 2f)) / 2f;
            float footerY = inRect.height - 40f;
            Text.Font = GameFont.Small;

            Rect clearRect = new Rect(0f, footerY, otherWidth, 35f);
            if (Widgets.ButtonText(clearRect, "Clear All"))
            {
                selectedTraits.Clear();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }

            Rect cancelRect = new Rect(otherWidth + spacing, footerY, otherWidth, 35f);
            if (Widgets.ButtonText(cancelRect, "Cancel"))
            {
                Close();
            }

            Rect confirmRect = new Rect(inRect.width - confirmWidth, footerY, confirmWidth, 35f);
            bool canConfirm = selectedTraits.Count > 0 && SelectionCurCost() <= selectionMaxCost;
            GUI.color = canConfirm ? Color.white : Color.gray;

            if (Widgets.ButtonText(confirmRect, $"Confirm Selection for {this.SelectionCurCost():F1}"))
            {
                if (canConfirm)
                {
                    onConfirm?.Invoke(selectedTraits.ToList());
                    Close();
                }
                else
                {
                    Messages.Message("No traits selected.", MessageTypeDefOf.RejectInput, false);
                    Log.Debug($"WindowSelectTraitsForNullThrumAbility({selector.Name})::DoWindowContents() -> Close()");
                    Close();
                }
            }
            GUI.color = Color.white;
            this.windowState.Restore();
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