#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Image.PNG;
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
        // Const
        public const bool CLIP_GLYPH_TEXTURES = false;
        public const int SDF_ATLAS_SIZE = 512;

        // Settings
        public static float SuperSample = 1;
        public static bool SdfBlockingCache = false;

        // Resources
        private static ShaderAsset _noDiscardShader;
        private static ShaderAsset _windingAaShader;
        private static ShaderAsset _windingShader;
        private static ShaderAsset _sdfGeneratingShader;
        private static ShaderAsset _sdfShader;

        private static RenderState _glyphRenderState;
        private static FrameBuffer _intermediateBuffer;
        private static FrameBuffer _intermediateBufferTwo;

        private static object _initLock = new object();
        private static bool _init;

        public static void InitEmotionRenderer()
        {
            if (_init) return;
            lock (_initLock)
            {
                if (_init) return;

                if (_glyphRenderState == null)
                {
                    _glyphRenderState = RenderState.Default.Clone();
                    _glyphRenderState.ViewMatrix = false;
                    _glyphRenderState.SFactorRgb = BlendingFactor.One;
                    _glyphRenderState.DFactorRgb = BlendingFactor.One;
                    _glyphRenderState.SFactorA = BlendingFactor.One;
                    _glyphRenderState.DFactorA = BlendingFactor.One;
                }

                _noDiscardShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
                _windingAaShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/WindingAA.xml");
                _windingShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/Winding.xml");
                _sdfGeneratingShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GenerateSDF.xml");
                _sdfShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");
                if (_noDiscardShader == null || _windingAaShader == null || _sdfGeneratingShader == null || _sdfShader == null)
                    Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.FontParser);

                _init = true;
            }
        }

        public static GlyphRendererState AddGlyphsToAtlas(DrawableFontAtlas atlas, GlyphRendererState state, List<AtlasGlyph> glyphsToAdd)
        {
            InitEmotionRenderer();

            bool justCreated = state == null;
            state = CommonGlyphRenderer.PrepareGlyphRenderer(atlas, state, glyphsToAdd, out Rectangle[] intermediateAtlasUVs, out Vector2 intermediateAtlasSize, 2);

            // Prepare for rendering.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(intermediateAtlasSize).WithColor();
            else
                _intermediateBuffer.Resize(intermediateAtlasSize, true);

            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Render glyphs to the intermediate buffer.
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader.Shader);
            var colors = new[]
            {
                new Color(1, 0, 0, 0),
                new Color(0, 1, 0, 0),
                new Color(0, 0, 1, 0),
                new Color(0, 0, 0, 1),
            };
            var offsets = new[]
            {
                new Vector3(-0.30f, 0, 0),
                new Vector3(0.30f, 0, 0),
                new Vector3(0.0f, -0.30f, 0),
                new Vector3(0.0f, 0.30f, 0),
            };
            for (var p = 0; p < 4; p++)
            {
                uint clr = colors[p].ToUint();
                Vector3 offset = offsets[p];
                RenderGlyphs(composer, glyphsToAdd, offset, clr, atlas.RenderScale, intermediateAtlasUVs);
            }

            composer.RenderTargetPop();

            // Copy to atlas buffer, while unwinding.
            composer.RenderTo(state.AtlasBuffer);
            if (justCreated) composer.ClearFrameBuffer();
            composer.SetAlphaBlend(false);
            composer.SetShader(_windingAaShader.Shader);
            _windingAaShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);

            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                AtlasGlyph atlasGlyph = glyphsToAdd[i];
                Rectangle placeWithinIntermediateAtlas = intermediateAtlasUVs[i];
                Debug.Assert(atlasGlyph.UVSize == placeWithinIntermediateAtlas.Size);
                composer.RenderSprite(new Vector3(atlasGlyph.UVLocation.X, atlasGlyph.UVLocation.Y, 0), atlasGlyph.UVSize, _intermediateBuffer.ColorAttachment, placeWithinIntermediateAtlas);
            }

            composer.SetShader();
            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(prevState);

            return state;
        }

        /// <summary>
        /// Render glyph vertices storing the winding number in the color buffer.
        /// </summary>
        private static void RenderGlyphs(RenderComposer composer, List<AtlasGlyph> glyphs, Vector3 offset, uint clr, float scale, Rectangle[] dstRects = null)
        {
            for (var c = 0; c < glyphs.Count; c++)
            {
                AtlasGlyph atlasGlyph = glyphs[c];
                Glyph fontGlyph = atlasGlyph.FontGlyph;
                Rectangle dst = dstRects != null ? dstRects[c] : new Rectangle(atlasGlyph.UVLocation, atlasGlyph.UVSize);
                if (CLIP_GLYPH_TEXTURES) composer.SetClipRect(dst);
                Vector3 atlasRenderPos = (dst.Position * SuperSample - new Vector2(atlasGlyph.XMin * SuperSample, -atlasGlyph.Height * SuperSample)).ToVec3();
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

        public static void RenderAtlasSDF(DrawableFontAtlas fontAtl, bool skipCache = false)
        {
            // Initialize shared objects.
            Debug.Assert(GLThread.IsGLThread());
            RenderComposer composer = Engine.Renderer;

            // Set render shader to atlas.
            fontAtl.FontShader = _sdfShader.Shader;

            // Create temporary atlas to house high resolution glyph data.
            var sdfAtlasTemp = new DrawableFontAtlas(fontAtl.Font, fontAtl.Font.Height / 4f); // fontAtl.FirstChar, fontAtl.NumChars
            Vector2 tempAtlasSize = sdfAtlasTemp.Texture.Size;
            List<AtlasGlyph> glyphs = sdfAtlasTemp.DrawableAtlasGlyphs;
            float scale = sdfAtlasTemp.RenderScale;

            // Calculate output texture size, and maintain aspect ratio.
            var outputResolution = new Vector2(SDF_ATLAS_SIZE);
            if (tempAtlasSize.X > tempAtlasSize.Y)
            {
                float newY = outputResolution.X * tempAtlasSize.Y / tempAtlasSize.X;
                outputResolution.Y = newY;
            }
            else
            {
                float newX = outputResolution.Y * tempAtlasSize.X / tempAtlasSize.Y;
                outputResolution.X = newX;
            }

            // Apply sdf metrics to the font atlas by scaling the temp atlas.
            static void ApplySDFScaledMetric(Vector2 outputResolution, DrawableFontAtlas fontAtl, DrawableFontAtlas sdfAtlasTemp, out float lowestGlyph, out float lowestGlyphOutput)
            {
                float scaleDifference = outputResolution.X / sdfAtlasTemp.Texture.Size.X;
                lowestGlyph = 0;
                lowestGlyphOutput = 0;
                for (var i = 0; i < sdfAtlasTemp.DrawableAtlasGlyphs.Count; i++)
                {
                    AtlasGlyph g = sdfAtlasTemp.DrawableAtlasGlyphs[i];
                    AtlasGlyph fontAtlGlyph = fontAtl.DrawableAtlasGlyphs[i];
                    Debug.Assert(fontAtlGlyph.FontGlyph == g.FontGlyph);
                    fontAtlGlyph.UVLocation = g.UVLocation * scaleDifference;
                    fontAtlGlyph.UVSize = g.UVSize * scaleDifference;

                    lowestGlyph = MathF.Max(g.UVLocation.Y + g.UVSize.Y + 50, lowestGlyph);
                }

                fontAtl.RenderScale = scaleDifference;
                lowestGlyphOutput = lowestGlyph * scaleDifference;
            }

            // Check if a cached texture exists, and if so load it and early out.
            var cachedRenderName = $"Player/SDFCache/{fontAtl.Font.FullName}-{fontAtl.RenderedWith}.png";
            bool cachedImageExists = Engine.AssetLoader.Exists(cachedRenderName);
            if (cachedImageExists && !skipCache)
            {
                if (SdfBlockingCache)
                {
                    var texture = Engine.AssetLoader.Get<TextureAsset>(cachedRenderName);
                    if (texture != null && texture.Texture.Size.X >= outputResolution.X)
                    {
                        //fontAtl.SetTexture(texture.Texture);
                        outputResolution = new Vector2(texture.Texture.Size.X);
                        ApplySDFScaledMetric(outputResolution, fontAtl, sdfAtlasTemp, out float _, out float __);
                        return;
                    }
                }
                else
                {
                    Task.Run(async () =>
                    {
                        var texture = await Engine.AssetLoader.GetAsync<TextureAsset>(cachedRenderName);
                        if (texture != null)
                        {
                            // Check if the cached sdf image is the same size, or larger than the one requested.
                            // We use only the width since we cut off the height in cache generation to save space.
                            if (texture.Texture.Size.X >= outputResolution.X)
                            {
                                //fontAtl.SetTexture(texture.Texture);
                                outputResolution = new Vector2(texture.Texture.Size.X);
                                ApplySDFScaledMetric(outputResolution, fontAtl, sdfAtlasTemp, out float _, out float __);
                            }
                            else
                            {
                                // Turns out texture sucked so restart process.
                                _ = GLThread.ExecuteGLThreadAsync(() => RenderAtlasSDF(fontAtl, true));
                            }
                        }
                    });
                    return;
                }
            }

            // Set SDF locations and sizes.
            ApplySDFScaledMetric(outputResolution, fontAtl, sdfAtlasTemp, out float lowestGlyph, out float lowestGlyphOutput);

            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Render glyphs just like the normal Emotion rasterizer.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(tempAtlasSize).WithColor();
            else
                _intermediateBuffer.Resize(tempAtlasSize, true);

            //RenderDocGraphicsContext.RenderDocCaptureStart();
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader.Shader);
            uint clr = new Color(1, 0, 0, 0).ToUint();
            Vector3 offset = Vector3.Zero;
            RenderGlyphs(composer, glyphs, offset, clr, scale);
            composer.RenderTargetPop();

            // Unwind glyphs.
            if (_intermediateBufferTwo == null)
                _intermediateBufferTwo = new FrameBuffer(tempAtlasSize).WithColor();
            else
                _intermediateBufferTwo.Resize(tempAtlasSize, true);

            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(_intermediateBufferTwo);
            composer.SetShader(_windingShader.Shader);
            _windingShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);
            composer.RenderFrameBuffer(_intermediateBuffer);
            composer.SetShader();
            composer.RenderTargetPop();
            //RenderDocGraphicsContext.RenderDocCaptureEnd();
            _intermediateBufferTwo.ColorAttachment.Smooth = true;

            // Downsize image and find distances.
            FrameBuffer atlasFinal = new FrameBuffer(outputResolution).WithColor();
            atlasFinal.ColorAttachment.Smooth = true;
            //RenderDocGraphicsContext.RenderDocCaptureStart();
            composer.SetShader(_sdfGeneratingShader.Shader);
            composer.RenderToAndClear(atlasFinal);
            composer.RenderFrameBuffer(
                _intermediateBufferTwo,
                new Vector2(outputResolution.X, -outputResolution.Y),
                new Vector3(0, outputResolution.Y, 0));
            composer.RenderTargetPop();
            composer.SetShader();
            atlasFinal.ColorAttachment.FlipY = false; // We drew it flipped.

            //fontAtl.SetTexture(atlasFinal.ColorAttachment);
            composer.PopModelMatrix();
            composer.SetState(prevState);

            // Save to cache so we don't have to do this again.
            byte[] data = atlasFinal.Sample(new Rectangle(0, 0, atlasFinal.Size), PixelFormat.Rgba);
            byte[] pngData = PngFormat.Encode(data, atlasFinal.Size, PixelFormat.Rgba);
            Engine.AssetLoader.Save(pngData, cachedRenderName, false);
        }
    }
}