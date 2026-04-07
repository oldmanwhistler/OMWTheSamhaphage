using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{

    public class WindowState
    {
        private TextAnchor _anchor;
        private GameFont _font;
        private Color _color;

        public WindowState()
        {
            this._anchor = Text.Anchor;
            this._font = Text.Font;
            this._color = GUI.color;
        }

        public void Restore()
        {
            Text.Anchor = this._anchor;
            Text.Font = this._font;
            GUI.color = this._color;
        }
            
    }

}