#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using OpenGL;

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

        public enum RenderLineMode
        {
            Center,
            Inward,
            Outward
        }

        /// <summary>
        /// Render a line made out of quads.
        /// </summary>
        /// <param name="pointOne">The point to start the line.</param>
        /// <param name="pointTwo">The point to end the line at.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line in world units. The line will always be at least 1 pixel thick.</param>
        /// <param name="snapToPixel">Whether to snap the start and ending positions to the nearest pixel.</param>
        /// <param name="renderMode">How to treat the points given.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1f, bool snapToPixel = true, RenderLineMode renderMode = RenderLineMode.Center)
        {
            bool cameraWasOn = CurrentState.ViewMatrix!.Value;
            SetUseViewMatrix(false);
            ProjectionBehavior oldProjection = CurrentState.ProjectionBehavior!.Value;
            SetProjectionBehavior(ProjectionBehavior.AlwaysCameraProjection);

            Matrix4x4 viewMatrix = cameraWasOn ? Camera.ViewMatrix : Matrix4x4.Identity;
            if (cameraWasOn) thickness *= Camera.CalculatedScale;

            pointOne = Vector3.Transform(pointOne, ModelMatrix * viewMatrix);
            pointTwo = Vector3.Transform(pointTwo, ModelMatrix * viewMatrix);

            PushModelMatrix(Matrix4x4.Identity, false);

            if (snapToPixel)
            {
                if (thickness < 1.0f) thickness = 1.0f;
                pointOne = pointOne.IntCastRoundXY();
                pointTwo = pointTwo.IntCastRoundXY();
            }

            Vector3 direction = Vector3.Normalize(pointTwo - pointOne);
            var normal = new Vector3(-direction.Y, direction.X, 0);
            Vector3 delta = normal * (thickness / 2f);
            Vector3 deltaNeg = -delta;

            if (renderMode == RenderLineMode.Inward)
            {
                pointOne += delta;
                pointTwo += delta;
            }
            else if (renderMode == RenderLineMode.Outward)
            {
                pointOne -= delta;
                pointTwo -= delta;
            }

            Span<VertexData> vertices = RenderStream.GetStreamMemory(4, BatchMode.Quad);
            vertices[0].Vertex = pointOne + delta;
            vertices[1].Vertex = pointTwo + delta;
            vertices[2].Vertex = pointTwo + deltaNeg;
            vertices[3].Vertex = pointOne + deltaNeg;

            uint c = color.ToUint();
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
                vertices[i].UV = Vector2.Zero;
            }

            PopModelMatrix();
            SetUseViewMatrix(cameraWasOn);
            SetProjectionBehavior(oldProjection);
        }

        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, bool, RenderLineMode)" />
        public void RenderLine(Vector2 pointOne, Vector2 pointTwo, Color color, float thickness = 1f, bool snapToPixel = true)
        {
            RenderLine(pointOne.ToVec3(), pointTwo.ToVec3(), color, thickness, snapToPixel);
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
        /// <inheritdoc cref="RenderLine(Vector3, Vector3, Color, float, bool, RenderLineMode)" />
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
        /// <param name="snapToPixel">Whether to snap the line points to the nearest pixel.</param>
        public void RenderOutline(Vector3 position, Vector2 size, Color color, float thickness = 1, bool snapToPixel = true)
        {
            Vector3 nn = position;
            Vector3 pn = new Vector3(position.X + size.X, position.Y, position.Z);
            Vector3 np = new Vector3(position.X, position.Y + size.Y, position.Z);
            Vector3 pp = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);

            RenderLine(nn, pn, color, thickness, snapToPixel, RenderLineMode.Inward);
            RenderLine(pn, pp, color, thickness, snapToPixel, RenderLineMode.Inward);
            RenderLine(pp, np, color, thickness, snapToPixel, RenderLineMode.Inward);
            RenderLine(np, nn, color, thickness, snapToPixel, RenderLineMode.Inward);
        }

        /// <inheritdoc cref="RenderOutline(Vector3, Vector2, Color, float, bool)" />
        public void RenderOutline(Rectangle rect, Color color, float thickness = 1)
        {
            RenderOutline(new Vector3(rect.Position, 0), rect.Size, color, thickness);
        }

        private void RenderStringInner(Vector3 position, Color color, string text, DrawableFontAtlas atlas, TextLayouter layouter)
        {
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
            RenderStringInner(position, color, text, atlas, layouter);
            atlas.FinishDrawing(this);
        }

        private static ShaderAsset _sdfShader;
        private static ShaderAsset _sdfShaderSubPixel;

        public void RenderStringTest(Vector3 position, Color color, string text, DrawableFontAtlas atlas, TextLayouter layouter = null)
        {
            if (atlas.RenderedWith != GlyphRasterizer.Astiopin)
            {
                RenderString(position, color, text, atlas, layouter);
                return;
            }
            

            if (atlas?.Glyphs == null) return;
            layouter ??= new TextLayouter(atlas);

            var useSubpixelHinting = false;
#if WEB
            useSubpixelHinting = false;
#endif
            _sdfShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/AstiopinSDF.xml");
            ShaderProgram shaderProg = _sdfShader.Shader;
            if (useSubpixelHinting)
            {
                _sdfShaderSubPixel ??= _sdfShader.GetShaderVariation("SUBPIXEL_HINTING");
                shaderProg = _sdfShaderSubPixel.Shader;
            }

            var sdf = TestGlyphRenderer.LastProducedSdf;
            var metrics = TestGlyphRenderer.LastProducedMetrics;

            float fontSize = atlas.FontSize;
            float desiredTotalScale = fontSize / atlas.Font.UnitsPerEm;
            float scaleDiffCap = metrics.CapHeight / desiredTotalScale;
            float scaleDiffLow = metrics.CapHeight / desiredTotalScale;

            // We use separate scale for the low case characters
            // so that x-height fits the pixel grid.
            // Other characters use cap-height to fit to the pixels
            float capScale = fontSize / metrics.CapHeight;
            float lowScale = MathF.Round(capScale / metrics.CapHeight) * metrics.CapHeight;

            // Ascent should be a whole number since it's used to calculate the baseline
            // position which should lie at the pixel boundary
            // This also applies to the line height.
            float ascent = MathF.Round(metrics.Ascent * capScale);
            float extraLineGap = fontSize * 0.2f;
            float lineHeight = MathF.Round(capScale * (metrics.Ascent + metrics.Descent + metrics.LineGap) + extraLineGap);

            SetShader(shaderProg);
            shaderProg.SetUniformVector2("sdf_tex_size", sdf.Size);
            shaderProg.SetUniformFloat("sdf_border_size", metrics.Y / 2f);
            shaderProg.SetUniformFloat("vertexZ", position.Z);

            if (useSubpixelHinting)
                SetAlphaBlendType(BlendingFactor.Src1Color, BlendingFactor.OneMinusSrcColor, BlendingFactor.Src1Alpha, BlendingFactor.OneMinusSrcAlpha);

            float widthUsed = 0;
            Vector2 pen = position.ToVec2();
            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    if (pen.X > widthUsed) widthUsed = pen.X;
                    pen.X = position.X;
                    pen.Y -= lineHeight;
                    continue;
                }

                if (c == ' ')
                {
                    pen.X += metrics.Aspect * metrics.SpaceAdvance * capScale;
                    continue;
                }

                if (atlas.Glyphs.TryGetValue(c, out AtlasGlyph glyph))
                {
                    bool isLowerCase = char.IsLower(c);

                    // Low case chars use their own scale
                    float scale = isLowerCase ? lowScale : capScale;

                    Span<VertexData> memory = RenderStream.GetStreamMemory(4, BatchMode.Quad, sdf.ColorAttachment);

                    // Pen position is at the top of the line, Y goes up
                    float top = pen.Y + (ascent + scale * (metrics.Descent + metrics.Y));
                    float bottom = -(scale * metrics.RowHeight);

                    float left = pen.X + metrics.Aspect * scale * (glyph.LSB - metrics.X);
                    float right = metrics.Aspect * scale * (glyph.UVSize.X / sdf.Size.X);

                    pen.X += metrics.Aspect * scale * glyph.Advance;

                    VertexData.SpriteToVertexData(memory, new Vector3(left, top, scale), new Vector2(right, bottom), color,
                        sdf.ColorAttachment, new Rectangle(glyph.UVLocation, glyph.UVSize));
                }
            }

            SetShader();
            SetDefaultAlphaBlendType();
        }

        private FrameBuffer _outlineFb;

        /// <summary>
        /// Render a string from an atlas.
        /// </summary>
        public void RenderOutlinedString(Vector3 position, Color color, string text, float outlineWidth, Color outlineColor, DrawableFontAtlas atlas, TextLayouter layouter = null)
        {
            if (atlas?.Glyphs == null) return;
            layouter ??= new TextLayouter(atlas);

            // Create effect using fancy SDF effects.
            if (atlas.RenderedWith == GlyphRasterizer.EmotionSDFVer3)
            {
                float scaleFactor = 0.5f / atlas.SdfSize;
                float outlineWidthInSdf = outlineWidth * scaleFactor;
                if (outlineWidthInSdf < 0.5f) // Exceeding SDF range
                {
                    atlas.SetupDrawing(this, text);
                    atlas.FontShader.SetUniformFloat("outlineWidthDist", outlineWidthInSdf);
                    atlas.FontShader.SetUniformColor("outlineColor", outlineColor);

                    RenderStringInner(position, color, text, atlas, layouter);

                    FlushRenderStream();
                    atlas.FontShader.SetUniformFloat("outlineWidthDist", 0.0f);
                    atlas.FinishDrawing(this);
                    return;
                }
            }

            // Hack the effect with an extra allocated framebuffer.
            Vector2 drawPadding = atlas.GlyphDrawPadding;
            Vector2 drawPaddingT2 = drawPadding * 2;
            Vector2 stringSize = Vector2.Zero;
            foreach (char c in text)
            {
                Vector2 gPos = layouter.AddLetter(c, out AtlasGlyph g);
                if (g == null || g.UVSize == Vector2.Zero) continue;
                stringSize = Vector2.Max(stringSize, new Vector2(gPos.X + g.Size.X + drawPaddingT2.X + outlineWidth * 2, gPos.Y + g.Size.Y + drawPaddingT2.Y + outlineWidth * 2));
            }

            if (layouter is TextLayouterWrap wrapper) wrapper.RestartPen();

            // Render the text to the framebuffer.
            if (_outlineFb == null)
                _outlineFb = new FrameBuffer(stringSize).WithColor();
            else
                _outlineFb.Resize(stringSize, true);

            var renderOffset = new Vector3(outlineWidth, outlineWidth, 0);
            Vector3 finalRenderPos = position - renderOffset;

            // Draw to the FB unaffected by the current state.
            RenderToAndClear(_outlineFb);
            PushModelMatrix(Matrix4x4.Identity, false);
            bool useViewMatrix = CurrentState.ViewMatrix ?? false;
            SetUseViewMatrix(false);

            RenderString(renderOffset, color, text, atlas, layouter);

            // Return the state to the user set one.
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