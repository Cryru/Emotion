#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// an signed distance field generating algorithm is ran over it. This is cached to a file (if an asset store is loaded).
        /// Then the atlas is used for all font sizes and read from using the SDF shader to provide crisp text at any size.
        /// Best looking, but very slow to render. Writes caches images to reduce subsequent loading but loading 4k PNGs isn't very
        /// fast :P
        /// Can reuse atlas images for various font sizes, reducing memory usage overall.
        /// </summary>
        EmotionSDF_01,

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
        /// The font's height scaled.
        /// Is used as the distance between lines and is regarded as the safe space.
        /// </summary>
        public float FontHeight { get; set; }

        private bool _smooth;
        private bool _pixelFont;
        private GlyphRendererState _state;

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
            RenderedWith = Rasterizer;
        }

        // Serialization constructor (for debugging)
        protected DrawableFontAtlas()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CacheGlyphs(string text)
        {
            List<AtlasGlyph> renderGlyphs = null;
            for (var i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (Glyphs.ContainsKey(ch) || !Font.Glyphs.TryGetValue(ch, out Glyph fontG)) continue;

                AtlasGlyph atlasGlyph;
                if (RenderedWith == GlyphRasterizer.EmotionSDF_01)
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
            switch (RenderedWith)
            {
                case GlyphRasterizer.Emotion:
                case GlyphRasterizer.EmotionSDF_01:
                case GlyphRasterizer.StbTrueType:
                    _state = StbGlyphRenderer.AddGlyphsToAtlas(this, _state, glyphs);
                    break;
            }
        }

        public void SetTexture(Texture texture)
        {
            Texture = texture;
            texture.Smooth = _smooth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetupDrawing(RenderComposer c, string text)
        {
            // Ensure we have all glyphs needed for the text.
            CacheGlyphs(text);

            // Set shader.
            if (FontShader != null)
            {
                c.SetShader(FontShader);
                if (_pixelFont)
                    // No AA whatsoever.
                    FontShader.SetUniformFloat("scaleFactor", Font.UnitsPerEm);
                else
                    // HalfRange / OutputScale - GenerateSDF.frag
                    FontShader.SetUniformFloat("scaleFactor", 10f * RenderScale);
            }
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
            if (_state == null) return;

            _state.AtlasBuffer.Dispose();
            _state = null;
            Texture = null;
            Font = null;
        }

        /// <summary>
        /// The default rasterizer.
        /// </summary>
#if WEB
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.StbTrueType;
#else
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.StbTrueType;
#endif

        /// <summary>
        /// The rasterizer to use for generating atlases.
        /// </summary>
        public static GlyphRasterizer Rasterizer = DefaultRasterizer;
    }
}