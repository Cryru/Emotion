#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.Image.PNG;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// Represents a single glyph within a FontAtlas.
    /// </summary>
    public class AtlasGlyph
    {
        /// <summary>
        /// The character index of the glyph.
        /// </summary>
        public char CharIndex { get; set; }

        /// <summary>
        /// The location of the glyph within the atlas texture.
        /// </summary>
        public Vector2 Location { get; set; }

        /// <summary>
        /// The size of the glyph.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The size of the glyph texture within the atlas texture.
        /// </summary>
        public Vector2 UV { get; set; }

        /// <summary>
        /// The glyph's scaled advance.
        /// </summary>
        public float Advance { get; set; }

        /// <summary>
        /// The glyph's scaled XMin.
        /// </summary>
        public float XMin { get; set; }

        /// <summary>
        /// The size below the origin.
        /// </summary>
        public float YMin { get; set; }

        /// <summary>
        /// The glyph's scaled YBearing, which is the font's scaled ascend minus the glyph's scaled YMax.
        /// </summary>
        public float YBearing { get; set; }

        public AtlasGlyph(char charIndex, float advance, float xMin, float yBearing)
        {
            CharIndex = charIndex;

            Advance = advance;
            XMin = xMin;
            YBearing = yBearing;
        }

        public AtlasGlyph(Glyph fontGlyph, float scale, float ascend)
        {
            CharIndex = (char) fontGlyph.CharIndex;
            Advance = MathF.Round(fontGlyph.AdvanceWidth * scale);
            XMin = MathF.Floor(fontGlyph.XMin * scale);
            Rectangle bbox = fontGlyph.GetBBox(scale);
            YBearing = bbox.Y + MathF.Ceiling(ascend * scale);
            YMin = MathF.Floor(fontGlyph.YMin * scale);
            Size = fontGlyph.GetDrawBox(scale).Size;
        }
    }

    /// <summary>
    /// Represents a rasterized texture of glyphs.
    /// </summary>
    public class FontAtlas
    {
        /// <summary>
        /// The scale of the glyphs relative to their size in the font.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// The size of the texture.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The pixel data of the texture.
        /// </summary>
        public byte[] Pixels { get; set; }

        /// <summary>
        /// A list of all glyphs, indexed by the character they represent.
        /// </summary>
        public Dictionary<char, AtlasGlyph> Glyphs { get; } = new Dictionary<char, AtlasGlyph>();

        /// <summary>
        /// The name of the rasterizer who rasterized this atlas.
        /// Is expected to be one of the Font.GlyphRasterizer enum, but doesn't have to be.
        /// </summary>
        public string RasterizedBy { get; set; }

        /// <summary>
        /// The font's height scaled.
        /// Is used as the distance between lines and is regarded as the safe space.
        /// </summary>
        public float FontHeight { get; set; }

        public FontAtlas(Vector2 size, byte[] pixels, string rasterizedBy, float scale, Font f)
        {
            Size = size;
            Pixels = pixels;
            RasterizedBy = rasterizedBy;
            Scale = scale;
            FontHeight = MathF.Ceiling(f.Height * scale);
        }

        public void DebugDump(string fileName)
        {
            byte[] bytes = ImageUtil.AToRgba(Pixels);
            bytes = PngFormat.Encode(bytes, (int) Size.X, (int) Size.Y);
            File.WriteAllBytes(fileName, bytes);
        }
    }
}