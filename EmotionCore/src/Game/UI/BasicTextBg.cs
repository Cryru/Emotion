// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public class BasicTextBg : BasicText
    {
        #region Properties

        /// <summary>
        /// The color of the text's background.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// The background's padding.
        /// </summary>
        public Rectangle Padding { get; set; }

        #endregion

        public BasicTextBg(Font font, uint textSize, string text, Color color, Vector3 position) : base(font, textSize, text, color, position)
        {
            BackgroundColor = Color.Black;
        }

        public BasicTextBg(Font font, uint textSize, string text, Color color, Color backgroundColor, Vector3 position) : base(font, textSize, text, color, position)
        {
            BackgroundColor = backgroundColor;
        }

        public override void Render(Renderer renderer)
        {
            if (_updateSize)
            {
                Atlas atlas = _font.GetFontAtlas(_textSize);
                Vector2 size = atlas.MeasureString(Text);
                Size = size;
                Width += Padding.X + Padding.Width;
                Height += Padding.Y + Padding.Height;
                _updateSize = false;
            }

            renderer.Render(Vector3.Zero, new Vector2(Width, Height), BackgroundColor);
            renderer.RenderString(_font, _textSize, _text, new Vector3(Padding.X, Padding.Y, 0), Color);
        }
    }
}