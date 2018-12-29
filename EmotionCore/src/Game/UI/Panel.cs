// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI
{
    public class Panel : ParentControl
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

        public override void Render()
        {
            // Check if a texture is set.
            if (PanelTexture == null) return;



            // Draw fill.
            if (Fill != Rectangle.Empty) Context.Renderer.Render(Vector3.Zero, Size, Color.White, PanelTexture, Fill);

            // Draw lines.
            if (Top != Rectangle.Empty)
                for (float x = 0; x <= Width - Top.Width * Scale; x += Top.Width * Scale)
                {
                    Context.Renderer.Render(new Vector3(x, 0, 0), new Vector2(Top.Width * Scale, Top.Height * Scale), Color.White, PanelTexture, Top);
                }

            if (Left != Rectangle.Empty)
                for (float y = 0; y <= Height - Left.Height * Scale; y += Left.Height * Scale)
                {
                    Context.Renderer.Render(new Vector3(0, y, 0), new Vector2(Left.Width * Scale, Left.Height * Scale), Color.White, PanelTexture, Left);
                }

            if (Right != Rectangle.Empty)
                for (float y = 0; y <= Height - Right.Height * Scale; y += Right.Height * Scale)
                {
                    Context.Renderer.Render(new Vector3(Width - Right.Width * Scale, y, 0), new Vector2(Right.Width * Scale, Right.Height * Scale), Color.White, PanelTexture, Right);
                }

            if (Bottom != Rectangle.Empty)
                for (float x = 0; x <= Width - Bottom.Width * Scale; x += Bottom.Width * Scale)
                {
                    Context.Renderer.Render(new Vector3(x, Height - Bottom.Height * Scale, 0), new Vector2(Bottom.Width * Scale, Bottom.Height * Scale), Color.White, PanelTexture, Bottom);
                }

            // Draw corners.
            if (TopLeftCorner != Rectangle.Empty)
                Context.Renderer.Render(Vector3.Zero, new Vector2(TopLeftCorner.Width * Scale, TopLeftCorner.Height * Scale), Color.White, PanelTexture, TopLeftCorner);
            if (TopRightCorner != Rectangle.Empty)
                Context.Renderer.Render(new Vector3(Width - TopRightCorner.Width * Scale, 0, 0), new Vector2(TopRightCorner.Width * Scale, TopRightCorner.Height * Scale), Color.White,
                    PanelTexture,
                    TopRightCorner);
            if (BottomRightCorner != Rectangle.Empty)
                Context.Renderer.Render(new Vector3(Width - BottomRightCorner.Width * Scale, Height - BottomRightCorner.Height * Scale, 0), new Vector2(BottomRightCorner.Width * Scale,
                    BottomRightCorner.Height * Scale), Color.White, PanelTexture, BottomRightCorner);
            if (BottomLeftCorner != Rectangle.Empty)
                Context.Renderer.Render(new Vector3(0, Height - BottomLeftCorner.Height * Scale, 0), new Vector2(BottomLeftCorner.Width * Scale, BottomLeftCorner.Height * Scale), Color.White,
                    PanelTexture, BottomLeftCorner);



            // Draw children.
            base.Render();
        }
    }
}