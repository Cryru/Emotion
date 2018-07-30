// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public class Panel : Control
    {
        public Texture PanelTexture { get; set; }

        public Rectangle TopLeftCorner { get; set; }
        public Rectangle Top { get; set; }
        public Rectangle TopRightCorner { get; set; }
        public Rectangle Right { get; set; }
        public Rectangle BottomRightCorner { get; set; }
        public Rectangle Bottom { get; set; }
        public Rectangle BottomLeftCorner { get; set; }
        public Rectangle Left { get; set; }
        public Rectangle Fill { get; set; }

        public float Scale { get; set; } = 1f;

        public Panel(Controller controller, Rectangle bounds, int priority) : base(controller, bounds, priority)
        {
        }

        public override void Draw(Renderer renderer)
        {
            // Check if a texture is set.
            if (PanelTexture == null) return;

            // Draw fill.
            if (Fill != Rectangle.Empty) renderer.DrawTexture(PanelTexture, new Rectangle(X, Y, Width, Height), Fill, false);

            // Draw lines.
            if (Top != Rectangle.Empty)
                for (float x = 0; x <= Width - Top.Width * Scale; x += Top.Width * Scale)
                {
                    renderer.DrawTexture(PanelTexture, new Rectangle(X + x, Y, Top.Width * Scale, Top.Height * Scale), Top, false);
                }

            if (Left != Rectangle.Empty)
                for (float y = 0; y <= Height - Left.Height * Scale; y += Left.Height * Scale)
                {
                    renderer.DrawTexture(PanelTexture, new Rectangle(X, Y + y, Left.Width * Scale, Left.Height * Scale), Left, false);
                }

            if (Right != Rectangle.Empty)
                for (float y = 0; y <= Height - Right.Height * Scale; y += Right.Height * Scale)
                {
                    renderer.DrawTexture(PanelTexture, new Rectangle(X + Width - Right.Width * Scale, Y + y, Right.Width * Scale, Right.Height * Scale), Right, false);
                }

            if (Bottom != Rectangle.Empty)
                for (float x = 0; x <= Width - Bottom.Width * Scale; x += Bottom.Width * Scale)
                {
                    renderer.DrawTexture(PanelTexture, new Rectangle(X + x, Y + Height - Bottom.Height * Scale, Bottom.Width * Scale, Bottom.Height * Scale), Bottom, false);
                }

            // Draw corners.
            if (TopLeftCorner != Rectangle.Empty)
                renderer.DrawTexture(PanelTexture, new Rectangle(X, Y, TopLeftCorner.Width * Scale, TopLeftCorner.Height * Scale), TopLeftCorner, false);
            if (TopRightCorner != Rectangle.Empty)
                renderer.DrawTexture(PanelTexture, new Rectangle(X + Width - TopRightCorner.Width * Scale, Y, TopRightCorner.Width * Scale, TopRightCorner.Height * Scale),
                    TopRightCorner, false);
            if (BottomRightCorner != Rectangle.Empty)
                renderer.DrawTexture(PanelTexture,
                    new Rectangle(X + Width - BottomRightCorner.Width * Scale, Y + Height - BottomRightCorner.Height * Scale, BottomRightCorner.Width * Scale,
                        BottomRightCorner.Height * Scale), BottomRightCorner, false);
            if (BottomLeftCorner != Rectangle.Empty)
                renderer.DrawTexture(PanelTexture, new Rectangle(X, Y + Height - BottomLeftCorner.Height * Scale, BottomLeftCorner.Width * Scale, BottomLeftCorner.Height * Scale),
                    BottomLeftCorner, false);
        }
    }
}