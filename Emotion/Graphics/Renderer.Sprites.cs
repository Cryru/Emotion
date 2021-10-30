#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    // Command routines for sprites (and sprite-like) which use the RenderComposer.
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
            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad, texture);
            VertexData.SpriteToVertexData(vertices, position, size, color, texture, textureArea, flipX, flipY);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Transform transform, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(transform.Position, transform.Size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Rectangle rect, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(rect.Position.ToVec3(), rect.Size, color, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector3 position, Vector2 size, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position, size, Color.White, texture, textureArea);
        }

        /// <inheritdoc cref="RenderSprite(Vector3, Vector2, Color, Texture, Rectangle?, bool, bool)" />
        public void RenderSprite(Vector2 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderSprite(position.ToVec3(), size, color, texture, textureArea);
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in world units. The line will always be at least 1 pixel thick.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            if (thickness < 1.0f) thickness = 1.0f;

            pointOne = pointOne.IntCastRound();
            pointTwo = pointTwo.IntCastRound();

            Vector3 direction = Vector3.Normalize(pointTwo - pointOne);
            var normal = new Vector3(-direction.Y, direction.X, 0);
            Vector3 delta = normal * (thickness / 2f);

            pointOne += delta; // Move to pixel center.
            pointTwo += delta;

            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
            vertices[0].Vertex = pointOne + delta;
            vertices[1].Vertex = pointTwo + delta;
            vertices[2].Vertex = pointTwo - delta;
            vertices[3].Vertex = pointOne - delta;

            uint c = color.ToUint();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
                vertices[i].UV = Vector2.Zero;
            }
        }

        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float)" />
        public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
        }

        /// <summary>
        /// Render a line in projected screen space. This allows for more accurate lines, with a visible Z dimension.
        /// </summary>
        public void RenderLineScreenSpace(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            if (thickness < 1.0f) thickness = 1.0f;

            bool? old = CurrentState.ViewMatrix;
            SetUseViewMatrix(false);

            pointOne = Vector3.Transform(pointOne, Camera.ViewMatrix * ModelMatrix);
            pointTwo = Vector3.Transform(pointTwo, Camera.ViewMatrix * ModelMatrix);
            PushModelMatrix(Matrix4x4.Identity, false);

            pointOne = pointOne.IntCastRound();
            pointTwo = pointTwo.IntCastRound();

            Vector3 direction = Vector3.Normalize(pointTwo - pointOne);
            var normal = new Vector3(-direction.Y, direction.X, 0);
            Vector3 delta = normal * (thickness / 2f);

            pointOne += delta; // Move to pixel center.
            pointTwo += delta;

            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
            vertices[0].Vertex = pointOne + delta;
            vertices[1].Vertex = pointTwo + delta;
            vertices[2].Vertex = pointTwo - delta;
            vertices[3].Vertex = pointOne - delta;

            uint c = color.ToUint();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
                vertices[i].UV = Vector2.Zero;
            }

            PopModelMatrix();
            SetUseViewMatrix(old ?? true);
        }

        /// <summary>
        /// Render a line, from a line segment.
        /// </summary>
        /// <param name="segment">The line segment to render.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void RenderLine(ref LineSegment segment, Color color, float thickness = 1f)
        {
            RenderLine(segment.Start, segment.End, color, thickness);
        }

        /// <summary>
        /// Render a line with an arrow at the end.
        /// </summary>
        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float)" />
        public void RenderArrow(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f)
        {
            RenderLine(pointOne, pointTwo, color, thickness);

            Vector3 diff = pointTwo - pointOne;
            const float maxArrowHeadLength = 10;
            float length = Math.Min(diff.Length() / 2, maxArrowHeadLength);
            float width = length / 2;

            Vector3 direction = Vector3.Normalize(diff);
            var normal = new Vector3(-direction.Y, direction.X, 0);
            Vector3 lengthDelta = length * direction;
            Vector3 delta = width * normal;
            Vector3 arrowPointOne = pointTwo - lengthDelta + delta;
            Vector3 arrowPointTwo = pointTwo - lengthDelta - delta;

            RenderLine(pointTwo, arrowPointOne, color, thickness);
            RenderLine(pointTwo, arrowPointTwo, color, thickness);
        }

        /// <inheritdoc cref="RenderArrow(Vector3, Vector3, Color, float)" />
        public void RenderArrow(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f)
        {
            RenderArrow(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness);
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

        /// <inheritdoc cref="RenderOutline(Vector3, Vector2, Color, float)" />
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
        /// <param name="layouter">The layouter to use.</param>
        public void RenderString(Vector3 position, Color color, string text, DrawableFontAtlas atlas, TextLayouter layouter = null)
        {
            if (atlas?.Glyphs == null) return;
            layouter ??= new TextLayouter(atlas);

            atlas.SetupDrawing(this, text);

            Vector2 drawPadding = atlas.GlyphDrawPadding;
            Vector2 drawPaddingT2 = drawPadding * 2;
            var drawPadding3 = new Vector3(drawPadding.X, drawPadding.Y, 0);

            foreach (char c in text)
            {
                Vector2 gPos = layouter.AddLetter(c, out AtlasGlyph g);
                if (g == null || g.UVSize == Vector2.Zero) continue;
                //gPos.X = MathF.Ceiling(gPos.X);

                var uv = new Rectangle(g.UVLocation, g.UVSize);
                RenderSprite(new Vector3(position.X + gPos.X, position.Y + gPos.Y, position.Z) - drawPadding3, g.Size + drawPaddingT2, color, atlas.Texture, uv);
            }

            atlas.FinishDrawing(this);
        }

        private FrameBuffer _outlineFb;

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        public void RenderOutlinedString(Vector3 position, Color color, string text, float outlineWidth, Color outlineColor, DrawableFontAtlas atlas, TextLayouter layouter = null)
        {
            // To do: Use SDF effects instead of this dirty stuff.
            if (atlas?.Glyphs == null) return;
            layouter ??= new TextLayouter(atlas);

            // Calculate how big the text draw is, and allocate a framebuffer of that size.
            Vector2 drawPadding = atlas.GlyphDrawPadding;
            Vector2 drawPaddingT2 = drawPadding * 2;
            Vector2 drawPaddingT4 = drawPadding * 4;
            Vector2 stringSize = Vector2.Zero;
            foreach (char c in text)
            {
                Vector2 gPos = layouter.AddLetter(c, out AtlasGlyph g);
                if (g == null || g.UVSize == Vector2.Zero) continue;
                stringSize = Vector2.Max(stringSize, new Vector2(gPos.X + g.Size.X + drawPaddingT4.X + outlineWidth * 2, gPos.Y + g.Size.Y + drawPaddingT4.Y + outlineWidth * 2));
            }

            if (layouter is TextLayouterWrap wrapper) wrapper.RestartPen();

            // Render the text to the framebuffer.
            if (_outlineFb == null)
                _outlineFb = new FrameBuffer(stringSize).WithColor();
            else
                _outlineFb.Resize(stringSize, true);

            Vector3 renderOffset = new Vector3(drawPadding.X, drawPadding.Y, 0) + new Vector3(outlineWidth, outlineWidth, 0);
            Vector3 finalRenderPos = position - new Vector3(drawPaddingT2.X, drawPaddingT2.Y, 0) - new Vector3(outlineWidth, outlineWidth, 0);

            RenderToAndClear(_outlineFb);
            PushModelMatrix(Matrix4x4.Identity, false);
            bool useViewMatrix = CurrentState.ViewMatrix ?? false;
            SetUseViewMatrix(false);
            bool useAlphaBlend = CurrentState.AlphaBlending ?? false;
            SetAlphaBlend(false);
            RenderString(renderOffset, color, text, atlas, layouter);
            if(useAlphaBlend) SetAlphaBlend(true);
            PopModelMatrix();
            RenderTo(null);
            if (useViewMatrix) SetUseViewMatrix(true);

            // Copy with a shader than applies an outline.
            var outlineShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/OutlineShader.xml");
            if (outlineShader == null) return;

            SetShader(outlineShader.Shader);
            outlineShader.Shader.SetUniformFloat("outlineThickness", outlineWidth);
            RenderFrameBuffer(_outlineFb, pos: finalRenderPos, color: outlineColor);
            SetShader();
        }
    }
}