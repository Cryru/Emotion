#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
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
        public const int SDF_REFERENCE_FONT_SIZE = 100;
        public const float SDF_HIGH_RES_SCALE = 1f;
        public static readonly Vector2 SdfAtlasGlyphSpacing = new Vector2(5);

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

        // https://medium.com/@calebfaith/implementing-msdf-font-in-opengl-ea09a9ab7e00
        // https://github.com/Chlumsky/msdfgen/issues/22
        public static GlyphRendererState AddGlyphsToAtlasSDF(DrawableFontAtlas atlas, GlyphRendererState _, List<AtlasGlyph> glyphsToAdd)
        {
            InitEmotionRenderer();
            _sdfReferenceAtlases ??= new Dictionary<Font, DrawableFontAtlas>();

            // Create the reference atlas - this is the atlas that is at the ouptut resolution of the SDF texture.
            _sdfReferenceAtlases.TryGetValue(atlas.Font, out DrawableFontAtlas refAtlas);
            if (refAtlas == null)
            {
                refAtlas = PollSDFCache(atlas);
                refAtlas ??= new DrawableFontAtlas(atlas.Font, SDF_REFERENCE_FONT_SIZE);
                _sdfReferenceAtlases.Add(atlas.Font, refAtlas);
            }

            var glyphPaddingBase = new Vector2(1);
            var refGlyphsToRender = new List<AtlasGlyph>();
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                AtlasGlyph reqGlyph = glyphsToAdd[i];
                var found = false;

                // Check if the reference atlas already has this glyph rendererd.
                for (var j = 0; j < refAtlas.DrawableAtlasGlyphs.Count; j++)
                {
                    AtlasGlyph refGlyph = refAtlas.DrawableAtlasGlyphs[j];
                    if (refGlyph.FontGlyph == reqGlyph.FontGlyph)
                    {
                        // Already rendered.
                        reqGlyph.UVLocation = refGlyph.UVLocation - glyphPaddingBase;
                        reqGlyph.UVSize = refGlyph.UVSize + glyphPaddingBase * 2;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var refGlyph = AtlasGlyph.CreateFloatScale(reqGlyph.FontGlyph, refAtlas.RenderScale, refAtlas.Font.Ascender);
                    refGlyphsToRender.Add(refGlyph);
                    refAtlas.DrawableAtlasGlyphs.Add(refGlyph);
                    refAtlas.Glyphs.Add((char)refAtlas.DrawableAtlasGlyphs.Count, refGlyph); // The char index doesn't matter, we need to add them here for atlas resizing reasons.
                }
            }

            if (atlas.GlyphDrawPadding == Vector2.Zero)
            {
                float scaleDiff = refAtlas.RenderScale / atlas.RenderScale;
                atlas.GlyphDrawPadding = glyphPaddingBase / scaleDiff;
            }

            atlas.FontShader = _sdfShader.Shader;

            // Check if anything to render.
            if (refGlyphsToRender.Count == 0) return refAtlas.GlyphRendererState;

            bool justCreated = refAtlas.GlyphRendererState == null;
            GlyphRendererState state = CommonGlyphRenderer.PrepareGlyphRenderer(refAtlas, refAtlas.GlyphRendererState, refGlyphsToRender, out Rectangle[] _, out Vector2 _, SdfAtlasGlyphSpacing.X + 2);
            refAtlas.GlyphRendererState = state;
            if (justCreated) state.AtlasBuffer.Texture.Smooth = true;

            // Create high resolution glyphs.
            var sdfFullResGlyphs = new List<AtlasGlyph>(refGlyphsToRender.Count);
            var sdfTempRects = new Rectangle[refGlyphsToRender.Count];
            Vector2 sdfGlyphSpacing = SdfAtlasGlyphSpacing * (1f / refAtlas.RenderScale);
            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                AtlasGlyph refGlyph = refGlyphsToRender[i];
                var sdfGlyph = AtlasGlyph.CreateFloatScale(refGlyph.FontGlyph, SDF_HIGH_RES_SCALE, atlas.Font.Ascender);
                sdfFullResGlyphs.Add(sdfGlyph);
                sdfTempRects[i] = new Rectangle(0, 0, sdfGlyph.Size + sdfGlyphSpacing * 2);
            }

            Vector2 sdfBaseAtlas = Binning.FitRectangles(sdfTempRects);

            // Remove spacing.
            for (var i = 0; i < sdfTempRects.Length; i++)
            {
                Rectangle r = sdfTempRects[i];
                sdfTempRects[i] = new Rectangle(r.Position + sdfGlyphSpacing, r.Size - sdfGlyphSpacing * 2);
            }

            // Generate ping-pong buffers.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(sdfBaseAtlas).WithColor();
            else
                _intermediateBuffer.Resize(sdfBaseAtlas, true);
            _intermediateBuffer.ColorAttachment.Smooth = false;

            if (_intermediateBufferTwo == null)
                _intermediateBufferTwo = new FrameBuffer(sdfBaseAtlas).WithColor();
            else
                _intermediateBufferTwo.Resize(sdfBaseAtlas, true);
            _intermediateBufferTwo.ColorAttachment.Smooth = false;

            //RenderDocGraphicsContext.RenderDocCaptureStart();
            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Render high res glyphs.
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader.Shader);
            uint clr = new Color(1, 0, 0, 0).ToUint();
            Vector3 offset = Vector3.Zero;
            RenderGlyphs(composer, sdfFullResGlyphs, offset, clr, SDF_HIGH_RES_SCALE, sdfTempRects);
            composer.RenderTargetPop();

            // Unwind them.
            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(_intermediateBufferTwo);
            composer.SetShader(_windingShader.Shader);
            _windingShader.Shader.SetUniformVector2("drawSize", _intermediateBufferTwo.AllocatedSize);
            composer.RenderFrameBuffer(_intermediateBuffer);
            composer.SetShader();
            composer.RenderTargetPop();

            // Generate SDF
            _intermediateBuffer.ColorAttachment.Smooth = true;
            _intermediateBufferTwo.ColorAttachment.Smooth = true;
            composer.SetState(RenderState.Default);
            composer.SetUseViewMatrix(false);
            composer.SetShader(_sdfGeneratingShader.Shader);

            // Generate SDF while downsizing individual glyphs.
            // If we do it in bulk the GPU might hang long enough for the OS to kill us.
            composer.RenderTo(state.AtlasBuffer);
            if (justCreated) composer.ClearFrameBuffer();
            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                AtlasGlyph refGlyph = refGlyphsToRender[i];
                Rectangle placeWithinSdfAtlas = sdfTempRects[i];

                // Copy sdf spacing as there might be data there.
                placeWithinSdfAtlas.Position -= sdfGlyphSpacing;
                placeWithinSdfAtlas.Size += sdfGlyphSpacing * 2;
                placeWithinSdfAtlas.Position = placeWithinSdfAtlas.Position.Floor();
                placeWithinSdfAtlas.Size = placeWithinSdfAtlas.Size.Floor();

                composer.RenderSprite(
                    new Vector3(refGlyph.UVLocation.X - SdfAtlasGlyphSpacing.X, refGlyph.UVLocation.Y - SdfAtlasGlyphSpacing.Y, 0),
                    refGlyph.UVSize + SdfAtlasGlyphSpacing * 2,
                    _intermediateBufferTwo.ColorAttachment,
                    placeWithinSdfAtlas);
            }

            composer.SetShader();
            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(prevState);
            //RenderDocGraphicsContext.RenderDocCaptureEnd();

            // Apply all UVs from the ref atlas to the req atlas.
            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                AtlasGlyph refGlyph = refGlyphsToRender[i];
                for (var j = 0; j < atlas.DrawableAtlasGlyphs.Count; j++)
                {
                    AtlasGlyph drawableGlyph = atlas.DrawableAtlasGlyphs[j];
                    if (drawableGlyph.FontGlyph != refGlyph.FontGlyph) continue;
                    drawableGlyph.UVLocation = refGlyph.UVLocation - glyphPaddingBase; // Apply UV padding to fit the GlyphRenderPadding.
                    drawableGlyph.UVSize = refGlyph.UVSize + glyphPaddingBase * 2;
                }
            }

            SaveToCache(refAtlas);
            return state;
        }

        public class SDFCacheGlyph
        {
            public Vector2 UVLocation;
            public Vector2 UVSize;
            public uint FontMapIndex;
        }

        public class SDFCacheAtlas
        {
            public List<SDFCacheGlyph> Glyphs = new List<SDFCacheGlyph>();
            public float RenderScale;
            public Binning.BinningResumableState BinningState;
        }

        private static Dictionary<Font, DrawableFontAtlas> _sdfReferenceAtlases;

        private static DrawableFontAtlas PollSDFCache(DrawableFontAtlas reqAtlas)
        {
            var cachedName = $"Player/SDFCache/{reqAtlas.Font.FullName}-{reqAtlas.RenderedWith}";
            var cachedRenderName = $"{cachedName}.png";
            var cachedMetaName = $"{cachedName}.xml";
            bool cachedImageExists = Engine.AssetLoader.Exists(cachedRenderName);
            bool cachedMetaExists = Engine.AssetLoader.Exists(cachedMetaName);

            if (!cachedImageExists || !cachedMetaExists) return null;

            var texture = Engine.AssetLoader.Get<TextureAsset>(cachedRenderName);
            var atlasMeta = Engine.AssetLoader.Get<XMLAsset<SDFCacheAtlas>>(cachedMetaName);

            if (texture == null || atlasMeta == null || atlasMeta.Content == null) return null;

            // Restore glyph table with its UV locations.
            var refAtlas = new DrawableFontAtlas(reqAtlas.Font, SDF_REFERENCE_FONT_SIZE);
            for (var i = 0; i < atlasMeta.Content.Glyphs.Count; i++)
            {
                SDFCacheGlyph g = atlasMeta.Content.Glyphs[i];
                uint mapIndex = g.FontMapIndex;
                Glyph fontGlyph = null;
                foreach (KeyValuePair<char, Glyph> fontGlyphData in reqAtlas.Font.Glyphs)
                {
                    if (fontGlyphData.Value.MapIndex != mapIndex) continue;
                    fontGlyph = fontGlyphData.Value;
                    break;
                }

                if (fontGlyph == null) return null;

                var refGlyph = AtlasGlyph.CreateFloatScale(fontGlyph, refAtlas.RenderScale, reqAtlas.Font.Ascender);
                refGlyph.UVLocation = g.UVLocation;
                refGlyph.UVSize = g.UVSize;
                refAtlas.DrawableAtlasGlyphs.Add(refGlyph);
                refAtlas.Glyphs.Add((char)refAtlas.DrawableAtlasGlyphs.Count, refGlyph);
            }

            Binning.BinningResumableState binningState = atlasMeta.Content.BinningState;
            FrameBuffer atlasBuffer = new FrameBuffer(binningState.Size).WithColor();
            atlasBuffer.ColorAttachment.Smooth = true;

            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity);
            composer.SetState(composer.BlitState);
            composer.RenderTo(atlasBuffer);

            // Draw it upside down as the atlas buffer is upside down.
            composer.RenderSprite(Vector3.Zero, texture.Texture.Size, Color.White, texture.Texture, null, false, true);

            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(prevState);

            var restoredState = new GlyphRendererState
            {
                AtlasBuffer = atlasBuffer,
                BinningState = binningState
            };
            refAtlas.GlyphRendererState = restoredState;

            // Free assets.
            Engine.AssetLoader.Destroy(cachedRenderName);
            Engine.AssetLoader.Destroy(cachedMetaName);

            return refAtlas;
        }

        public static void SaveToCache(DrawableFontAtlas refAtlas)
        {
            var cachedName = $"Player/SDFCache/{refAtlas.Font.FullName}-{refAtlas.RenderedWith}";
            var cachedRenderName = $"{cachedName}.png";
            var cachedMetaName = $"{cachedName}.xml";

            // Generate serializable meta objects.
            var serializeAtlas = new SDFCacheAtlas();
            serializeAtlas.BinningState = refAtlas.GlyphRendererState.BinningState;
            for (var i = 0; i < refAtlas.DrawableAtlasGlyphs.Count; i++)
            {
                AtlasGlyph refGlyph = refAtlas.DrawableAtlasGlyphs[i];
                var serializeGlyph = new SDFCacheGlyph();
                serializeGlyph.UVLocation = refGlyph.UVLocation;
                serializeGlyph.UVSize = refGlyph.UVSize;
                serializeGlyph.FontMapIndex = refGlyph.FontGlyph.MapIndex;
                serializeAtlas.Glyphs.Add(serializeGlyph);
            }

            XMLAsset<SDFCacheAtlas>.CreateFromContent(serializeAtlas).SaveAs(cachedMetaName, false);

            // Sample framebuffer and save it.
            FrameBuffer atlasBuffer = refAtlas.GlyphRendererState.AtlasBuffer;
            byte[] data = atlasBuffer.Sample(new Rectangle(0, 0, atlasBuffer.Size), PixelFormat.Rgba);
            byte[] pngData = PngFormat.Encode(data, atlasBuffer.Size, PixelFormat.Rgba);
            Engine.AssetLoader.Save(pngData, cachedRenderName, false);
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
                Vector3 atlasRenderPos = (dst.Position - new Vector2(atlasGlyph.XMin, -atlasGlyph.Height)).ToVec3();
                atlasRenderPos += offset;
                composer.PushModelMatrix(Matrix4x4.CreateScale(scale, -scale, 1) * Matrix4x4.CreateTranslation(atlasRenderPos));

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
    }
}