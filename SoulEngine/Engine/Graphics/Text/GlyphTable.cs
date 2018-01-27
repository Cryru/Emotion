// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;

#endregion

namespace Soul.Engine.Graphics.Text
{
    /// <summary>
    /// A table of loaded glyphs.
    /// </summary>
    public class GlyphTable
    {
        #region Public Data

        /// <summary>
        /// The name of the font whose glyphs are stored.
        /// </summary>
        public string FontName { get; private set; }

        #endregion

        #region Data

        /// <summary>
        /// A dictionary of loaded glyphs.
        /// </summary>
        private Dictionary<string, Glyph> _glyphStorage;

        #endregion

        /// <summary>
        /// Create a new glyph table to hold glyphs.
        /// </summary>
        public GlyphTable(string fontName)
        {
            FontName = fontName;
            _glyphStorage = new Dictionary<string, Glyph>();
        }

        #region Functions

        /// <summary>
        /// Add a glyph to the table.
        /// </summary>
        /// <param name="character">The character the glyph represents</param>
        /// <param name="size">The size of the character.</param>
        /// <param name="glyph">The glyph itself.</param>
        public void AddGlyph(uint character, uint size, Glyph glyph)
        {
            // Add the glyph.
            _glyphStorage.Add(character + "@" + size, glyph);
        }

        /// <summary>
        /// Returns whether a character of the requested size is cached.
        /// </summary>
        /// <param name="character">The character to lookup.</param>
        /// <param name="size">The size to lookup.</param>
        public bool HasGlyph(uint character, uint size)
        {
            return _glyphStorage.ContainsKey(character + "@" + size);
        }


        /// <summary>
        /// Returns a cached glyph or an exception if the glyph isn't cached.
        /// </summary>
        /// <param name="character">The character to check.</param>
        /// <param name="size">The requested size of the character.</param>
        /// <returns></returns>
        public Glyph GetGlyph(uint character, uint size)
        {
            return _glyphStorage[character + "@" + size];
        }

        #endregion

        /// <summary>
        /// Destroy the glyph table.
        /// </summary>
        public void Dispose()
        {
            // Clear all glyphs.
            foreach (KeyValuePair<string, Glyph> g in _glyphStorage)
            {
                g.Value.Dispose();
            }

            _glyphStorage.Clear();
            _glyphStorage = null;
        }
    }
}