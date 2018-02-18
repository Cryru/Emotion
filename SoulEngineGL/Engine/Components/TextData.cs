// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Legacy;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Components
{
    public class TextData : ComponentBase
    {
        /// <summary>
        /// The text to draw.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Recalculate = true;
            }
        }

        private string _text = "";

        /// <summary>
        /// Whether a recalculation is needed.
        /// </summary>
        public bool Recalculate = true;

        /// <summary>
        /// The font to render the text with.
        /// </summary>
        public SpriteFont Font = AssetLoader.DefaultFont;

        /// <summary>
        /// The text formatting style.
        /// </summary>
        public TextStyle Style = TextStyle.Left;

        /// <summary>
        /// The character to draw up to. Used for scrolling and cutting off text without messing with formatting.
        /// </summary>
        public int DrawLimit = -1;

        /// <summary>
        /// The default text color.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The size of the text.
        /// </summary>
        public Vector2 TextSize;

        #region Cache

        internal Vector2 CachedSize;

        internal List<TextLine> LinesCache = new List<TextLine>();

        #endregion

    }
}