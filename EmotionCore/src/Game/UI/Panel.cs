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

        public Panel(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public override void Draw(Renderer renderer)
        {
            // Check if a texture is set.
            if (PanelTexture == null) return;

            renderer.RenderFlush();

            // Draw fill.
            if (Fill != Rectangle.Empty) renderer.RenderQueue(Position, Size, Color.White, PanelTexture, Fill);

            // Draw lines.
            if (Top != Rectangle.Empty)
                for (float x = 0; x <= Width - Top.Width * Scale; x += Top.Width * Scale)
                {
                    renderer.RenderQueue(new Vector3(X + x, Y, Z), new Vector2(Top.Width * Scale, Top.Height * Scale), Color.White, PanelTexture, Top);
                }

            if (Left != Rectangle.Empty)
                for (float y = 0; y <= Height - Left.Height * Scale; y += Left.Height * Scale)
                {
                    renderer.RenderQueue(new Vector3(X, Y + y, Z), new Vector2(Left.Width * Scale, Left.Height * Scale), Color.White, PanelTexture, Left);
                }

            if (Right != Rectangle.Empty)
                for (float y = 0; y <= Height - Right.Height * Scale; y += Right.Height * Scale)
                {
                    renderer.RenderQueue(new Vector3(X + Width - Right.Width * Scale, Y + y, Z), new Vector2(Right.Width * Scale, Right.Height * Scale), Color.White, PanelTexture, Right);
                }

            if (Bottom != Rectangle.Empty)
                for (float x = 0; x <= Width - Bottom.Width * Scale; x += Bottom.Width * Scale)
                {
                    renderer.RenderQueue(new Vector3(X + x, Y + Height - Bottom.Height * Scale, Z), new Vector2(Bottom.Width * Scale, Bottom.Height * Scale), Color.White, PanelTexture, Bottom);
                }

            // Draw corners.
            if (TopLeftCorner != Rectangle.Empty)
                renderer.RenderQueue(new Vector3(X, Y, Z), new Vector2(TopLeftCorner.Width * Scale, TopLeftCorner.Height * Scale), Color.White, PanelTexture, TopLeftCorner);
            if (TopRightCorner != Rectangle.Empty)
                renderer.RenderQueue(new Vector3(X + Width - TopRightCorner.Width * Scale, Y, Z), new Vector2(TopRightCorner.Width * Scale, TopRightCorner.Height * Scale), Color.White, PanelTexture,
                    TopRightCorner);
            if (BottomRightCorner != Rectangle.Empty)
                renderer.RenderQueue(new Vector3(X + Width - BottomRightCorner.Width * Scale, Y + Height - BottomRightCorner.Height * Scale, Z), new Vector2(BottomRightCorner.Width * Scale,
                    BottomRightCorner.Height * Scale), Color.White, PanelTexture, BottomRightCorner);
            if (BottomLeftCorner != Rectangle.Empty)
                renderer.RenderQueue(new Vector3(X, Y + Height - BottomLeftCorner.Height * Scale, Z), new Vector2(BottomLeftCorner.Width * Scale, BottomLeftCorner.Height * Scale), Color.White,
                    PanelTexture, BottomLeftCorner);

            renderer.RenderFlush();
        }
    }
}