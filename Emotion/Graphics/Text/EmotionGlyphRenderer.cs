#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#pragma warning disable 162
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode

namespace Emotion.Graphics.Text
{
    public static class EmotionGlyphRenderer
    {
        public static float SuperSample = 1;
        public const bool CLIP_GLYPH_TEXTURES = false;
        private static ShaderAsset _noDiscardShader;
        private static ShaderAsset _windingAaShader;

        public static void RenderAtlas(DrawableFontAtlas fontAtl)
        {
            Debug.Assert(GLThread.IsGLThread());
            RenderComposer composer = Engine.Renderer;
            _noDiscardShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
            _windingAaShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/WindingAA.xml");
            if (_noDiscardShader == null || _windingAaShader == null)
            {
                Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.FontParser);
                return;
            }

            Vector2 atlasSize = fontAtl.AtlasSize;
            List<AtlasGlyph> glyphs = fontAtl.DrawableAtlasGlyphs;
            float scale = fontAtl.RenderScale;

            // Create two framebuffers. Glyphs are rendered with their winding in the first one which is then blitted over to the final framebuffer,
            // which is set as the drawable atlas texture.
            FrameBuffer atlasIntermediate = new FrameBuffer(atlasSize * SuperSample).WithColor();
            FrameBuffer atlasFinal = new FrameBuffer(atlasSize).WithColor();
            composer.SetState(RenderState.Default);

            //RenderDocGraphicsContext.RenderDocCaptureStart();
            composer.RenderToAndClear(atlasIntermediate);
            composer.SetShader(_noDiscardShader.Shader);
            composer.SetUseViewMatrix(false);
            composer.SetAlphaBlendType(BlendingFactor.One, BlendingFactor.One, BlendingFactor.One, BlendingFactor.One);

            var colors = new[]
            {
                new Color(1, 0, 0, 0),
                new Color(0, 1, 0, 0),
                new Color(0, 0, 1, 0),
                new Color(0, 0, 0, 1),
            };
            var offsets = new[]
            {
                new Vector3(-0.25f, 0, 0),
                new Vector3(0.25f, 0, 0),
                new Vector3(0.0f, -0.25f, 0),
                new Vector3(0.0f, 0.25f, 0),
            };
            for (var p = 0; p < 4; p++)
            {
                uint clr = colors[p].ToUint();
                Vector3 offset = offsets[p];

                for (var c = 0; c < glyphs.Count; c++)
                {
                    AtlasGlyph atlasGlyph = glyphs[c];
                    Glyph fontGlyph = atlasGlyph.FontGlyph;
                    if (CLIP_GLYPH_TEXTURES) composer.SetClipRect(new Rectangle(atlasGlyph.UVLocation, atlasGlyph.UVSize));
                    Vector3 atlasRenderPos = (atlasGlyph.UVLocation * SuperSample - new Vector2(atlasGlyph.XMin * SuperSample, -atlasGlyph.Height * SuperSample)).ToVec3();
                    atlasRenderPos += offset;
                    composer.PushModelMatrix(Matrix4x4.CreateScale(scale * SuperSample, -scale * SuperSample, 1) * Matrix4x4.CreateTranslation(atlasRenderPos));

                    // Count vertices.
                    GlyphVertex[] verts = fontGlyph.Vertices;
                    var verticesCount = 0;
                    for (var v = 0; v < verts.Length; v++)
                    {
                        GlyphVertex currentVert = verts[v];
                        if (currentVert.TypeFlag != VertexTypeFlag.Move) verticesCount++;
                    }

                    // Draw lines between all vertex points.
                    Span<VertexData> lines = composer.RenderStream.GetStreamMemory((uint)(verticesCount * 3), BatchMode.SequentialTriangles);
                    for (var j = 0; j < lines.Length; j++)
                    {
                        lines[j].UV = Vector2.Zero;
                        lines[j].Color = clr;
                    }

                    var currentVertIdx = 0;
                    for (var i = 1; i < verts.Length; i++)
                    {
                        GlyphVertex currentVert = verts[i];
                        if (currentVert.TypeFlag == VertexTypeFlag.Move) continue;
                        var currentVertPos = new Vector3(currentVert.X, currentVert.Y, 0);

                        GlyphVertex prevVert = verts[i - 1];
                        var prevVertPos = new Vector3(prevVert.X, prevVert.Y, 0);

                        lines[currentVertIdx].Vertex = Vector3.Zero;
                        currentVertIdx++;
                        lines[currentVertIdx].Vertex = currentVertPos;
                        currentVertIdx++;
                        lines[currentVertIdx].Vertex = prevVertPos;
                        currentVertIdx++;
                    }

                    // Draw curves. These will flip pixels in the curve approximations from above.
                    for (var i = 1; i < verts.Length; i++)
                    {
                        GlyphVertex currentVert = verts[i];
                        var currentVertPos = new Vector3(currentVert.X, currentVert.Y, 0);
                        if (currentVert.TypeFlag == VertexTypeFlag.Curve)
                        {
                            GlyphVertex prevVert = verts[i - 1];
                            var prevVertPos = new Vector3(prevVert.X, prevVert.Y, 0);

                            Span<VertexData> memory = composer.GetStreamedQuadraticCurveMesh(prevVertPos, currentVertPos,
                                new Vector3(currentVert.Cx, currentVert.Cy, 0));
                            for (var j = 0; j < memory.Length; j++)
                            {
                                memory[j].UV = Vector2.Zero;
                                memory[j].Color = clr;
                            }
                        }
                        else if (currentVert.TypeFlag == VertexTypeFlag.Cubic)
                        {
                            GlyphVertex prevVert = verts[i - 1];
                            var prevVertPos = new Vector3(prevVert.X, prevVert.Y, 0);

                            Span<VertexData> memory = composer.GetStreamedCubicCurveMesh(prevVertPos, currentVertPos,
                                new Vector3(currentVert.Cx, currentVert.Cy, 0),
                                new Vector3(currentVert.Cx1, currentVert.Cy1, 0));
                            for (var j = 0; j < memory.Length; j++)
                            {
                                memory[j].UV = Vector2.Zero;
                                memory[j].Color = clr;
                            }
                        }
                    }

                    composer.PopModelMatrix();
                    if (CLIP_GLYPH_TEXTURES) composer.SetClipRect(null);
                }
            }

            composer.RenderTargetPop();

            composer.SetDefaultAlphaBlendType();
            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(atlasFinal);
            composer.SetShader(_windingAaShader.Shader);
            _windingAaShader.Shader.SetUniformVector2("drawSize", atlasFinal.Size);
            composer.RenderFrameBuffer(atlasIntermediate, new Vector2(atlasFinal.Size.X, -atlasFinal.Size.Y), color: Color.White, pos: new Vector3(0, atlasFinal.Size.Y, 0));
            composer.SetShader();
            composer.RenderTo(null);
            //RenderDocGraphicsContext.RenderDocCaptureEnd();

            atlasFinal.ColorAttachment.FlipY = false; // We drew it flipped, so it's okay.
            atlasIntermediate.Dispose();
            fontAtl.SetTexture(atlasFinal.ColorAttachment);
        }
    }
}