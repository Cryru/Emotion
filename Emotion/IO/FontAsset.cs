#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Graphics.Text;
using Emotion.Standard.OpenType;
using Emotion.Utility;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A font file and cached atlas textures.
    /// </summary>
    public class FontAsset : Asset
    {
        /// <summary>
        /// The Emotion.Standard.OpenType font generated from the font file.
        /// </summary>
        public Font Font { get; protected set; }

        /// <summary>
        /// List of loaded atlases. Some of these are cached and loaded by the AssetLoader, some are loaded by the FontAsset.
        /// </summary>
        private Dictionary<string, DrawableFontAtlas> _loadedAtlases = new Dictionary<string, DrawableFontAtlas>();

        /// <inheritdoc />
        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            Font = new Font(data);
        }

        /// <inheritdoc />
        protected override void DisposeInternal()
        {
            foreach (KeyValuePair<string, DrawableFontAtlas> atlas in _loadedAtlases)
            {
                atlas.Value.Dispose();
            }

            _loadedAtlases.Clear();
        }

        /// <summary>
        /// Get a font atlas that can be used for text drawing.
        /// Atlases are cached, so requesting the same one twice will return the same reference.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="firstChar">The codepoint of the first character to include in the atlas.</param>
        /// <param name="numChars">The number of characters to include in the atlas, after the first character.</param>
        /// <param name="smooth">Whether to apply bilinear filtering to the texture. You most likely want this.</param>
        /// <param name="pixelFont">
        /// If the font should be rendered pixel perfect. If set to true the font size passed is
        /// overwritten with the closest one.
        /// </param>
        /// <returns></returns>
        public DrawableFontAtlas GetAtlas(int fontSize, uint firstChar = 0, int numChars = 127, bool smooth = true, bool pixelFont = false)
        {
            var hash = $"{fontSize}-{firstChar}-{numChars}-{DrawableFontAtlas.Rasterizer}";

            // Check if the atlas is already loaded.
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) return atlas;

            lock (_loadedAtlases)
            {
                // Recheck as another thread could have built the atlas while waiting on lock.
                found = _loadedAtlases.TryGetValue(hash, out atlas);
                if (found) return atlas;

                // Scale to closest power of two.
                float sizeFloat = fontSize;
                if (pixelFont)
                {
                    float fontHeight = Font.Height;
                    float scaleFactor = fontHeight / fontSize;
                    int scaleFactorP2 = Maths.ClosestPowerOfTwoGreaterThan((int)MathF.Floor(scaleFactor));
                    sizeFloat = fontHeight / scaleFactorP2;
                }

                // Load a new atlas.
                PerfProfiler.ProfilerEventStart($"FontAtlas {Name} {fontSize} {hash}", "Loading");
                atlas = new DrawableFontAtlas(Font, sizeFloat, firstChar, numChars, smooth);
                atlas.RenderAtlas();
                PerfProfiler.ProfilerEventEnd($"FontAtlas {Name} {fontSize} {hash}", "Loading");

                _loadedAtlases.Add(hash, atlas);
            }

            return atlas;
        }

        /// <inheritdoc cref="GetAtlas(int, uint, int, bool, bool)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DrawableFontAtlas GetAtlas(float fontSize, uint firstChar = 0, int numChars = -1, bool smooth = true)
        {
            var intFontSize = (int)MathF.Ceiling(fontSize); // Ceil so we dont store atlases for every floating deviation.
            return GetAtlas(intFontSize, firstChar, numChars, smooth);
        }

        /// <summary>
        /// Free memory by destroying a cached atlas.
        /// </summary>
        public void DestroyAtlas(int fontSize, int firstChar = 0, int numChars = -1, GlyphRasterizer? rasterizer = null)
        {
            fontSize = (int)MathF.Ceiling(fontSize);
            var hash = $"{fontSize}-{firstChar}-{numChars}-{rasterizer ?? DrawableFontAtlas.DefaultRasterizer}";
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found)
            {
                atlas.Dispose();
                _loadedAtlases.Remove(hash);
            }
        }

        /// <summary>
        /// Free memory by destroying a cached atlas.
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="firstChar"></param>
        /// <param name="numChars"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyAtlas(float fontSize, int firstChar = 0, int numChars = -1)
        {
            DestroyAtlas((int)MathF.Ceiling(fontSize), firstChar, numChars);
        }
    }
}