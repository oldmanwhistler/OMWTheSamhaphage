using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace OMW_Samhaphage
{
    public class Dialog_Options : Window
    {
        private List<FloatMenuOption> options;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(450f, 300f);

        public Dialog_Options(List<FloatMenuOption> options)
        {
            this.options = options;
            this.forcePause = true; // This pauses the game
            this.doCloseButton = true; // Adds an "X" or "Close" button
            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true; // Prevents clicking things behind the window
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 35f), "Select Biological Action");
            Text.Font = GameFont.Small;

            // Define the area for the buttons
            Rect outRect = new Rect(0, 45f, inRect.width, inRect.height - 100f);
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, options.Count * 38f);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (var option in options)
            {
                Rect buttonRect = new Rect(0, curY, viewRect.width, 32f);

                // If the option is disabled (e.g. "Can't make pregnant"), draw it differently
                if (option.Disabled)
                {
                    GUI.color = Color.gray;
                    Widgets.Label(buttonRect, option.Label);
                    GUI.color = Color.white;
                }
                else if (Widgets.ButtonText(buttonRect, option.Label))
                {
                    option.action(); // Execute the stored action
                    Close(); // Close the window after selection
                }

                curY += 38f;
            }

            Widgets.EndScrollView();
        }
    }
}