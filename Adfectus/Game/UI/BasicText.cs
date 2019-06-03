#region Using

using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics.Text;
using Adfectus.Primitives;

#endregion

namespace Adfectus.Game.UI
{
    // todo: Cache rendered text into a buffer. Maybe bring RichText down to a basic form and then extend that.
    public class BasicText : Control
    {
        #region Properties

        /// <summary>
        /// The text's font.
        /// </summary>
        public Font Font
        {
            get => _font;
            set
            {
                _font = value;
                _updateSize = true;
            }
        }

        protected Font _font;

        /// <summary>
        /// The size of the text to render.
        /// </summary>
        public uint TextSize
        {
            get => _textSize;
            set
            {
                _textSize = value;
                _updateSize = true;
            }
        }

        protected uint _textSize;

        /// <summary>
        /// The text to render.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _updateSize = true;
            }
        }

        protected string _text;

        /// <summary>
        /// The color of the text to render.
        /// </summary>
        public Color Color { get; set; }

        #endregion

        protected bool _updateSize = true;

        public BasicText(Font font, uint textSize, string text, Color color, Vector3 position) : base(position, Vector2.Zero)
        {
            _font = font;
            _textSize = textSize;
            _text = text;
            Color = color;
        }

        public override void Render()
        {
            if (_updateSize)
            {
                Atlas atlas = _font.GetFontAtlas(_textSize);
                Vector2 size = atlas.MeasureString(Text);
                Size = size;
                _updateSize = false;
            }

            Engine.Renderer.RenderString(_font.GetFontAtlas(_textSize), _text, Vector3.Zero, Color);
        }
    }
}