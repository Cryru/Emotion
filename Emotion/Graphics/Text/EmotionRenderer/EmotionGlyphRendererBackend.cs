#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Primitives;
using Emotion.Standard.OpenType;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionRenderer
{
    public static class EmotionGlyphRendererBackend
    {
        public const bool CLIP_GLYPH_TEXTURES = false;

        /// <summary>
        /// Render glyph vertices storing the winding number in the color buffer.
        /// </summary>
        public static void RenderGlyphs(Font font, RenderComposer composer, List<DrawableGlyph> glyphs, Vector3 offset, uint clr, float scale, Rectangle[]? dstRects = null)
        {
            for (var c = 0; c < glyphs.Count; c++)
            {
                DrawableGlyph atlasGlyph = glyphs[c];
                FontGlyph fontGlyph = atlasGlyph.FontGlyph;

                Rectangle dst = dstRects != null ? dstRects[c] : atlasGlyph.GlyphUV;
#pragma warning disable CS0162
                if (CLIP_GLYPH_TEXTURES) composer.SetClipRect(dst);
#pragma warning restore CS0162

                float baseline = font.Descender * scale;
                Vector3 atlasRenderPos = (dst.Position - new Vector2(atlasGlyph.XBearing, baseline)).ToVec3();
                atlasRenderPos += offset;
                composer.PushModelMatrix(Matrix4x4.CreateScale(scale, scale, 1) * Matrix4x4.CreateTranslation(atlasRenderPos));

                // Count vertices.
                GlyphDrawCommand[]? commands = fontGlyph.Commands;
                if (commands == null) continue;

                var verticesCount = 0;
                for (var v = 0; v < commands.Length; v++)
                {
                    GlyphDrawCommand currentCommand = commands[v];
                    if (currentCommand.Type != GlyphDrawCommandType.Move) verticesCount++;
                }

                // Draw lines between all vertex points.
                Span<VertexData> lines = composer.RenderStream.GetStreamMemory((uint) (verticesCount * 3), BatchMode.SequentialTriangles);
                for (var j = 0; j < lines.Length; j++)
                {
                    lines[j].UV = Vector2.Zero;
                    lines[j].Color = clr;
                }

                Vector3 prevPos = Vector3.Zero;
                int currentContourStart = -1;
                var currentVertIdx = 0;
                for (var i = 0; i < commands.Length; i++)
                {
                    GlyphDrawCommand currentCommand = commands[i];
                    if (currentContourStart == -1) currentContourStart = i;
                    if (currentCommand.Type != GlyphDrawCommandType.Move)
                    {
                        Vector3 currentVertPos = currentCommand.P0.ToVec3();

                        if (currentCommand.Type == GlyphDrawCommandType.Close)
                        {
                            GlyphDrawCommand startingVert = commands[currentContourStart];
                            currentVertPos = startingVert.P0.ToVec3();
                            currentContourStart = -1;
                        }

                        lines[currentVertIdx].Vertex = Vector3.Zero;
                        currentVertIdx++;
                        lines[currentVertIdx].Vertex = currentVertPos;
                        currentVertIdx++;
                        lines[currentVertIdx].Vertex = prevPos;
                        currentVertIdx++;
                    }

                    prevPos = currentCommand.P0.ToVec3();
                }

                // Draw curves. These will flip pixels in the curve approximations from above.
                for (var i = 0; i < commands.Length; i++)
                {
                    GlyphDrawCommand currentCommand = commands[i];
                    if (currentCommand.Type == GlyphDrawCommandType.Curve)
                    {
                        Span<VertexData> memory = composer.GetStreamedQuadraticCurveMesh(prevPos, currentCommand.P0.ToVec3(), currentCommand.P1.ToVec3());
                        for (var j = 0; j < memory.Length; j++)
                        {
                            memory[j].UV = Vector2.Zero;
                            memory[j].Color = clr;
                        }
                    }

                    prevPos = currentCommand.P0.ToVec3();
                }

                composer.PopModelMatrix();
#pragma warning disable CS0162
                if (CLIP_GLYPH_TEXTURES) composer.SetClipRect(null);
#pragma warning restore CS0162
            }
        }
    }
}