// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI.Layout
{
    public class LayoutControl
    {
        public Transform Control;
        public Rectangle Margin;

        public void Render()
        {
            // Top
            Context.Renderer.RenderQueue(new Vector3(Control.X, Control.Y - Margin.Y, Control.Z), new Vector2(Control.Width, Margin.Y), new Color(219, 43, 143, 125));

            // Left
            Context.Renderer.RenderQueue(new Vector3(Control.X - Margin.X, Control.Y, Control.Z), new Vector2(Margin.X, Control.Height), new Color(66, 244, 132, 125));

            // Bottom
            Context.Renderer.RenderQueue(new Vector3(Control.X, Control.Y + Control.Height, Control.Z), new Vector2(Control.Width, Margin.Height), new Color(66, 244, 132, 125));

            // Right
            Context.Renderer.RenderQueue(new Vector3(Control.X + Control.Width, Control.Y, Control.Z), new Vector2(Margin.Width, Control.Height), new Color(219, 43, 143, 125));
        }
    }
}