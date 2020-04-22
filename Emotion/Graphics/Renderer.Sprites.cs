#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Text;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.OpenType;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// Command routines for sprites (and sprite-like) which use the RenderComposer.
    /// </summary>
    public sealed partial class RenderComposer
    {
        /// <summary>
        /// Render a (textured) quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        /// <param name="flipX">Whether to flip the texture on the x axis.</param>
        /// <param name="flipY">Whether to flip the texture on the y axis.</param>
        public void RenderSprite(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, bool flipX = false, bool flipY = false)
        {
            SpriteBatchBase<VertexData> batch = GetBatch();
            Span<VertexData> vertices = batch.GetData(texture);
            Debug.Assert(vertices.Length >= 4);
            VertexData.SpriteToVertexData(vertices, position, size, color, texture, textureArea, flipX, flipY);
        }

        /// <summary>
        /// Render a (textured) quad to the screen using its transform.
        /// </summary>
        /// <param name="transform">The quad's transform.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void RenderSprite(Transform transform, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(transform.Position, transform.Size, color, texture, textureArea);
        }

        /// <summary>
        /// Render a (textured) quad to the screen.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void RenderSprite(Vector3 position, Vector2 size, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position, size, Color.White, texture, textureArea);
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(new Vector3(pointOne, 0), new Vector3(pointTwo, 0), color, thickness);
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            SpriteBatchBase<VertexData> batch = GetBatch();
            Span<VertexData> vertices = batch.GetData(null);

            Vector2 normal = Vector2.Normalize(new Vector2(pointTwo.Y - pointOne.Y, -(pointTwo.X - pointOne.X))) * thickness;
            float z = Math.Max(pointOne.Z, pointTwo.Z);

            vertices[0].Vertex = new Vector3(pointOne.X + normal.X, pointOne.Y + normal.Y, z);
            vertices[1].Vertex = new Vector3(pointTwo.X + normal.X, pointTwo.Y + normal.Y, z);
            vertices[2].Vertex = new Vector3(pointTwo.X - normal.X, pointTwo.Y - normal.Y, z);
            vertices[3].Vertex = new Vector3(pointOne.X - normal.X, pointOne.Y - normal.Y, z);

            uint c = color.ToUint();
            vertices[0].Color = c;
            vertices[1].Color = c;
            vertices[2].Color = c;
            vertices[3].Color = c;
        }

        /// <summary>
        /// Render a rectangle outline.
        /// </summary>
        /// <param name="position">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderOutline(Vector3 position, Vector2 size, Color color, float thickness = 1)
        {
            RenderLine(position, new Vector3(position.X + size.X, position.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X + size.X, position.Y, position.Z), new Vector3(position.X + size.X, position.Y + size.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X + size.X, position.Y + size.Y, position.Z), new Vector3(position.X, position.Y + size.Y, position.Z), color, thickness);
            RenderLine(new Vector3(position.X, position.Y + size.Y, position.Z), position, color, thickness);
        }

        /// <summary>
        /// Render a rectangle outline.
        /// </summary>
        /// <param name="rect">The rectangle to outline.</param>
        /// <param name="color">The color of the lines.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderOutline(Rectangle rect, Color color, float thickness = 1)
        {
            RenderOutline(new Vector3(rect.Position, 0), rect.Size, color, thickness);
        }

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        /// <param name="position">The top left position of where to start drawing the string.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text itself.</param>
        /// <param name="atlas">The font atlas to use.</param>
        public void RenderString(Vector3 position, Color color, string text, DrawableFontAtlas atlas)
        {
            if (atlas?.Atlas?.Glyphs == null) return;
            RenderString(position, color, text, atlas, new TextLayouter(atlas.Atlas));
        }

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        /// <param name="position">The top left position of where to start drawing the string.</param>
        /// <param name="color">The text color.</param>
        /// <param name="text">The text itself.</param>
        /// <param name="atlas">The font atlas to use.</param>
        /// <param name="layouter">The layouter to use.</param>
        public void RenderString(Vector3 position, Color color, string text, DrawableFontAtlas atlas, TextLayouter layouter)
        {
            if (atlas?.Atlas?.Glyphs == null) return;
            position = position.RoundClosest();
            foreach (char c in text)
            {
                Vector2 gPos = layouter.AddLetter(c, out AtlasGlyph g);
                if (g == null) continue;
                var uv = new Rectangle(g.Location, g.UV);
                RenderSprite(new Vector3(position.X + gPos.X, position.Y + gPos.Y, position.Z), g.Size, color, atlas.Texture, uv);
            }
        }
    }
}