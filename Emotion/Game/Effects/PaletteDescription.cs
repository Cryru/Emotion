#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

#endregion

namespace Emotion.Game.Effects
{
    public class PaletteDescription
    {
        public Palette[] Palettes { get; set; }
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