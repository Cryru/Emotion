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

        public BasicTextBg(Font font, uint textSize, string text, Color color, Vector2 location, float priority) : base(font, textSize, text, color, location, priority)
        {
            BackgroundColor = Color.Black;
        }

        public BasicTextBg(Font font, uint textSize, string text, Color color, Color backgroundColor, Vector2 location, float priority) : base(font, textSize, text, color, location, priority)
        {
            BackgroundColor = backgroundColor;
        }

        public override void Draw(Renderer renderer)
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

            renderer.Render(new Vector3(X, Y, Z), new Vector2(Width, Height), BackgroundColor);
            renderer.RenderString(_font, _textSize, _text, new Vector3(X + Padding.X, Y + Padding.Y, Z), Color);
        }
    }
}