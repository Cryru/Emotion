// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Components.UI;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Systems.UI
{
    public class BoxRenderer : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(Box)};
        }

        protected internal override void Setup()
        {
        }

        internal override void Update(Entity link)
        {
        }

        internal override void Draw(Entity link)
        {
            // Get components.
            Box box = link.GetComponent<Box>();

            // Correct texture if missing.
            Texture2D texture = box.Texture ?? AssetLoader.BlankTexture;

            Core.Context.Ink.Start(DrawLocation.Screen);

            // Draw top horizontal line.
            for (int i = box.TopLeft.Width; i <= link.Width - box.HorizontalTop.Width; i += box.HorizontalTop.Width)
            {
                DrawHelper(link, box.HorizontalTop, texture, new Vector2(i, 0));
            }

            // Draw left vertical line.
            for (int i = box.TopLeft.Height; i <= link.Height - box.VerticalLeft.Height; i += box.VerticalLeft.Height)
            {
                DrawHelper(link, box.VerticalLeft, texture, new Vector2(0, i));
            }

            // Draw right vertical line.
            for (int i = box.TopRight.Height;
                i <= link.Height - box.VerticalRight.Height;
                i += box.VerticalRight.Height)
            {
                DrawHelper(link, box.VerticalRight, texture, new Vector2(link.Width - box.VerticalRight.Width, i));
            }

            // Draw bottom horizontal line.
            for (int i = box.BottomLeft.Width;
                i <= link.Width - box.HorizontalBottom.Width;
                i += box.HorizontalBottom.Width)
            {
                DrawHelper(link, box.HorizontalBottom, texture,
                    new Vector2(i, link.Height - box.HorizontalBottom.Height));
            }

            // Draw top left corner.
            DrawHelper(link, box.TopLeft, texture, new Vector2(0, 0));

            // Draw top right corner.
            DrawHelper(link, box.TopRight, texture, new Vector2(link.Width - box.TopRight.Width, 0));

            // Draw bottom left.
            DrawHelper(link, box.BottomLeft, texture, new Vector2(0, link.Height - box.BottomLeft.Height));

            // Draw bottom right.
            DrawHelper(link, box.BottomRight, texture,
                new Vector2(link.Width - box.BottomRight.Width, link.Height - box.BottomRight.Height));

            //Draw fill.
            Rectangle fillBounds = new Rectangle
            {
                X = (int) (link.X + box.VerticalLeft.Width),
                Y = (int) (link.Y + box.HorizontalTop.Height),
                Width = (int) (link.Width - box.VerticalLeft.Width - box.VerticalRight.Width),
                Height = (int) (link.Height - box.HorizontalBottom.Height - box.HorizontalTop.Height)
            };

            Core.Context.Ink.Draw(
                texture,
                new Rectangle(fillBounds.X, fillBounds.Y, fillBounds.Width, fillBounds.Height),
                box.Fill,
                Color.White,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                1f);

            Core.Context.Ink.End();
        }

        private static void DrawHelper(Entity link, Rectangle area, Texture2D texture, Vector2 position)
        {
            int width = area.Width;
            int height = area.Height;
            int x = (int) (link.X + position.X);
            int y = (int) (link.Y + position.Y);

            Core.Context.Ink.Draw(
                texture,
                new Rectangle(x, y, width, height),
                area,
                Color.White,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                1f);
        }
    }
}