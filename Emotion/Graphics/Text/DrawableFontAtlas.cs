#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.OpenType;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Text
{
    public class DrawableFontAtlas : IDisposable
    {
        /// <summary>
        /// The OpenType font this atlas was generated from.
        /// </summary>
        public Font Font { get; init; }

        /// <summary>
        /// The texture atlas.
        /// </summary>
        public virtual Texture Texture { get => Texture.NoTexture; }

        /// <summary>
        /// The px size of the font.
        /// </summary>
        public float FontSize { get; init; }

        /// <summary>
        /// Whether this font is supposed to be drawn pixel perfect.
        /// </summary>
        public bool PixelFont { get; init; }

        /// <summary>
        /// The scale multiplier for glyph metrics for this font's size.
        /// </summary>
        public float RenderScale { get; init; }

        /// <summary>
        /// The space between new lines, from baseline to baseline. Used as a line gap.
        /// Is the combination of the ascent and descent.
        /// </summary>
        public float FontHeight { get; init; }

        /// <summary>
        /// The distance from the baseline to the highest glyph ascent. Is positive because Y is up in glyph metrics.
        /// </summary>
        public float Ascent { get; init; }

        /// <summary>
        /// The distance from the baseline to the lowest glyph descent.
        /// </summary>
        public float Descent { get; init; }

        public Dictionary<char, DrawableGlyph> Glyphs { get; init; } = new Dictionary<char, DrawableGlyph>();

        public DrawableFontAtlas(Font font, float fontSize, bool pixelFont = false)
        {
            Font = font;
            FontSize = fontSize;
            if (pixelFont)
            {
                // Scale to closest power of two.
                float fontHeight = Font.Height;
                float scaleFactor = fontHeight / fontSize;
                int scaleFactorP2 = Maths.ClosestPowerOfTwoGreaterThan((int) MathF.Floor(scaleFactor));
                FontSize = fontHeight / scaleFactorP2;
                PixelFont = true;
            }

            // Convert from Emotion font size (legacy) to real font size.
            if (Engine.Configuration.UseEmotionFontSize)
            {
                float diff = Font.UnitsPerEm / Font.Height;
                FontSize = MathF.Round(FontSize * diff);
            }

            // The scale to render at.
            float scale = FontSize / Font.UnitsPerEm;
            Ascent = font.Ascender * scale;
            Descent = font.Descender * scale;
            FontHeight = font.Height * scale;
            RenderScale = scale;
        }

        #region Internal API

        protected virtual void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
        }

        private void AddGlyphsToAtlasBenchmark(List<DrawableGlyph> glyphsToAdd)
        {
            var w = Stopwatch.StartNew();
            AddGlyphsToAtlas(glyphsToAdd);
            w.Stop();
            Engine.Log.Info($"Rendered {glyphsToAdd.Count} glyphs in {w.ElapsedMilliseconds}ms", GetType().Name);
        }

        #endregion

        public virtual void CacheGlyphs(string text)
        {
            List<DrawableGlyph> renderGlyphs = null;
            for (var i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (Glyphs.ContainsKey(ch) || !Font.CharToGlyph.TryGetValue(ch, out FontGlyph fontG)) continue;

                var atlasGlyph = new DrawableGlyph(ch, fontG, RenderScale);
                Glyphs.Add(ch, atlasGlyph);

                if (!atlasGlyph.CanBeShown()) continue;

                renderGlyphs ??= new List<DrawableGlyph>();
                renderGlyphs.Add(atlasGlyph);
            }

            if (renderGlyphs != null) GLThread.ExecuteOnGLThreadAsync(AddGlyphsToAtlas, renderGlyphs);
        }

        public virtual void SetupDrawing(RenderComposer c, string text, FontEffect effect = FontEffect.None, float effectAmount = 0f, Color? effectColor = null)
        {
            if (text == null) return;
            CacheGlyphs(text);
        }

        public virtual void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
        }

        public virtual void FinishDrawing(RenderComposer c)
        {
        }

        public virtual void Dispose()
        {
        }

        #region Packing

        protected Packing.PackingResumableState PackGlyphsInAtlas(List<DrawableGlyph> glyphs, Packing.PackingResumableState bin)
        {
            if (bin.Size != Vector2.Zero)
            {
                var refitAll = false;

                // Try to add new ones.
                for (var i = 0; i < glyphs.Count; i++)
                {
                    DrawableGlyph glyph = glyphs[i];
                    Vector2 glyphSize = BinGetGlyphDimensions(glyph);
                    Vector2? position = Packing.FitRectanglesResumable(glyphSize, bin);

                    // Couldn't find space, rebin all.
                    if (position == null)
                    {
                        // Go through all existing glyphs and request them to be rerendered as well.
                        foreach ((char _, DrawableGlyph gRef) in BinGetAllGlyphs())
                        {
                            // Already exists in request.
                            if (!gRef.CanBeShown() || glyphs.IndexOf(gRef) != -1) continue;
                            glyphs.Add(gRef);
                        }

                        refitAll = true;
                        break;
                    }

                    // If found position apply it to UV.
                    glyph.GlyphUV = new Rectangle(position.Value, glyphSize);
                }

                if (!refitAll) return bin;
            }

            // Create bin rectangles.
            var packingRects = new Rectangle[glyphs.Count];
            for (var i = 0; i < glyphs.Count; i++)
            {
                DrawableGlyph refGlyph = glyphs[i];
                packingRects[i] = new Rectangle(0, 0, BinGetGlyphDimensions(refGlyph));
            }

            bin = new Packing.PackingResumableState(Vector2.Zero);
            Packing.FitRectangles(packingRects, false, bin);

            // Assign binned uvs.
            for (var i = 0; i < glyphs.Count; i++)
            {
                DrawableGlyph gRef = glyphs[i];
                gRef.GlyphUV = packingRects[i];
            }

            return bin;
        }

        protected virtual Dictionary<char, DrawableGlyph> BinGetAllGlyphs()
        {
            return Glyphs;
        }

        protected virtual Vector2 BinGetGlyphDimensions(DrawableGlyph g)
        {
            // Set to constant size to better align to baseline.
            return new Vector2(g.Width, FontHeight);
        }

        #endregion
    }
}