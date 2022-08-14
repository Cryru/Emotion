#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text.NewRenderer;
using Emotion.IO;
using Emotion.Platform.Debugger;
using Emotion.Primitives;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text
{
    //https://github.com/astiopin/sdf_atlas
    //https://github.com/astiopin/webgl_fonts/blob/41b4253974d350ffb27a95fb9f929d3efee29f86/src/main.js

    /// <summary>
    /// This renderer is based on Anton Stiopin (astiopin)'s work.
    /// https://astiopin.github.io/2019/01/06/sdf-on-gpu.html
    /// </summary>
    public static class TestGlyphRenderer
    {
        public static FrameBuffer? LastProducedSdf;
        public static SdfMetrics? LastProducedMetrics;

        public class SdfMetrics
        {
            public float X;
            public float Y;
            public float RowHeight;
            public float Aspect;
            public float Ascent;
            public float Descent;
            public float LineGap;
            public float CapHeight;
            public float XHeight;
            public float SpaceAdvance;
        }

        public static void GenerateSdf(Font font, List<AtlasGlyph> glyphsToAdd, int atlasSize = 2048, int sdfDist = 32, int glyphSize = 256)
        {
            // pen
            float penX = 0;
            float penY = 0;
            float heightUsed = 0;

            float fontHeight = font.UnitsPerEm;
            float scale = glyphSize / fontHeight;
            float baseline = -font.Descender * scale;

            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                AtlasGlyph atlasGlyph = glyphsToAdd[i];
                FontGlyph fontGlyph = atlasGlyph.FontGlyph;

                float glyphWidth = (fontGlyph.Max.X - fontGlyph.Min.X) * scale + sdfDist * 2.0f;
                float glyphHeight = glyphSize + sdfDist * 2.0f;

                // New pen line.
                if (penX + glyphWidth > atlasSize)
                {
                    penX = 0.0f;
                    penY = MathF.Ceiling(penY + glyphHeight);
                    heightUsed = MathF.Ceiling(penY + glyphHeight);
                }

                atlasGlyph.UVLocation = new Vector2(penX, penY);
                atlasGlyph.UVSize = new Vector2(glyphWidth, glyphHeight);
                penX = MathF.Ceiling(penX + glyphWidth);
            }

            // Metrics
            float xScale = (glyphSize / (float) atlasSize) / fontHeight;
            float yScale = (glyphSize / heightUsed) / fontHeight;

            float ix = sdfDist / (float) atlasSize;
            float iy = sdfDist / heightUsed;
            float rowHeight = (glyphSize + 2.0f * sdfDist) / heightUsed;
            float aspectRatio = atlasSize / heightUsed;
            float ascent = font.Ascender * yScale;
            float descender = MathF.Abs(font.Descender * yScale);
            float lineGap = font.LineGap * yScale;

            // Try to estimate some font metrics from specific characters in the font. If they are missing approximate them.
            FontGlyph? spaceGlyph = font.CharToGlyph.GetValueOrDefault(' ');
            FontGlyph? capitalXGlyph = font.CharToGlyph.GetValueOrDefault('X');
            FontGlyph? lowerCaseXGlyph = font.CharToGlyph.GetValueOrDefault('x');

            float capHeight = capitalXGlyph != null ? capitalXGlyph.Max.Y * yScale : ascent * 0.66f;
            float lowHeight = lowerCaseXGlyph != null ? lowerCaseXGlyph.Max.Y * yScale : ascent * 0.55f;
            float spaceAdvance = spaceGlyph != null ? spaceGlyph.AdvanceWidth * xScale : 0.02f;

            LastProducedMetrics = new SdfMetrics()
            {
                X = ix,
                Y = iy,
                RowHeight = rowHeight,
                Aspect = aspectRatio,
                Ascent = ascent,
                Descent = descender,
                LineGap = lineGap,
                CapHeight = capHeight,
                XHeight = lowHeight,
                SpaceAdvance = spaceAdvance,
            };

            var painter = new GlyphPainter();
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                AtlasGlyph glyph = glyphsToAdd[i];
                FontGlyph fontGlyph = glyphsToAdd[i].FontGlyph;

                glyph.LSB = fontGlyph.LeftSideBearing * xScale;
                glyph.Advance = fontGlyph.AdvanceWidth * xScale;

                float left = fontGlyph.LeftSideBearing * scale;
                Vector2 glyphPos = new Vector2(glyph.UVLocation.X, glyph.UVLocation.Y + baseline) + new Vector2(sdfDist - left, sdfDist);
                painter.RasterizeGlyph(fontGlyph, glyphPos, scale, sdfDist);
            }

            if (LastProducedSdf != null) return;

            FrameBuffer buffer = new FrameBuffer(new Vector2(atlasSize, heightUsed)).WithColor(true, InternalFormat.Red, PixelFormat.Red).WithDepthStencil();
            LastProducedSdf = buffer;
            RenderComposer renderer = Engine.Renderer;
            // todo: ensure states here and cache framebuffer like emotion sdf

            RenderDoc.StartCapture();

            var lineRenderShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GlyphRenderLine.xml");
            var fillRenderShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GlyphRenderFill.xml");

            renderer.SetState(RenderState.Default);
            renderer.PushModelMatrix(Matrix4x4.Identity, false);
            renderer.SetAlphaBlend(false);
            renderer.SetDepthTest(true);
            renderer.SetUseViewMatrix(false);

            var renderStream = new RenderStreamBatch<SdfVertex>(0, 1, false);
            renderer.RenderToAndClear(buffer);

            // Draw lines
            renderer.SetShader(lineRenderShader.Shader);
            Span<SdfVertex> memory = renderStream.GetStreamMemory((uint) painter.LinePainter.Vertices.Count, BatchMode.SequentialTriangles);
            Span<SdfVertex> lineDataSpan = CollectionsMarshal.AsSpan(painter.LinePainter.Vertices);
            lineDataSpan.CopyTo(memory);
            renderStream.FlushRender();
            renderer.SetShader();

            // Draw fill
            renderer.SetDepthTest(false);
            renderer.SetStencilTest(true);
            renderer.StencilWindingStart();
            renderer.ToggleRenderColor(false);

            renderer.SetShader(fillRenderShader.Shader);
            memory = renderStream.GetStreamMemory((uint) painter.FillPainter.Vertices.Count, BatchMode.SequentialTriangles);
            Span<SdfVertex> fillDataSpan = CollectionsMarshal.AsSpan(painter.FillPainter.Vertices);
            fillDataSpan.CopyTo(memory);
            renderStream.FlushRender();
            renderer.SetShader();

            // draw full screen quad, inverting stencil where it is 1
            renderer.StencilWindingEnd();
            renderer.ToggleRenderColor(true);
            renderer.SetAlphaBlend(true);
            renderer.SetAlphaBlendType(BlendingFactor.OneMinusDstColor, BlendingFactor.Zero, BlendingFactor.One, BlendingFactor.Zero);
            Gl.StencilFunc(StencilFunction.Notequal, 0, 0xff);
            Gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);

            renderer.RenderSprite(Vector3.Zero, buffer.Size, Color.White);

            renderer.RenderTo(null);
            renderer.SetDefaultAlphaBlendType();
            renderer.SetAlphaBlend(true);
            renderer.SetStencilTest(false);
            renderer.PopModelMatrix();

            RenderDoc.EndCapture();

            LastProducedSdf = buffer;

            // Sample framebuffer and save it.
            //FrameBuffer atlasBuffer = buffer;
            //byte[] data = atlasBuffer.Sample(new Rectangle(0, 0, atlasBuffer.Size), PixelFormat.Red);
            //byte[] pngData = PngFormat.Encode(data, atlasBuffer.Size, PixelFormat.Red);
            //Engine.AssetLoader.Save(pngData, "Player/blabla.png", false);
        }

        public static GlyphRendererState AddGlyphsToAtlas(DrawableFontAtlas atlas, GlyphRendererState state, List<AtlasGlyph> glyphsToAdd)
        {
            state ??= new GlyphRendererState();
            GenerateSdf(atlas.Font, glyphsToAdd);

            return state;
        }
    }
}