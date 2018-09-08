// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.IO;

#endregion

namespace Emotion.Graphics.Text
{
    public sealed class Font : Asset
    {
        private byte[] _fontBytes;
        private Dictionary<uint, Atlas> _atlasCache;

        #region Asset API

        internal override void Create(byte[] data)
        {
            _fontBytes = data;
            _atlasCache = new Dictionary<uint, Atlas>();
        }

        internal override void Destroy()
        {
            // Destroy loaded atlases.
            foreach (KeyValuePair<uint, Atlas> atlas in _atlasCache)
            {
                atlas.Value.Destroy();
            }

            _atlasCache.Clear();
        }

        #endregion

        #region Font API

        /// <summary>
        /// Returns a font atlas of the specified size and which the specified glyphs loaded. Loaded atlases won't be reloaded.
        /// </summary>
        /// <param name="fontSize">The font size of the atlas to return.</param>
        /// <param name="glyphs">The number of glyphs to load. By default loads letters and grammar - the first 128.</param>
        /// <returns>A font atlas.</returns>
        public Atlas GetFontAtlas(uint fontSize, int glyphs = 128)
        {
            // Cache the atlas if it isn't cached.
            if (!_atlasCache.ContainsKey(fontSize)) _atlasCache[fontSize] = new Atlas(_fontBytes, fontSize, glyphs);

            return _atlasCache[fontSize];
        }

        #endregion
    }
}