#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Standard.Image.PNG;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Standard.OpenType
{
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

        /// <summary>
        /// A font atlas containing a rasterized font and meta data about the atlas.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="pixels"></param>
        /// <param name="rasterizedBy"></param>
        /// <param name="scale"></param>
        /// <param name="f"></param>
        public FontAtlas(Vector2 size, byte[] pixels, string rasterizedBy, float scale, Font f)
        {
            Size = size;
            Pixels = pixels;
            RasterizedBy = rasterizedBy;
            Scale = scale;
            FontHeight = MathF.Ceiling(f.Height * scale);
        }

        /// <summary>
        /// Dump the pixel data to a file in the "Player" AssetStore in the DebugDump folder.
        /// Used for debugging purposes.
        /// </summary>
        /// <param name="fileName">The file to save to.</param>
        [Conditional("DEBUG")]
        public void DebugDump(string fileName)
        {
            byte[] bytes = ImageUtil.AToRgba(Pixels);
            bytes = PngFormat.Encode(bytes, Size, PixelFormat.Rgba);
            Engine.AssetLoader.Save(bytes, $"Player/DebugDump/{fileName}");
        }
    }
}