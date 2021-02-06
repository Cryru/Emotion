#region Using

using System;
using System.Linq;

#endregion

namespace Emotion.Game.Effects
{
    public class PaletteDescription
    {
        public Palette[] Palettes = new Palette[0];
        public string BaseAsset { get; set; }

        /// <summary>
        /// Get the palette with the specified name.
        /// </summary>
        public Palette GetPalette(string name)
        {
            return Palettes.FirstOrDefault(x => string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}