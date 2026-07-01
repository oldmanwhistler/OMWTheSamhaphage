using System;
using System.Collections.Generic;
using RimWorld.QuestGen;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OMW_Samhaphage
{

    // The MIT License (MIT)

    // Copyright (c) 2022 James

    // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    // The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

    // From: https://github.com/Epicguru/BetterFloatMenu/tree/master/BetterFloatMenu

    /// <summary>
    /// An alternative to the built-in <see cref="FloatMenu"/>.
    /// Provides a more visual layout, and a search bar.
    /// </summary>
    public class BetterFloatMenu : Window
    {
        static Logger Log = new Logger("UI");
        private static readonly Texture2D LethalIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Death", false) ?? BaseContent.BadTex;
        private static readonly Texture2D CustomBG = ContentFinder<Texture2D>.Get("UI/Menu/SamhaphageBG", false);
        private static readonly Texture2D TranslucentBlackTex = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));

        /// <summary>
        /// Opens a new float menu using the items provided.
        /// Note: by default, opening a new window will close existing windows.
        /// You should set <see cref="Window.onlyOneOfTypeAllowed"/> to false to disable this behaviour.
        /// </summary>
        /// <param name="items">The list of items that the user can choose from.</param>
        /// <param name="onSelected">The method to be called when an item is selected.</param>
        /// <returns>The newly created window.</returns>
        public static BetterFloatMenu Open(List<MenuItemBase> items, Pawn caster, Func<MenuItemBase, bool> onSelected, Pawn targetPawn = null)
        {
            var created = new BetterFloatMenu();
            created.Items = items;
            created.Caster = caster;
            created.TargetPawn = targetPawn;
            created.OnSelected = onSelected;
            created.closeOnAccept = false;
            created.closeOnCancel = true;
            created.closeOnClickedOutside = false;
            created.absorbInputAroundWindow = true;
            created.doCloseX = true;
            created.forcePause = true;
            created.layer = WindowLayer.SubSuper;
            Find.WindowStack.Add(created);
            return created;
        }

        /// <summary>
        /// Generic string search and highlighting utility method.
        /// If it returns null, the search does not match the label.
        /// If it returns a string, the search succeeded. Furthermore, the return value will be a highlighted version of <paramref name="label"/> using RichText
        /// if the <paramref name="highlightColor"/> argument is not null, otherwise simply <paramref name="label"/>.
        /// </summary>
        /// <param name="label">The string to search in.</param>
        /// <param name="search">The search input.</param>
        /// <param name="highlightColor">The Hex format of the color to highlight with. Should be in the format #RRGGBB(AA). Can be null to disable highlighting.</param>
        /// <returns></returns>
        public static string SearchMatch(string label, string search, string highlightColor = "#65f065")
        {
            int index = label.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            if (highlightColor == null)
                return label;

            return label.Insert(index + search.Length, "</color>").Insert(index, $"<color={highlightColor}>");
        }

        /// <summary>
        /// A utility function to build a sorted list of <see cref="MenuItemBase"/>, to be used in <see cref="Open(List{MenuItemBase}, Action{MenuItemBase})"/>.
        /// Takes an numeration of 'raw items' and a function that converts those 'raw items' to <see cref="MenuItemBase"/>.
        /// </summary>
        /// <typeparam name="T">The type of the raw items.</typeparam>
        /// <param name="rawItems">An enumeration of raw items to build the menu items from.</param>
        /// <param name="makeItem">A function to build a menu item based on a raw item. If it returns a null item, it is ignored and will not be added to the final list.</param>
        /// <returns>A sorted list of menu items. Will not contain null values.</returns>
        public static List<MenuItemBase> MakeItems<T>(IEnumerable<T> rawItems, Func<T, MenuItemBase> makeItem)
        {
            var list = new List<MenuItemBase>();
            foreach (var item in rawItems)
            {
                var result = makeItem(item);
                if (result != null)
                    list.Add(result);
            }
            list.Sort();
            return list;
        }

        /// <summary>
        /// The list of items to display.
        /// </summary>
        public List<MenuItemBase> Items;
        /// <summary>
        /// The pawn whose resonance will be displayed.
        /// </summary>
        public Pawn Caster;
        /// <summary>
        /// An optional target pawn shown in the info panel above the menu.
        /// </summary>
        public Pawn TargetPawn;
        /// <summary>
        /// Action called when an item is selected (clicked on).
        /// </summary>
        public Func<MenuItemBase, bool> OnSelected;
        /// <summary>
        /// If true, the window will close after selecting an item.
        /// If false, <see cref="OnSelected"/> will be still be called but the window will not close.
        /// Default value: true.
        /// </summary>
        public bool CloseOnSelected = true;
        /// <summary>
        /// If true, displays a search bar that allows for items to be filtered out.
        /// </summary>
        public bool CanSearch = false;
        /// <summary>
        /// How many items to display per row.
        /// Default value: 4.
        /// </summary>
        public int Columns = 4;
        /// <summary>
        /// The amount of padding between items, measured in unscaled pixels.
        /// Default value: 6.
        /// </summary>
        public float Padding = 6;
        /// <summary>
        /// The current search string. Set to <see cref="string.Empty"/> or null to reset search bar.
        /// </summary>
        public string SearchString = "";

        private readonly List<MenuItemBase> preRenderItems = new List<MenuItemBase>();
        private readonly PawnInfoPanel casterInfoPanel = new PawnInfoPanel();
        private readonly PawnInfoPanel targetInfoPanel = new PawnInfoPanel();
        private float lastHeight, lastWidth;
        private Vector2 scroll;

        public override Vector2 InitialSize => new Vector2(1376f, 768f);

        public override void DoWindowContents(Rect inRect)
        {
            if (CustomBG != null)
            {
                GUI.DrawTexture(inRect, CustomBG);
            }

            SearchString ??= "";

            if (Items == null || Items.Count == 0)
            {
                Log.Debug($"Opened a {nameof(BetterFloatMenu)} with no items! Closing...");
                Close();
                return;
            }

            float panelGap = 30f;
            float panelWidth = (inRect.width - panelGap * 2f) / 3f;
            Rect leftPanelRect = new Rect(inRect.x, inRect.y, panelWidth, inRect.height);
            Rect middlePanelRect = new Rect(leftPanelRect.xMax + panelGap, inRect.y, panelWidth, inRect.height);
            Rect rightPanelRect = new Rect(middlePanelRect.xMax + panelGap, inRect.y, panelWidth, inRect.height);

            DrawInfoPanel(leftPanelRect, Caster, null, "CASTER");
            DrawInfoPanel(rightPanelRect, TargetPawn, Caster, "VICTIM");

            GUI.BeginGroup(middlePanelRect);
            try
            {
                DrawCenterContent(new Rect(panelGap / 2f, panelGap, middlePanelRect.width - panelGap, middlePanelRect.height - panelGap * 2f));
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawCenterContent(Rect rect)
        {
            if (CanSearch)
            {
                float cancelWidth = 70f;
                float spacing = 6f;
                Rect searchRect = new Rect(0f, 0f, rect.width - cancelWidth - spacing, 28f);
                Rect cancelRect = new Rect(searchRect.xMax + spacing, 0f, cancelWidth, 28f);

                SearchString = Widgets.TextField(searchRect, SearchString);
                if (Widgets.ButtonText(cancelRect, "Cancel"))
                {
                    Close();
                }
                rect.yMin += 36f;
            }

            if (Caster != null)
            {
                float curRes = ResonanceUtility.Total(Caster);
                float maxRes = OMW_Mod.settings.resonanceMax;
                float fillPercent = maxRes > 0 ? Mathf.Clamp01(curRes / maxRes) : 0f;
                Rect meterRect = new Rect(rect.x, rect.y, rect.width, 26f);
                // TranslucentBlackTex vs BaseContent.BlackTex
                Widgets.FillableBar(meterRect, fillPercent, SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.1f, 0.6f)), TranslucentBlackTex, false);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(meterRect, $"Resonance: {curRes:F1} / {maxRes:F1}");
                Text.Anchor = TextAnchor.UpperLeft;
                rect.yMin += 36f;
            }

            if (CanSearch || preRenderItems.Count != Items.Count)
            {
                preRenderItems.Clear();
                preRenderItems.AddRange(FilteredItems(SearchString));
            }

            float curX = 0;
            float curY = 0;
            float maxRowHeight = 0;
            int columnCount = 0;

            Widgets.BeginScrollView(rect, ref scroll, new Rect(0, 0, lastWidth, lastHeight));
            lastWidth = 0;
            lastHeight = 0;

            for (int i = 0; i < preRenderItems.Count; i++)
            {
                var item = preRenderItems[i];
                var pos = new Vector2(curX, curY);
                var size = item.Draw(pos);
                var area = new Rect(pos, size);

                if (item.IsLethal)
                {
                    Rect lethalIconRect = new Rect(area.xMax - 22f, area.y + 2f, 20f, 20f);
                    GUI.color = Color.red;
                    GUI.DrawTexture(lethalIconRect, LethalIcon);
                    GUI.color = Color.white;
                }
                else if (item.BoxThickness > 0 && item.BoxColor.a > 0)
                {
                    GUI.color = item.BoxColor;
                    Widgets.DrawBox(area, item.BoxThickness);
                    GUI.color = Color.white;
                }

                if (!item.Disabled)
                {
                    Widgets.DrawHighlightIfMouseover(area);
                    if (Widgets.ButtonInvisible(area))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        OnSelected?.Invoke(item);
                        if (CloseOnSelected)
                        {
                            Close();
                            break;
                        }
                    }
                }

                curX += size.x + Padding * 2;
                maxRowHeight = Mathf.Max(maxRowHeight, size.y);
                columnCount++;

                if (columnCount >= Columns)
                {
                    lastWidth = Mathf.Max(lastWidth, curX);
                    curX = 0;
                    curY += maxRowHeight + Padding;
                    maxRowHeight = 0;
                    columnCount = 0;
                }

                lastHeight = Mathf.Max(lastHeight, curY + maxRowHeight);
                lastWidth = Mathf.Max(lastWidth, curX);
            }

            Widgets.EndScrollView();
        }

        private void DrawInfoPanel(Rect panelRect, Pawn source, Pawn dest, string roleLabel)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawBox(panelRect);
            GUI.color = Color.white;
            Rect innerRect = panelRect.ContractedBy(6f);
            PawnInfoPanel panel = roleLabel == "CASTER" ? casterInfoPanel : targetInfoPanel;
            panel.Draw(innerRect, source, dest, roleLabel);
        }

        /// <summary>
        /// Returns the items that match the search string: the items that return true from <see cref="MenuItemBase.MatchesSearch(string)"/>.
        /// Passing in a null or blank <paramref name="search"/> string will return all items.
        /// </summary>
        /// <param name="search">The search string. May be null to return all items.</param>
        /// <returns>An enumeration of all items that match the search.</returns>
        public virtual IEnumerable<MenuItemBase> FilteredItems(string search)
        {
            if (Items == null)
                yield break;

            bool all = string.IsNullOrWhiteSpace(search);
            string newSearch = search?.Trim();

            foreach (var item in Items)
            {
                if (all || item.MatchesSearch(newSearch))
                    yield return item;
            }
        }
    }

    /// <summary>
    /// The base class for items displayed by a <see cref="BetterFloatMenu"/>.
    /// </summary>
    public abstract class MenuItemBase : IComparable<MenuItemBase>
    {
        /// <summary>
        /// User data.
        /// </summary>
        public object Payload { get; set; }
        /// <summary>
        /// If true, a visual warning (red border) will be drawn to indicate a lethal action.
        /// </summary>
        public bool IsLethal = false;
        /// <summary>
        /// The color of the containing box.
        /// </summary>
        public Color BoxColor = Color.white;
        /// <summary>
        /// The width, in pixels, of the containing box.
        /// </summary>
        public int BoxThickness = 1;
        // Is the button disabled and can't be pressed
        public bool Disabled = false;

        /// <summary>
        /// Returns the <see cref="Payload"/>, cast to a specified type. May throw an invalid cast or null exception.
        /// Equivalent to: <code>(T)Payload</code>
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>The payload, cast to a particular type.</returns>
        public T GetPayload<T>() => (T)Payload;

        /// <summary>
        /// Returns true if this item should be shown when searching for the <paramref name="search"/> string.
        /// </summary>
        /// <param name="search">The search string, that comes from the search bar.</param>
        /// <returns>True if this item should be shown, false to hide.</returns>
        public abstract bool MatchesSearch(string search);

        /// <summary>
        /// Used to sort this item within the window. Only called automatically when using <see cref="BetterFloatMenu.MakeItems{T}(IEnumerable{T}, Func{T, MenuItemBase})"/>.
        /// </summary>
        /// <param name="other">The other item to compare to.</param>
        public abstract int CompareTo(MenuItemBase other);

        /// <summary>
        /// When called should draw the item at the provided position.
        /// The position is the top-left corner. Should return the size, in pixels, that this item occupied.
        /// </summary>
        /// <param name="pos">The input position. This is the top-left, and it is in GUI space.</param>
        /// <returns>The size of this drawn item. Should not be negative.</returns>
        public abstract Vector2 Draw(Vector2 pos);
    }

    /// <summary>
    /// A <see cref="MenuItemBase"/> that displays a single icon and no label. It is normally square.
    /// Can display a tooltip.
    /// </summary>
    public class MenuItemIcon : MenuItemBase
    {
        static Logger Log = new Logger("UI");
        /// <summary>
        /// The size of the item.
        /// </summary>
        public Vector2 Size = new Vector2(64, 84);
        /// <summary>
        /// The optional tooltip text. Used to filter searches if provided.
        /// </summary>
        public string Tooltip;
        /// <summary>
        /// The icon to display.
        /// </summary>
        public Texture2D Icon;
        /// <summary>
        /// The tint of the icon.
        /// </summary>
        public Color Color = Color.white;
        /// <summary>
        /// The background color to place behind the icon. Defaults to (0, 0, 0, 0) i.e. no background.
        /// </summary>
        public Color BGColor = default;

        protected string drawLabel;
        public string Label;

        public NullThrumAbilityBase Ability = null;

        // Constructor for abilities with a generic payload and optional target
        public MenuItemIcon(NullThrumAbilityBase ability, string tooltip, object payload)
        {
            this.Ability = ability;
            this.Label = ability.AbilityName;
            this.Icon = ability.Icon;
            this.IsLethal = ability.IsLethal;
            this.Tooltip = tooltip;
            Log.Debug($"MenuItemIcon, enabled, {this.Label}, {tooltip}");
            this.Payload = payload;
            this.Color = Color.white;
            this.Disabled = false;
        }

        // Constructor for disabled items
        public MenuItemIcon(NullThrumAbilityBase ability, string tooltip)
        {
            this.Ability = ability;
            this.Label = ability.AbilityName;
            this.Icon = ability.Icon;
            this.IsLethal = ability.IsLethal;
            this.Tooltip = tooltip;
            Log.Debug($"MenuItemIcon, disabled, {this.Label}, {tooltip}");        
            this.Payload = null;           
            this.Color = Color.gray;
            this.Disabled = true;
        }

        public override bool MatchesSearch(string search)
        {
            drawLabel = BetterFloatMenu.SearchMatch(Label ?? "", search, null);
            if (drawLabel == null && Tooltip != null)
            {
                drawLabel = BetterFloatMenu.SearchMatch(Tooltip, search, null);
            }
            return drawLabel != null;
        }

        public override int CompareTo(MenuItemBase other)
        {
            return 0; // No order, sort by natural load order (mod).
        }

        public override Vector2 Draw(Vector2 pos)
        {
            Rect area = new Rect(pos, Size);
            Rect iconArea = new Rect(pos.x, pos.y, Size.x, Size.x);
            Rect labelArea = new Rect(pos.x, pos.y + Size.x + 2f, Size.x, Size.y - Size.x - 2f);

            if (Icon != null)
            {
                if (BGColor != default)
                {
                    Widgets.DrawBoxSolid(iconArea, BGColor);
                }

                var oldColor = GUI.color;
                if (Color != Color.white)
                    GUI.color = Color;
                Widgets.DrawTextureFitted(iconArea, Icon, 1f);
                GUI.color = oldColor;
            }

            if (!Label.NullOrEmpty())
            {
                var oldAnchor = Text.Anchor;
                var oldFont = Text.Font;
                Text.Anchor = TextAnchor.UpperCenter;
                Text.Font = GameFont.Tiny;
                if (Disabled) GUI.color = Color.gray;
                Widgets.Label(labelArea, Label);
                GUI.color = Color.white;
                Text.Anchor = oldAnchor;
                Text.Font = oldFont;
            }

            if (Tooltip != null)
                TooltipHandler.TipRegion(area, Tooltip);

            return Size;
        }
    }
}