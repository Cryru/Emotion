#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Primitives;
using Emotion.Standard.OpenType;

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
        /// The size of the atlas texture, in pixels.
        /// </summary>
        public Vector2 AtlasSize { get; protected set; }

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
        /// The first char of the atlas.
        /// </summary>
        public uint FirstChar { get; protected set; }

        /// <summary>
        /// The number of characters rasterized in the atlas.
        /// </summary>
        public int NumChars { get; protected set; }

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

        /// <summary>
        /// Upload a font atlas texture to the gpu. Also holds a reference to the atlas itself
        /// for its metadata.
        /// </summary>
        /// <param name="font">The font to generate an atlas from.</param>
        /// <param name="fontSize">The size to scale glyphs to. This is relative to the font height.</param>
        /// <param name="firstChar">The codepoint of the first character to include in the atlas.</param>
        /// <param name="numChars">The number of characters to include in the atlas, after the first character.</param>
        /// <param name="smooth">Whether to smoothen the atlas texture.</param>
        public DrawableFontAtlas(Font font, float fontSize, uint firstChar = 0, int numChars = -1, bool smooth = true)
        {
            Font = font;
            FontSize = fontSize;
            _smooth = smooth;

            if (numChars == -1)
                NumChars = (int)(Font.LastCharIndex - firstChar);
            else
                NumChars = numChars;

            if (firstChar < Font.FirstCharIndex)
                FirstChar = Font.FirstCharIndex;
            else
                FirstChar = firstChar;

            // Set temporary texture so the atlas doesn't crash anything.
            Texture = Texture.EmptyWhiteTexture;

            Init();
        }

        private void Init()
        {
            if (Font.Glyphs == null || Font.Glyphs.Length == 0) return;

            var lastIdx = (int)(FirstChar + NumChars);

            // Convert from Emotion font size (legacy) to real font size.
            if (Engine.Configuration.UseEmotionFontSize)
            {
                float diff = (float) Font.UnitsPerEm / Font.Height;
                FontSize = MathF.Round(FontSize * diff);
            }

            // The scale to render at.
            float scale = FontSize / Font.UnitsPerEm;
            FontHeight = MathF.Ceiling(Font.Height * scale);
            RenderScale = scale;
            RenderedWith = Rasterizer;

            for (var i = 0; i < Font.Glyphs.Length; i++)
            {
                Glyph g = Font.Glyphs[i];
                bool inIndex = g.CharIndex.Any(charIdx => charIdx >= FirstChar && charIdx < lastIdx);
                if (!inIndex) continue;

                AtlasGlyph atlasGlyph;
                if (RenderedWith == GlyphRasterizer.EmotionSDF_01)
                    atlasGlyph = AtlasGlyph.CreateFloatScale(g, scale, Font.Ascender);
                else
                    atlasGlyph = AtlasGlyph.CreateIntScale(g, scale, Font.Ascender);

                // Associate atlas glyph with all representing chars.
                List<char> representingChars = atlasGlyph.FontGlyph.CharIndex;
                for (var j = 0; j < representingChars.Count; j++)
                {
                    Glyphs[representingChars[j]] = atlasGlyph;
                }

                if (atlasGlyph.Size.X > 0 && atlasGlyph.Size.Y > 0 && g.Vertices != null && g.Vertices.Length > 0)
                    DrawableAtlasGlyphs.Add(atlasGlyph);
            }

            // Fit glyphs into an atlas.
            int glyphSpacing = RenderedWith == GlyphRasterizer.EmotionSDF_01 ? 20 : 1;
            var glyphSpacing2 = new Vector2(glyphSpacing);
            var glyphRects = new Rectangle[DrawableAtlasGlyphs.Count];
            for (var i = 0; i < DrawableAtlasGlyphs.Count; i++)
            {
                AtlasGlyph atlasGlyph = DrawableAtlasGlyphs[i];
                // Inflate binning rect for spacing on all sides.
                var r = new Rectangle(0, 0, atlasGlyph.Size.X + glyphSpacing * 2, atlasGlyph.Size.Y + glyphSpacing * 2);
                glyphRects[i] = r;
            }

            AtlasSize = Binning.FitRectangles(glyphRects);

            // Set atlas UV data based on binning result.
            for (var i = 0; i < DrawableAtlasGlyphs.Count; i++)
            {
                AtlasGlyph atlasGlyph = DrawableAtlasGlyphs[i];
                Rectangle atlasRect = glyphRects[i];
                atlasRect.Position += glyphSpacing2;
                atlasRect.Size -= glyphSpacing2;
                atlasGlyph.UVLocation = atlasRect.Position;
                atlasGlyph.UVSize = atlasGlyph.Size;
            }
        }

        public void RenderAtlas()
        {
            switch (RenderedWith)
            {
                case GlyphRasterizer.Emotion:
                    EmotionGlyphRenderer.InitEmotionRenderer();
                    GLThread.ExecuteGLThreadAsync(() => EmotionGlyphRenderer.RenderAtlas(this));
                    break;
                case GlyphRasterizer.EmotionSDF_01:
                    EmotionGlyphRenderer.InitEmotionRenderer();
                    GLThread.ExecuteGLThreadAsync(() => EmotionGlyphRenderer.RenderAtlasSDF(this));
                    break;
                case GlyphRasterizer.StbTrueType:
                    GLThread.ExecuteGLThreadAsync(() => StbGlyphRenderer.RenderAtlas(this));
                    break;
            }
        }

        public void SetTexture(Texture texture)
        {
            Texture = texture;
            texture.Smooth = _smooth;
        }

        /// <summary>
        /// Clear resources.
        /// </summary>
        public void Dispose()
        {
            Texture.Dispose();
        }

        /// <summary>
        /// The default rasterizer.
        /// </summary>
#if WEB
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.StbTrueType;
#else
        public static GlyphRasterizer DefaultRasterizer { get; } = GlyphRasterizer.EmotionSDF_01;
#endif

        /// <summary>
        /// The rasterizer to use for generating atlases.
        /// </summary>
        public static GlyphRasterizer Rasterizer = DefaultRasterizer;
    }
}