﻿#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Text;
using Emotion.Utility;

#endregion

namespace Emotion.IO
{
    public class FontAsset : Asset
    {
        /// <summary>
        /// The Emotion.Standard.Text font generated from the font file.
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

        protected override void CreateInternal(byte[] data)
        {
            Font = new Font(data);
        }

        protected override void DisposeInternal()
        {
            foreach (KeyValuePair<int, DrawableFontAtlas> atlas in _loadedAtlases)
            {
                atlas.Value.Dispose();
            }

            _loadedAtlases.Clear();
        }

        public static void SetRasterizer(Font.GlyphRasterizer rasterizer)
        {
            _rasterizer = rasterizer;
        }

        public DrawableFontAtlas GetAtlas(float fontSize, uint firstChar = 0, int numChars = -1, bool smooth = true)
        {
            int hash = $"{fontSize}-{firstChar}-{numChars}".GetHashCode();

            // Check if the atlas is already loaded.
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) return atlas;

            // Load the atlas manually.
            FontAtlas standardAtlas = Font.GetAtlas(fontSize, firstChar, numChars, _rasterizer);
            atlas = new DrawableFontAtlas(standardAtlas, smooth);

            _loadedAtlases.Add(hash, atlas);

            return atlas;
        }

        public void DestroyAtlas(float fontSize, int firstChar = 0, int numChars = -1)
        {
            int hash = $"{fontSize}-{firstChar}-{numChars}".GetHashCode();
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) atlas.Dispose();
        }
    }

    public class DrawableFontAtlas : IDisposable
    {
        /// <summary>
        /// The atlas data itself.
        /// </summary>
        public FontAtlas Atlas { get; protected set; }

        /// <summary>
        /// The atlas texture.
        /// </summary>
        public Texture Texture { get; protected set; }

        public DrawableFontAtlas(FontAtlas atlas, bool smooth = true)
        {
            // Invalid font, no glyphs, etc.
            if (atlas == null) return;

            Atlas = atlas;

            GLThread.ExecuteGLThread(() =>
            {
                Texture = new Texture(Atlas.Size, ImageUtil.AToRgba(Atlas.Pixels)) {Smooth = smooth};
            });

            // Free memory.
            Atlas.Pixels = null;
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}