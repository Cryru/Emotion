#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Standard.OpenType;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Text
{
    public enum GlyphRasterizer
    {
        /// <summary>
        /// A custom GPU-based rasterizer built for the engine. Still not mature enough to be the default.
        /// Slow and ugliest.
        /// </summary>
        Emotion,

        /// <summary>
        /// A custom GPU-based rasterizer that requires a custom shader when drawing glyphs.
        /// First a very high resolution atlas is rendered using the Emotion rasterizer and
        /// a signed distance field generating algorithm is ran over it. This is cached to a file (if an asset store is loaded).
        /// Then the atlas is used for all font sizes and read from using the SDF shader to provide crisp text at any size.
        /// Best looking, but very slow to render. Writes caches images to reduce subsequent loading but if you are using an atlas
        /// at only one font size, loading the image isn't very fast either.
        /// Can reuse atlas images for various font sizes, reducing memory usage overall.
        /// </summary>
        EmotionSDFVer3,

        /// <summary>
        /// Mature software rasterizer.
        /// Default.
        /// </summary>
        StbTrueType,
    }

    /// <summary>
    /// An uploaded to the GPU font atlas.
    /// </summary>
    public class DrawableFontAtlas : IDisposable
    {
        public Font Font;

        /// <summary>
        /// The font size the atlas was rasterized at.
        /// </summary>
        public float FontSize { get; protected set; }

        /// <summary>
        /// The rasterized used to produce this atlas.
        /// </summary>
        public GlyphRasterizer RenderedWith { get; protected set; }

        /// <summary>
        /// Font shader to use.
        /// </summary>
        public ShaderProgram FontShader { get; set; }

        /// <summary>
        /// The atlas' render scale, based on the font size.
        /// </summary>
        public float RenderScale { get; set; }

        /// <summary>
        /// Padding around the glyph render position to render with.
        /// The SDF atlas glyphs require this as the distance gets clipped by the glyph
        /// bounding box too fast otherwise.
        /// </summary>
        public Vector2 GlyphDrawPadding { get; set; }

        /// <summary>
        /// The atlas texture.
        /// </summary>
        public Texture Texture { get; protected set; }

        /// <summary>
        /// A list of all glyphs, indexed by the character they represent.
        /// </summary>
        public Dictionary<char, AtlasGlyph> Glyphs { get; } = new Dictionary<char, AtlasGlyph>();

        /// <summary>
        /// List of all unique drawable atlas glyphs.
        /// </summary>
        public List<AtlasGlyph> DrawableAtlasGlyphs { get; } = new List<AtlasGlyph>();

        /// <summary>
        /// The objects used by the glyph renderer to render and add glyphs to the atlas.
        /// </summary>
        public GlyphRendererState GlyphRendererState { get; set; }

        /// <summary>
        /// The font's height scaled.
        /// Is used as the distance between lines and is regarded as the safe space.
        /// </summary>
        public float FontHeight { get; set; }

        private bool _smooth;
        private bool _pixelFont;

        /// <summary>
        /// Upload a font atlas texture to the gpu. Also holds a reference to the atlas itself
        /// for its metadata.
        /// </summary>
        /// <param name="font">The font to generate an atlas from.</param>
        /// <param name="fontSize">The size to scale glyphs to. This is relative to the font height.</param>
        /// <param name="smooth">Whether to smoothen the atlas texture.</param>
        /// <param name="pixelFont">
        /// If the font should be rendered pixel perfect. If set to true the font size passed is
        /// overwritten with the closest pixel accurate one.
        /// </param>
        public DrawableFontAtlas(Font font, float fontSize, bool smooth = true, bool pixelFont = false)
        {
            Font = font;
            FontSize = fontSize;
            _smooth = smooth;
            _pixelFont = pixelFont;

            if (_pixelFont)
            {
                // Scale to closest power of two.
                float fontHeight = Font.Height;
                float scaleFactor = fontHeight / fontSize;
                int scaleFactorP2 = Maths.ClosestPowerOfTwoGreaterThan((int)MathF.Floor(scaleFactor));
                FontSize = fontHeight / scaleFactorP2;
            }

            // Set temporary texture so the atlas doesn't crash anything.
            Texture = Texture.EmptyWhiteTexture;

            // Convert from Emotion font size (legacy) to real font size.
            if (Engine.Configuration.UseEmotionFontSize)
            {
                float diff = (float)Font.UnitsPerEm / Font.Height;
                FontSize = MathF.Round(FontSize * diff);
            }

            // The scale to render at.
            float scale = FontSize / Font.UnitsPerEm;
            FontHeight = MathF.Ceiling(Font.Height * scale);
            RenderScale = scale;
            RenderedWith = _pixelFont ? GlyphRasterizer.StbTrueType : Rasterizer;
        }

        // Serialization constructor (for debugging)
        protected DrawableFontAtlas()
        {
        }

        /// <summary>
        /// Adds all missing characters from the provided string to the font atlas.
        /// This includes their metrics as well as rendering them (async).
        /// </summary>
        /// <param name="text">The text whose characters to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CacheGlyphs(string text)
        {
            List<AtlasGlyph> renderGlyphs = null;
            for (var i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (Glyphs.ContainsKey(ch) || !Font.Glyphs.TryGetValue(ch, out Glyph fontG)) continue;

                AtlasGlyph atlasGlyph;
                if (RenderedWith == GlyphRasterizer.EmotionSDFVer3)
                    atlasGlyph = AtlasGlyph.CreateFloatScale(fontG, RenderScale, Font.Ascender);
                else
                    atlasGlyph = AtlasGlyph.CreateIntScale(fontG, RenderScale, Font.Ascender);

                Glyphs.Add(ch, atlasGlyph);

                if (!(atlasGlyph.Size.X > 0) || !(atlasGlyph.Size.Y > 0) || fontG.Vertices == null || fontG.Vertices.Length <= 0) continue;
                DrawableAtlasGlyphs.Add(atlasGlyph);

                renderGlyphs ??= new List<AtlasGlyph>();
                renderGlyphs.Add(atlasGlyph);
            }

            if (renderGlyphs != null)
                GLThread.ExecuteGLThread(() => { QueueGlyphRender(renderGlyphs); });
        }

        private void QueueGlyphRender(List<AtlasGlyph> glyphs)
        {
            Debug.Assert(GLThread.IsGLThread());
            bool justCreated = GlyphRendererState == null;
            switch (RenderedWith)
            {
                case GlyphRasterizer.Emotion:
                    GlyphRendererState = EmotionGlyphRenderer.AddGlyphsToAtlas(this, GlyphRendererState, glyphs);
                    break;
                case GlyphRasterizer.EmotionSDFVer3:
                    GlyphRendererState = EmotionGlyphRenderer.AddGlyphsToAtlasSDF(this, GlyphRendererState, glyphs);
                    break;
                case GlyphRasterizer.StbTrueType:
                    GlyphRendererState = StbGlyphRenderer.AddGlyphsToAtlas(this, GlyphRendererState, glyphs);
                    break;
            }

            if (justCreated && GlyphRendererState != null)
            {
                Texture = GlyphRendererState.AtlasBuffer.ColorAttachment;
                Texture.Smooth = _smooth;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDrawing(RenderComposer c, string text)
        {
            // Ensure we have all glyphs needed for the text.
            CacheGlyphs(text);

            // Set shader.
            if (FontShader != null) c.SetShader(FontShader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FinishDrawing(RenderComposer c)
        {
            if (FontShader != null) c.SetShader();
        }

        /// <summary>
        /// Clear resources.
        /// </summary>
        public void Dispose()
        {
            if (GlyphRendererState == null) return;

            GlyphRendererState.AtlasBuffer.Dispose();
            GlyphRendererState = null;
            Texture = null;
            Font = null;
        }

        /// <summary>
        /// The default rasterizer.
        /// </summary>
#if WEB
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.StbTrueType;
#else
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.EmotionSDFVer3;
#endif

        /// <summary>
        /// The rasterizer to use for generating atlases.
        /// </summary>
        public static GlyphRasterizer Rasterizer = DefaultRasterizer;
    }
}