#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Graphics.Text;
using Emotion.Standard.OpenType;

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
        /// <param name="smooth">Whether to apply bilinear filtering to the texture. You most likely want this.</param>
        /// <param name="pixelFont">
        /// If the font should be rendered pixel perfect. If set to true the font size passed is
        /// overwritten with the closest one.
        /// </param>
        /// <returns></returns>
        public DrawableFontAtlas GetAtlas(int fontSize, bool smooth = true, bool pixelFont = false)
        {
            var hash = $"{fontSize}-{DrawableFontAtlas.Rasterizer}";

            // Check if the atlas is already loaded.
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) return atlas;

            lock (_loadedAtlases)
            {
                // Recheck as another thread could have built the atlas while waiting on lock.
                found = _loadedAtlases.TryGetValue(hash, out atlas);
                if (found) return atlas;

                // Load a new atlas.
                PerfProfiler.ProfilerEventStart($"FontAtlas {Name} {fontSize} {hash}", "Loading");
                atlas = new DrawableFontAtlas(Font, fontSize, smooth, pixelFont);
                atlas.CacheGlyphs(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
                PerfProfiler.ProfilerEventEnd($"FontAtlas {Name} {fontSize} {hash}", "Loading");

                _loadedAtlases.Add(hash, atlas);
            }

            return atlas;
        }

        /// <inheritdoc cref="GetAtlas(int, bool, bool)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DrawableFontAtlas GetAtlas(float fontSize, bool smooth = true, bool pixelFont = false)
        {
            var intFontSize = (int)MathF.Ceiling(fontSize); // Ceil so we dont store atlases for every floating deviation.
            return GetAtlas(intFontSize, smooth, pixelFont);
        }

        /// <summary>
        /// Free memory by destroying a cached atlas.
        /// </summary>
        public void DestroyAtlas(int fontSize, GlyphRasterizer? rasterizer = null)
        {
            fontSize = (int)MathF.Ceiling(fontSize);
            var hash = $"{fontSize}-{rasterizer ?? DrawableFontAtlas.DefaultRasterizer}";
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyAtlas(float fontSize)
        {
            DestroyAtlas((int)MathF.Ceiling(fontSize));
        }
    }
}