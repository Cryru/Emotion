#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Game.Effects
{
    public class PaletteAsset : XMLAsset<PaletteDescription>
    {
        public PaletteBaseTexture BaseTexture { get; set; }
        public Dictionary<Palette, Texture> PaletteSwaps = new Dictionary<Palette, Texture>();

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            base.CreateInternal(data);
            if (Content == null) return;

            // Load the base texture.
            BaseTexture = Engine.AssetLoader.Get<PaletteBaseTexture>(Content.BaseAsset);

            Debug.Assert(BaseTexture != null);
            Debug.Assert(BaseTexture.PaletteMap != null);

            // Load all variations.
            var pixels = new byte[BaseTexture.PaletteMap.Length * 4];
            Span<Color> pixelsAsColor = MemoryMarshal.Cast<byte, Color>(pixels);
            foreach (Palette p in Content.Palettes)
            {
                for (var j = 0; j < pixelsAsColor.Length; j++)
                {
                    byte index = BaseTexture.PaletteMap[j];
                    if (index >= p.Colors.Length) index = 0;

                    Color c = p.Colors[index];
                    pixelsAsColor[j] = c;
                }

                GLThread.ExecuteGLThread(() =>
                {
                    var texture = new Texture(BaseTexture.Texture.Size, PixelFormat.Rgba);
                    texture.Upload(BaseTexture.Texture.Size, pixels);
                    texture.FlipY = BaseTexture.Texture.FlipY;
                    PaletteSwaps.Add(p, texture);
                });
            }

            // Free up the palette map memory.
            BaseTexture.PaletteMap = null;
        }

        /// <summary>
        /// Get the palette texture under a specific name.
        /// </summary>
        /// <param name="paletteName">The name of the palette texture to get.</param>
        /// <returns>A horizontal texture containing the specified palette.</returns>
        public Texture GetTextureForPalette(string paletteName)
        {
            if (string.Equals(paletteName, "default", StringComparison.OrdinalIgnoreCase)) return BaseTexture.Texture;

            Palette p = Content.GetPalette(paletteName);
            if (p == null) return null;
            return PaletteSwaps.ContainsKey(p) ? PaletteSwaps[p] : null;
        }

        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            GLThread.ExecuteGLThreadAsync(() =>
            {
                BaseTexture.Dispose();
                foreach (KeyValuePair<Palette, Texture> p in PaletteSwaps)
                {
                    p.Value.Dispose();
                }
            });
        }
    }
}