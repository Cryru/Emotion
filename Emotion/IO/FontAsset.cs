#region Using

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
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
        private Dictionary<int, DrawableFontAtlas> _loadedAtlases = new Dictionary<int, DrawableFontAtlas>();

        /// <summary>
        /// The rasterizer to use for getting atlases.
        /// </summary>
        private static Font.GlyphRasterizer _rasterizer = Font.GlyphRasterizer.Emotion;

        /// <inheritdoc />
        protected override void CreateInternal(byte[] data)
        {
            Font = new Font(data);
        }

        /// <inheritdoc />
        protected override void DisposeInternal()
        {
            foreach (KeyValuePair<int, DrawableFontAtlas> atlas in _loadedAtlases)
            {
                atlas.Value.Dispose();
            }

            _loadedAtlases.Clear();
        }

        /// <summary>
        /// Set the rasterizer to use for subsequent atlas generation.
        /// </summary>
        /// <param name="rasterizer"></param>
        public static void SetRasterizer(Font.GlyphRasterizer rasterizer)
        {
            _rasterizer = rasterizer;
        }

        /// <summary>
        /// Get a font atlas that can be used for text drawing.
        /// Atlases are cached, so requesting the same one twice will return the same reference.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="firstChar">The codepoint of the first character to include in the atlas.</param>
        /// <param name="numChars">The number of characters to include in the atlas, after the first character.</param>
        /// <param name="smooth">Whether to apply bilinear filtering to the texture. You most likely want this.</param>
        /// <returns></returns>
        public DrawableFontAtlas GetAtlas(int fontSize, uint firstChar = 0, int numChars = -1, bool smooth = true)
        {
            int hash = $"{fontSize}-{firstChar}-{numChars}".GetHashCode();

            // Check if the atlas is already loaded.
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) return atlas;

            // Load the atlas manually.
            PerfProfiler.ProfilerEventStart($"FontAtlas {Name} {fontSize} {hash}", "Loading");
            FontAtlas standardAtlas = Font.GetAtlas(fontSize, firstChar, numChars, _rasterizer);
            atlas = new DrawableFontAtlas(standardAtlas, smooth);
            PerfProfiler.ProfilerEventEnd($"FontAtlas {Name} {fontSize} {hash}", "Loading");

            _loadedAtlases.Add(hash, atlas);

            return atlas;
        }

        /// <summary>
        /// Get a font atlas that can be used for text drawing.
        /// Atlases are cached, so requesting the same one twice will return the same reference.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="firstChar">The codepoint of the first character to include in the atlas.</param>
        /// <param name="numChars">The number of characters to include in the atlas, after the first character.</param>
        /// <param name="smooth">Whether to apply bilinear filtering to the texture. You most likely want this.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DrawableFontAtlas GetAtlas(float fontSize, uint firstChar = 0, int numChars = -1, bool smooth = true)
        {
            var intFontSize = (int) MathF.Ceiling(fontSize);
            return GetAtlas(intFontSize, firstChar, numChars, smooth);
        }

        /// <summary>
        /// Free memory by destroying a cached atlas.
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="firstChar"></param>
        /// <param name="numChars"></param>
        public void DestroyAtlas(int fontSize, int firstChar = 0, int numChars = -1)
        {
            fontSize = (int) MathF.Ceiling(fontSize);
            int hash = $"{fontSize}-{firstChar}-{numChars}".GetHashCode();
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) atlas.Dispose();
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
            DestroyAtlas((int) MathF.Ceiling(fontSize), firstChar, numChars);
        }
    }

    /// <summary>
    /// An uploaded to the GPU font atlas.
    /// </summary>
    public class DrawableFontAtlas : IDisposable
    {
        /// <summary>
        /// The atlas data itself. Pixels are stripped for less memory usage after upload.
        /// </summary>
        public FontAtlas Atlas { get; protected set; }

        /// <summary>
        /// The atlas texture.
        /// </summary>
        public Texture Texture { get; protected set; }

        /// <summary>
        /// Upload a font atlas texture to the gpu. Also holds a reference to the atlas itself
        /// for its metadata.
        /// </summary>
        /// <param name="atlas">The atlas texture to upload.</param>
        /// <param name="smooth">Whether to apply bilinear filtering to the texture.</param>
        public DrawableFontAtlas(FontAtlas atlas, bool smooth = true)
        {
            // Invalid font, no glyphs, etc.
            if (atlas == null) return;

            Atlas = atlas;

            GLThread.ExecuteGLThread(() => { Texture = new Texture(Atlas.Size, ImageUtil.AToRgba(Atlas.Pixels)) {Smooth = smooth}; });

            // Free memory.
            Atlas.Pixels = null;
        }

        /// <summary>
        /// Clear resources.
        /// </summary>
        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}