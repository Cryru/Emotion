#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
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

                NewRendererGlyph gr = new NewRendererGlyph();
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
            for (int i = 0; i < glyphsAdded.Count; i++)
            {
                var glyph = glyphsAdded[i];
                var oldFontGlyph = glyphsToAdd[i].FontGlyph;
                var fontGlyph = newFont.Glyphs[oldFontGlyph.MapIndex];

                float left = fontGlyph.LeftSideBearing * scale;
                Vector2 glyphPos = new Vector2(glyph.x0, glyph.y0 + baseline) + new Vector2(sdfDist - left, sdfDist);
                painter.DrawGlyph(glyph, glyphPos, scale, sdfDist);
            }

            if(LastProducedSdf != null) return;

            var buffer = new FrameBuffer(new Vector2(atlasSize, maxHeight)).WithColor().WithDepth(true);

            var renderer = Engine.Renderer;
            var lineData = painter.lp.vertices.ToArray();

            Engine.Host.Context.RenderDoc.StartCapture();

            VertexBuffer vbo = new VertexBuffer();
            vbo.Upload(lineData);
            VertexArrayObject<SdfVertex> vao = new VertexArrayObject<SdfVertex>(vbo);
            renderer.RenderToAndClear(buffer);
            var shader = Engine.AssetLoader.Get<ShaderAsset>("linetest.xml");
            renderer.SetShader(shader.Shader);
            renderer.SetAlphaBlend(false);
            VertexArrayObject.EnsureBound(vao);
            VertexBuffer.EnsureBound(vbo.Pointer);
           
            Gl.DrawArrays(PrimitiveType.Triangles, 0, lineData.Length);
            renderer.SetShader();
            renderer.RenderTo(null);
            renderer.SetAlphaBlend(true);

            Engine.Host.Context.RenderDoc.EndCapture();

            LastProducedSdf = buffer;
        }

        public class GlyphPainter
        {
            public LinePainter lp = new LinePainter();
            public FillPainter fp = new FillPainter();

            public void DrawGlyph(NewRendererGlyph glyph, Vector2 pos, float scale, float sdfDist)
            {
                FontGlyph fontGlyph = glyph.FontGlyph;
                GlyphDrawCommand[] fontCommands = fontGlyph.Commands;
                if (fontCommands == null || fontCommands.Length == 0) return;

                for (var i = 0; i < fontCommands.Length; i++)
                {
                    GlyphDrawCommand cmd = fontCommands[i];
                    switch (cmd.Type)
                    {
                        case GlyphDrawCommandType.Move:
                        {
                            Vector2 p0 = cmd.P0 * scale + pos;
                            fp.move_to(p0);
                            lp.move_to(p0);
                            break;
                        }
                        case GlyphDrawCommandType.Line:
                        {
                            Vector2 p0 = cmd.P0 * scale + pos;
                            fp.line_to(p0);
                            lp.line_to(p0, sdfDist);
                            break;
                        }
                        //case VertexTypeFlag.Cubic:
                        case GlyphDrawCommandType.Curve:
                        {
                            Vector2 p0 = cmd.P0 * scale + pos;
                            Vector2 p1 = cmd.P1 * scale + pos;
                            fp.qbez_to(p0, p1);
                            lp.qbez_to(p0, p1, sdfDist);
                            break;
                        }
                        case GlyphDrawCommandType.Close:
                        {
                            fp.close();
                            lp.close(sdfDist);
                            break;
                        }
                    }

                    if (lp.vertices.Count >= 34)
                    {
                        bool a = true;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SdfVertex
        {
            [VertexAttribute(2, false)] public Vector2 Pos;

            // Vertex pos in parabola space
            [VertexAttribute(2, false)] public Vector2 Parabola;

            // Parabolic segment
            [VertexAttribute(2, false)] public Vector2 Limit;

            [VertexAttribute(1, false)] public float Scale;

            [VertexAttribute(1, false)] public float LineWidth;

            public SdfVertex(Vector2 pos, float lineWidth)
            {
                Pos = pos;
                Parabola = Vector2.Zero;
                Limit = Vector2.Zero;
                Scale = 0f;
                LineWidth = lineWidth;
            }

            public SdfVertex(Vector2 pos, Vector2 parabola, Vector2 limit, float scale, float lineWidth)
            {
                Pos = pos;
                Parabola = parabola;
                Limit = limit;
                Scale = scale;
                LineWidth = lineWidth;
            }
        }

        public static void FillTriangle(Vector2 p0, Vector2 p1, Vector2 p2, List<SdfVertex> vertices)
        {
            SdfVertex v0, v1, v2;
            v0 = new SdfVertex(p0, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            v1 = new SdfVertex(p1, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            v2 = new SdfVertex(p2, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
        }

        public class FillPainter
        {
            private Vector2 fan_pos;
            private Vector2 prev_pos;
            private List<SdfVertex> vertices = new List<SdfVertex>();

            public void move_to(Vector2 p0)
            {
                fan_pos = p0;
                prev_pos = p0;
            }

            public void line_to(Vector2 p1)
            {
                FillTriangle(fan_pos, prev_pos, p1, vertices);
                prev_pos = p1;
            }

            public void qbez_to(Vector2 p1, Vector2 p2)
            {
                FillTriangle(fan_pos, prev_pos, p2, vertices);

                var v0 = new SdfVertex(prev_pos, new Vector2(-1.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
                var v1 = new SdfVertex(p1, new Vector2(0.0f, -1.0f), new Vector2(0.0f), 0.0f, 0.0f);
                var v2 = new SdfVertex(p2, new Vector2(1.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                prev_pos = p2;
            }

            public void close()
            {
                if ((fan_pos - prev_pos).LengthSquared() < 1e-7) return;
                line_to(fan_pos);
            }
        }

        public static Vector2 perp_right(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        public static Vector2 perp_left(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        public static void set_par_vertex(ref SdfVertex v, Parabola par)
        {
            Vector2 par_pos = par.world_to_par(v.Pos);
            v.Parabola = par_pos;
            v.Limit = new Vector2(par.XStart, par.XEnd);
            v.Scale = par.Scale;
        }

        public enum QbezType
        {
            Parabola,
            Line,
            TwoLines
        }

        public static QbezType qbez_type(Vector2 np10, Vector2 np12)
        {
            float d = Vector2.Dot(np10, np12);
            float dmax = 1.0f - 1e-6f;
            if (d >= dmax) return QbezType.TwoLines;
            if (d <= -dmax) return QbezType.Line;
            return QbezType.Parabola;
        }

        public class Parabola
        {
            public Matrix3x2 Matrix;
            public float Scale;
            public float XStart;
            public float XEnd;

            public static Parabola FromQuadratic(Vector2 p0, Vector2 p1, Vector2 p2)
            {
                Parabola res = new Parabola();

                Vector2 pc = Vector2.Lerp(p0, p2, 0.5f);
                Vector2 yaxis = Vector2.Normalize(pc - p1);
                Vector2 xaxis = perp_right(yaxis);

                Vector2 p01 = Vector2.Normalize(p1 - p0);
                Vector2 p12 = Vector2.Normalize(p2 - p1);
                float cx0 = Vector2.Dot(xaxis, p01);
                float sx0 = Vector2.Dot(yaxis, p01);
                float cx2 = Vector2.Dot(xaxis, p12);
                float sx2 = Vector2.Dot(yaxis, p12);

                float x0 = sx0 / cx0 * 0.5f;
                float x2 = sx2 / cx2 * 0.5f;
                float y0 = x0 * x0;

                float p02x = Vector2.Dot(p2 - p0, xaxis);
                float scale = p02x / (x2 - x0);

                Vector2 vertex = p0 - new Vector2(y0 * scale) * yaxis - new Vector2(x0 * scale) * xaxis;

                res.Scale = scale;
                res.Matrix = new Matrix3x2(xaxis.X, xaxis.Y, yaxis.X, yaxis.Y, vertex.X, vertex.Y);

                if (x0 < x2)
                {
                    res.XStart = x0;
                    res.XEnd = x2;
                }
                else
                {
                    res.XStart = x2;
                    res.XEnd = x0;
                }

                return res;
            }

            public static Parabola FromLine(Vector2 p0, Vector2 p1)
            {
                float px0 = 100.0f;
                float px1 = 100.0f + 1e-3f;

                float py0 = px0 * px0;
                float py1 = px1 * px1;
                float plen = (new Vector2(px0, py0) - new Vector2(px1, py1)).Length();
                float pslope = 2.0f * px0 / MathF.Sqrt(1.0f * 4.0f * px0 * px0);
                float len = (p1 - p0).Length();
                Vector2 ldir = (p1 - p0) / len;
                Vector2 lndir = perp_right(ldir);
                float scale = len / plen;
                Vector2 rc = p0 + (ldir * (px1 - px0) + lndir * (py1 - py0)) * pslope * scale;
                Vector2 px = Vector2.Normalize(rc - p0);
                Vector2 py = perp_left(px);
                Vector2 pvertex = p0 - px * scale * px0 - py * scale * py0;

                Parabola res = new Parabola();

                res.Matrix = new Matrix3x2(px.X, px.Y, py.X, py.Y, pvertex.X, pvertex.Y);
                res.Scale = scale;
                res.XStart = px0;
                res.XEnd = px1;
                return res;
            }

            public Vector2 world_to_par(Vector2 pos)
            {
                float iss = 1.0f / Scale;
                Vector2 dpos = pos - new Vector2(Matrix.M31, Matrix.M32);
                Vector2 r0 = dpos * new Vector2(Matrix.M11, Matrix.M12);
                Vector2 r1 = dpos * new Vector2(Matrix.M21, Matrix.M22);
                float v0 = iss * (r0.X + r0.Y);
                float v1 = iss * (r1.X + r1.Y);
                return new Vector2(v0, v1);
            }
        }

        public class LinePainter
        {
            private Vector2 start_pos;
            private Vector2 prev_pos;
            public List<SdfVertex> vertices = new List<SdfVertex>();

            public void move_to(Vector2 p0)
            {
                start_pos = p0;
                prev_pos = p0;
            }

            public void line_to(Vector2 p1, float lineWidth)
            {
                Vector2 vmin = Vector2.Min(prev_pos, p1);
                Vector2 vmax = Vector2.Max(prev_pos, p1);

                vmin -= new Vector2(lineWidth);
                vmax += new Vector2(lineWidth);

                Parabola par = Parabola.FromLine(prev_pos, p1);
                LineRect(par, vmin, vmax, lineWidth, vertices);

                prev_pos = p1;
            }

            public void qbez_to(Vector2 p1, Vector2 p2, float line_width)
            {
                Vector2 p0 = prev_pos;

                Vector2 mid01 = new Vector2(0.5f) * (p0 + p1);
                Vector2 mid12 = new Vector2(0.5f) * (p1 + p2);

                Vector2 vmin = Vector2.Min(p0, mid01);
                vmin = Vector2.Min(vmin, mid12);
                vmin = Vector2.Min(vmin, p2);

                Vector2 vmax = Vector2.Max(p0, mid01);
                vmax = Vector2.Max(vmax, mid12);
                vmax = Vector2.Max(vmax, p2);

                vmin -= new Vector2(line_width);
                vmax += new Vector2(line_width);

                Vector2 v10 = p0 - p1;
                Vector2 v12 = p2 - p1;
                Vector2 np10 = Vector2.Normalize(v10);
                Vector2 np12 = Vector2.Normalize(v12);

                QbezType qtype = qbez_type(np10, np12);

                switch (qtype)
                {
                    case QbezType.Parabola:
                    {
                        var par = Parabola.FromQuadratic(p0, p1, p2);
                        LineRect(par, vmin, vmax, line_width, vertices);
                        break;
                    }
                    case QbezType.Line:
                    {
                        var par = Parabola.FromLine(p0, p2);
                        LineRect(par, vmin, vmax, line_width, vertices);
                        break;
                    }
                    case QbezType.TwoLines:
                    {
                        float l10 = v10.Length();
                        float l12 = v12.Length();
                        float qt = l10 / (l10 + l12);
                        float nqt = 1.0f - qt;
                        Vector2 qtop = p0 * (nqt * nqt) + p1 * (2.0f * nqt * qt) + p2 * (qt * qt);
                        Parabola par0 = Parabola.FromLine(p0, qtop);
                        LineRect(par0, vmin, vmax, line_width, vertices);
                        Parabola par1 = Parabola.FromLine(qtop, p1);
                        LineRect(par1, vmin, vmax, line_width, vertices);
                        break;
                    }
                }

                prev_pos = p2;
            }

            public static void LineRect(Parabola par, Vector2 vmin, Vector2 vmax, float lineWidth, List<SdfVertex> vertices)
            {
                SdfVertex v0, v1, v2, v3;
                v0 = new SdfVertex(new Vector2(vmin.X, vmin.Y), lineWidth);
                v1 = new SdfVertex(new Vector2(vmax.X, vmin.Y), lineWidth);
                v2 = new SdfVertex(new Vector2(vmax.X, vmax.Y), lineWidth);
                v3 = new SdfVertex(new Vector2(vmin.X, vmax.Y), lineWidth);

                set_par_vertex(ref v0, par);
                set_par_vertex(ref v1, par);
                set_par_vertex(ref v2, par);
                set_par_vertex(ref v3, par);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                vertices.Add(v0);
                vertices.Add(v2);
                vertices.Add(v3);
            }

            public void close(float lineWidth)
            {
                if ((start_pos - prev_pos).LengthSquared() < 1e-7) return;
                line_to(start_pos, lineWidth);
            }
        }


        public static GlyphRendererState AddGlyphsToAtlas(DrawableFontAtlas atlas, GlyphRendererState state, List<AtlasGlyph> glyphsToAdd)
        {
            state ??= new GlyphRendererState();
            GenerateSdf(atlas.Font, glyphsToAdd);

            return state;
        }
    }
}