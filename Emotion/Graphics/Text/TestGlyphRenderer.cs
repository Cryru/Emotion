#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text.NewRenderer;
using Emotion.IO;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

namespace Emotion.Graphics.Text
{
    //https://github.com/astiopin/sdf_atlas
    //https://github.com/astiopin/webgl_fonts/blob/41b4253974d350ffb27a95fb9f929d3efee29f86/src/main.js
    public static class TestGlyphRenderer
    {
        public static FrameBuffer LastProducedSdf;

        public class NewRendererGlyph
        {
            public int glyph_idx;
            public FontGlyph FontGlyph;

            public float x0;
            public float y0;
            public float x1;
            public float y1;
        }

        public static void GenerateSdf(Font font, List<AtlasGlyph> glyphsToAdd, int atlasSize = 1024, int sdfDist = 16, int rowHeight = 96)
        {
            FontAnton newFont = font.NewFont;

            // pen
            float posx = 0;
            float posy = 0;
            float maxHeight = 0;

            float fheight = newFont.Ascender - newFont.Descender;
            float scale = rowHeight / fheight;
            float baseline = -newFont.Descender * scale;

            var glyphsAdded = new List<NewRendererGlyph>();
            for (int i = 0; i < glyphsToAdd.Count; i++)
            {
                var oldFontGlyph = glyphsToAdd[i].FontGlyph;

                var fontGlyph = newFont.Glyphs[oldFontGlyph.MapIndex];

                float rect_width = (fontGlyph.Max.X - fontGlyph.Min.X) * scale + sdfDist * 2.0f;
                float row_and_border = rowHeight + sdfDist * 2.0f;

                // New pen line.
                if (posx + rect_width > atlasSize)
                {
                    posx = 0.0f;
                    posy = MathF.Ceiling(posy + row_and_border);
                    maxHeight = MathF.Ceiling(posy + row_and_border);
                }

                var gr = new NewRendererGlyph();
                gr.glyph_idx = fontGlyph.MapIndex;
                gr.FontGlyph = fontGlyph;
                gr.x0 = posx;
                gr.x1 = posx + rect_width;
                gr.y0 = posy;
                gr.y1 = posy + row_and_border;
                glyphsAdded.Add(gr);

                posx = MathF.Ceiling(posx + rect_width);
            }

            var painter = new GlyphPainter();
            for (var i = 0; i < glyphsAdded.Count; i++)
            {
                NewRendererGlyph glyph = glyphsAdded[i];
                Glyph oldFontGlyph = glyphsToAdd[i].FontGlyph;
                FontGlyph fontGlyph = newFont.Glyphs[oldFontGlyph.MapIndex];

                float left = fontGlyph.LeftSideBearing * scale;
                Vector2 glyphPos = new Vector2(glyph.x0, glyph.y0 + baseline) + new Vector2(sdfDist - left, sdfDist);
                painter.RasterizeGlyph(glyph, glyphPos, scale, sdfDist);
            }

            if (LastProducedSdf != null) return;

            FrameBuffer buffer = new FrameBuffer(new Vector2(atlasSize, maxHeight)).WithColor().WithDepth(true);

            RenderComposer renderer = Engine.Renderer;
            // todo: ensure states here and cache framebuffer like emotion sdf
            // todo: use render stream

            // Draw sdf lines.
            SdfVertex[] lineData = painter.LinePainter.vertices.ToArray();
            var vbo = new VertexBuffer();
            vbo.Upload(lineData);
            var vao = new VertexArrayObject<SdfVertex>(vbo);
            renderer.RenderToAndClear(buffer);
            var shader = Engine.AssetLoader.Get<ShaderAsset>("linetest.xml");
            renderer.SetShader(shader.Shader);
            renderer.SetAlphaBlend(false);
            VertexArrayObject.EnsureBound(vao);
            VertexBuffer.EnsureBound(vbo.Pointer);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, lineData.Length);
            renderer.SetShader();

            // Draw fill


            renderer.RenderTo(null);
            renderer.SetAlphaBlend(true);

            LastProducedSdf = buffer;
        }

       



       





       

        


        public static GlyphRendererState AddGlyphsToAtlas(DrawableFontAtlas atlas, GlyphRendererState state, List<AtlasGlyph> glyphsToAdd)
        {
            state ??= new GlyphRendererState();
            GenerateSdf(atlas.Font, glyphsToAdd);

            return state;
        }
    }
}