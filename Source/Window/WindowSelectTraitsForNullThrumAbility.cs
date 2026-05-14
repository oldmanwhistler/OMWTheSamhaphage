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
        private NullThrumSelectionTrait selector;
        private WindowState windowState;
        private HashSet<TraitPlus> selectedTraits = new HashSet<TraitPlus>();
        private System.Action<List<TraitPlus>> onConfirm;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(450f, 700f);

        public WindowSelectTraitsForNullThrumAbility(NullThrumSelectionTrait selector, System.Action<List<TraitPlus>> callback)
        {
            this.selector = selector;
            this.onConfirm = callback;

            this.forcePause = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;
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
        
        public override void DoWindowContents(Rect inRect)
        {
            this.windowState = new WindowState();
            // --- Header ---
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, 40f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, $"Select Traits to {this.selector.Name} ({100f * this.SelectionCurCost() / this.SelectionMaxCost():F0}%)");

            // Clear All Button
            Rect clearBtnRect = new Rect(inRect.width - 100f, inRect.y + 5f, 100f, 25f);
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(clearBtnRect, "Clear All"))
            {
                selectedTraits.Clear();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }

            float listStartY = headerRect.yMax + 5f;
            Widgets.DrawLineHorizontal(0f, listStartY, inRect.width);

            // --- ScrollView ---
            float footerHeight = 50f;
            Rect scrollRect = new Rect(0f, listStartY + 5f, inRect.width,
                inRect.height - listStartY - footerHeight - 10f);
            float viewHeight = (this.selector.traits.Count * 40f) + 20f;
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 26f, viewHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (TraitPlus plus in this.selector.traits)
            {
                Rect rowRect = new Rect(0f, curY, viewRect.width, 36f);
                bool isSelected = selectedTraits.Contains(plus);

                if (isSelected) Widgets.DrawHighlightSelected(rowRect);
                else Widgets.DrawHighlightIfMouseover(rowRect);

                TooltipHandler.TipRegion(rowRect, new TipSignal(() =>
                {
                    return plus.ToString();
                }, plus.GetHashCode()));

                // Trait icons are not standard in vanilla, but Widgets.DefIcon will handle TraitDef
                Widgets.DefIcon(new Rect(rowRect.x + 4f, rowRect.y + 3f, 30f, 30f), plus.trait.def);
                Rect labelRect = new Rect(rowRect.x + 40f, rowRect.y, rowRect.width - 80f, rowRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;

                // Apply color for conflicts
                if (plus.HasConflict()) GUI.color = Color.red;

                Widgets.Label(labelRect, plus.trait.LabelCap);
                GUI.color = Color.white;

                Widgets.Checkbox(new Vector2(rowRect.xMax - 30f, rowRect.y + 6f), ref isSelected, 24f, false);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (isSelected)
                    {
                        selectedTraits.Remove(plus);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else if (this.SelectionCurCost() < this.SelectionMaxCost())
                    {
                        selectedTraits.Add(plus);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    else
                    {
                        Messages.Message($"Max cost of {this.SelectionMaxCost()} resonance reached.", MessageTypeDefOf.RejectInput, false);
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }
                }

                curY += 40f;
            }

            Widgets.EndScrollView();

            // --- Footer Section ---
            float buttonWidth = (inRect.width / 2f) - 10f;
            float footerY = inRect.height - 40f;

            Rect cancelRect = new Rect(0f, footerY, buttonWidth, 35f);
            if (Widgets.ButtonText(cancelRect, "Cancel"))
            {
                Close();
            }

            Rect confirmRect = new Rect(inRect.width - buttonWidth, footerY, buttonWidth, 35f);
            GUI.color = selectedTraits.Count > 0 ? Color.white : Color.gray;

            if (Widgets.ButtonText(confirmRect, "Confirm Selection"))
            {
                if (selectedTraits.Count > 0)
                {
                    onConfirm?.Invoke(selectedTraits.ToList());
                    Close();
                }
                else
                {
                    Messages.Message("No traits selected.", MessageTypeDefOf.RejectInput, false);
                }
            }
            GUI.color = Color.white;
            this.windowState.Restore();
        }
    }
}