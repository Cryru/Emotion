#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.IO;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Game.Effects
{
    /// <summary>
    /// Used to load the texture and get a palette map.
    /// </summary>
    public class PaletteBaseTexture : TextureAsset
    {
        public byte[] PaletteMap;

        protected override void UploadTexture(Vector2 size, byte[] bgraPixels, bool flipped, PixelFormat format)
        {
            PaletteMap = GeneratePaletteMap(bgraPixels, format, out _);
            base.UploadTexture(size, bgraPixels, flipped, format);
        }

        /// <summary>
        /// Generate a palette from a texture.
        /// </summary>
        /// <param name="pixels">The pixels to generate from.</param>
        /// <param name="format">The pixel format.</param>
        /// <param name="colors">The colors in the map.</param>
        /// <returns>An index based palette map.</returns>
        public static byte[] GeneratePaletteMap(byte[] pixels, PixelFormat format, out List<Color> colors)
        {
            var paletteMap = new byte[pixels.Length / 4];
            colors = new List<Color>();
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var c = new Color(
                    pixels[i],
                    pixels[i + 1],
                    pixels[i + 2],
                    pixels[i + 3],
                    format
                );

                int index = colors.IndexOf(c);
                if (index == -1)
                {
                    index = colors.Count;
                    colors.Add(c);
                }

                paletteMap[i / 4] = (byte)index;
            }

            return paletteMap;
        }
    }
}