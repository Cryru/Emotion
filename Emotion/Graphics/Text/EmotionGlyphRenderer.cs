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
        public static float SuperSample = 1;
        public const bool CLIP_GLYPH_TEXTURES = false;
        public const int SDF_ATLAS_SIZE = 512;
        public static bool SdfBlockingCache = false;

        private static ShaderAsset _noDiscardShader;
        private static ShaderAsset _windingAaShader;
        private static ShaderAsset _windingShader;
        private static ShaderAsset _sdfGeneratingShader;
        private static ShaderAsset _sdfShader;

        private static RenderState _glyphRenderState;
        private static FrameBuffer _intermediateBuffer;

        private static void RenderGlyphs(RenderComposer composer, List<AtlasGlyph> glyphs, Vector3 offset, uint clr, float scale)
        {
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

        public static void RenderAtlas(DrawableFontAtlas fontAtl)
        {
            // Initialize shared objects.
            Debug.Assert(GLThread.IsGLThread());
            RenderComposer composer = Engine.Renderer;
            _noDiscardShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
            _windingAaShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/WindingAA.xml");
            if (_noDiscardShader == null || _windingAaShader == null)
            {
                Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.FontParser);
                return;
            }

            if (_glyphRenderState == null)
            {
                _glyphRenderState = RenderState.Default.Clone();
                _glyphRenderState.ViewMatrix = false;
                _glyphRenderState.SFactorRgb = BlendingFactor.One;
                _glyphRenderState.DFactorRgb = BlendingFactor.One;
                _glyphRenderState.SFactorA = BlendingFactor.One;
                _glyphRenderState.DFactorA = BlendingFactor.One;
            }

            Vector2 atlasSize = fontAtl.AtlasSize;
            List<AtlasGlyph> glyphs = fontAtl.DrawableAtlasGlyphs;
            float scale = fontAtl.RenderScale;

            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(atlasSize * SuperSample).WithColor();
            else
                _intermediateBuffer.Resize(atlasSize * SuperSample, true);

            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Glyphs are rendered with their winding in the first one which is then copied over to the final framebuffer, which is set as the drawable atlas texture.
            //RenderDocGraphicsContext.RenderDocCaptureStart();
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
                RenderGlyphs(composer, glyphs, offset, clr, scale);
            }

            composer.RenderTargetPop();

            FrameBuffer atlasFinal = new FrameBuffer(atlasSize).WithColor();
            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(atlasFinal);
            composer.SetShader(_windingAaShader.Shader);
            _windingAaShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);
            composer.RenderFrameBuffer(_intermediateBuffer, new Vector2(atlasFinal.Size.X, -atlasFinal.Size.Y), color: Color.White, pos: new Vector3(0, atlasFinal.Size.Y, 0));
            composer.SetShader();
            composer.RenderTo(null);
            //RenderDocGraphicsContext.RenderDocCaptureEnd();

            atlasFinal.ColorAttachment.FlipY = false; // We drew it flipped, so it's okay.
            fontAtl.SetTexture(atlasFinal.ColorAttachment);
            composer.PopModelMatrix();
            composer.SetState(prevState);
        }

        public static void RenderAtlasSDF(DrawableFontAtlas fontAtl)
        {
            // Initialize shared objects.
            Debug.Assert(GLThread.IsGLThread());
            RenderComposer composer = Engine.Renderer;

            _noDiscardShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
            _windingShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/Winding.xml");
            _sdfGeneratingShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GenerateSDF.xml");
            if (_noDiscardShader == null || _windingShader == null || _sdfGeneratingShader == null)
            {
                Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.FontParser);
                return;
            }

            if (_glyphRenderState == null)
            {
                _glyphRenderState = RenderState.Default.Clone();
                _glyphRenderState.ViewMatrix = false;
                _glyphRenderState.SFactorRgb = BlendingFactor.One;
                _glyphRenderState.DFactorRgb = BlendingFactor.One;
                _glyphRenderState.SFactorA = BlendingFactor.One;
                _glyphRenderState.DFactorA = BlendingFactor.One;
            }

            // Create temporary atlas to house high resolution glyph data.
            var sdfAtlasTemp = new DrawableFontAtlas(fontAtl.Font, fontAtl.Font.Height / 4f, fontAtl.FirstChar, fontAtl.NumChars);
            Vector2 tempAtlasSize = sdfAtlasTemp.AtlasSize;
            List<AtlasGlyph> glyphs = sdfAtlasTemp.DrawableAtlasGlyphs;
            float scale = sdfAtlasTemp.RenderScale;

            // Calculate output texture size, and maintain aspect ratio.
            var outputResolution = new Vector2(SDF_ATLAS_SIZE);
            if (fontAtl.AtlasSize.X >= SDF_ATLAS_SIZE || fontAtl.AtlasSize.Y >= SDF_ATLAS_SIZE)
                outputResolution = new Vector2(MathF.Max(fontAtl.AtlasSize.X, fontAtl.AtlasSize.Y) * 2);
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

            // Set SDF locations and sizes.
            // This is done before the texture is actually loaded/rasterized so the cache can have access to it.
            Vector2 scaleDifference = outputResolution / tempAtlasSize;
            float lowestGlyph = 0, lowestGlyphOutput = 0;
            for (var i = 0; i < sdfAtlasTemp.DrawableAtlasGlyphs.Count; i++)
            {
                AtlasGlyph g = sdfAtlasTemp.DrawableAtlasGlyphs[i];
                AtlasGlyph fontAtlGlyph = fontAtl.DrawableAtlasGlyphs[i];
                Debug.Assert(fontAtlGlyph.FontGlyph == g.FontGlyph);
                fontAtlGlyph.UVLocation = g.UVLocation * scaleDifference;
                fontAtlGlyph.UVSize = g.UVSize * scaleDifference;

                lowestGlyph = MathF.Max(g.UVLocation.Y + g.UVSize.Y + 50, lowestGlyph);
            }

            fontAtl.RenderScale = scaleDifference.X;
            lowestGlyphOutput = lowestGlyph * scaleDifference.Y;

            // Set render shader to atlas.
            _sdfShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");
            if (_sdfShader == null)
                Engine.Log.Error("SDF shader missing!", MessageSource.FontParser);
            else
                fontAtl.FontShader = _sdfShader.Shader;

            // Check if a cached texture exists, and if so load it and early out.
            var cachedRenderName = $"Player/SDFCache/{fontAtl.Font.FullName}-{fontAtl.FirstChar}-{fontAtl.NumChars}-{fontAtl.RenderedWith}-{outputResolution.X}-{outputResolution.Y}";
            bool cachedImageExists = Engine.AssetLoader.Exists($"{cachedRenderName}.png");
            if (cachedImageExists)
            {
                if (SdfBlockingCache)
                {
                    var texture = Engine.AssetLoader.Get<TextureAsset>($"{cachedRenderName}.png");
                    if (texture == null) return;
                    fontAtl.SetTexture(texture.Texture);
                }
                else
                {
                    Engine.AssetLoader.GetAsync<TextureAsset>($"{cachedRenderName}.png").ContinueWith(t =>
                    {
                        TextureAsset textureAss = t.Result;
                        if (textureAss == null) return;
                        fontAtl.SetTexture(textureAss.Texture);
                    });
                }

                return;
            }

            // Render glyphs just like the normal Emotion rasterizer.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(tempAtlasSize).WithColor();
            else
                _intermediateBuffer.Resize(tempAtlasSize, true);

            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Create high resolution atlas image.
            //RenderDocGraphicsContext.RenderDocCaptureStart();
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader.Shader);
            uint clr = new Color(1, 0, 0, 0).ToUint();
            Vector3 offset = Vector3.Zero;
            RenderGlyphs(composer, glyphs, offset, clr, scale);
            composer.RenderTargetPop();

            FrameBuffer atlasFinal = new FrameBuffer(tempAtlasSize).WithColor();
            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(atlasFinal);
            composer.SetShader(_windingShader.Shader);
            _windingShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);
            composer.RenderFrameBuffer(_intermediateBuffer, new Vector2(atlasFinal.Size.X, -atlasFinal.Size.Y), color: Color.White, pos: new Vector3(0, atlasFinal.Size.Y, 0));
            composer.SetShader();
            composer.RenderTargetPop();
            //RenderDocGraphicsContext.RenderDocCaptureEnd();
            atlasFinal.ColorAttachment.Smooth = true;
            atlasFinal.ColorAttachment.FlipY = false; // We drew it flipped.

            // Downsize image and find distances..
            //RenderDocGraphicsContext.RenderDocCaptureStart();
            composer.SetShader(_sdfGeneratingShader.Shader);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.RenderSprite(Vector3.Zero, atlasFinal.ColorAttachment.Size, Color.White, atlasFinal.ColorAttachment);
            composer.RenderTargetPop();
            composer.SetShader();

            // Copy back to resized atlas.
            outputResolution.Y = lowestGlyphOutput;
            atlasFinal.Resize(outputResolution);
            _intermediateBuffer.ColorAttachment.Smooth = true;
            composer.RenderToAndClear(atlasFinal);
            composer.RenderFrameBuffer(_intermediateBuffer,
                new Vector2(outputResolution.X, -outputResolution.Y),
                color: Color.White,
                pos: new Vector3(0, outputResolution.Y, 0),
                uv: new Rectangle(0, 0, _intermediateBuffer.Size.X, lowestGlyph));
            composer.RenderTo(null);
            _intermediateBuffer.ColorAttachment.Smooth = false;
            //RenderDocGraphicsContext.RenderDocCaptureEnd();

            fontAtl.SetTexture(atlasFinal.ColorAttachment);
            composer.PopModelMatrix();
            composer.SetState(prevState);

            // Save to cache so we don't have to do this again.
            byte[] data = atlasFinal.Sample(new Rectangle(0, 0, atlasFinal.Size), PixelFormat.Rgba);
            byte[] pngData = PngFormat.Encode(data, atlasFinal.Size, PixelFormat.Rgba);
            Engine.AssetLoader.Save(pngData, $"{cachedRenderName}.png", false);
        }
    }
}