// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics.Text;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Label : Renderable2D
    {
        public string Text;
        public Font Font;

        #region Constructors

        public Label(Rectangle bounds, Font font, string text, float rotation = 0f, Color? color = null) : base(bounds.Location, bounds.Size, rotation)
        {
            Font = font;
            Text = text;
            if (color != null) Color = (Color) color;
        }

        public Label(Vector3 position, Vector2 size, Font font, string text, float rotation = 0f, Color? color = null) : base(position.X, position.Y, position.Z, size.X, size.Y, rotation)
        {
            Font = font;
            Text = text;
            if (color != null) Color = (Color) color;
        }

        public Label(Vector2 position, Vector2 size, Font font, string text, float rotation = 0f, Color? color = null) : base(position.X, position.Y, 0, size.X, size.Y, rotation)
        {
            Font = font;
            Text = text;
            if (color != null) Color = (Color) color;
        }

        public Label(Font font, string text, float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f, float rotation = 0f, Color? color = null) :
            base(x, y, z, width, height, rotation)
        {
            Font = font;
            Text = text;
            if (color != null) Color = (Color) color;
        }

        #endregion
    }
}