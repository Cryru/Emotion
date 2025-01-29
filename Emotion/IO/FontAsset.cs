#region Using

using System.Runtime.CompilerServices;
using Emotion.Graphics.Text;
using Emotion.Graphics.Text.EmotionRenderer;
using Emotion.Graphics.Text.EmotionSDF;
using Emotion.Graphics.Text.EmotionSDF3;
using Emotion.Graphics.Text.StbRenderer;
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
        private Dictionary<int, DrawableFontAtlas> _loadedAtlases = new Dictionary<int, DrawableFontAtlas>();

        /// <inheritdoc />
        protected override void CreateInternal(ReadOnlyMemory<byte> data)
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

        public static GlyphRasterizer GlyphRasterizer { get; set; } = GlyphRasterizer.EmotionSDFVer4;

        public DrawableFontAtlas GetAtlas(int fontSize, bool pixelFont = false)
        {
            int hash = HashCode.Combine(fontSize, GlyphRasterizer);

            // Check if the atlas is already loaded.
            bool found = _loadedAtlases.TryGetValue(hash, out DrawableFontAtlas atlas);
            if (found) return atlas;

            lock (_loadedAtlases)
            {
                // Recheck as another thread could have built the atlas while waiting on lock.
                found = _loadedAtlases.TryGetValue(hash, out atlas);
                if (found) return atlas;

                // Get atlas of specified type.
                switch (GlyphRasterizer)
                {
                    case GlyphRasterizer.StbTrueType:
                        atlas = new StbDrawableFontAtlas(Font, fontSize, pixelFont);
                        break;
                    case GlyphRasterizer.Emotion:
                        atlas = new EmotionRendererDrawableFontAtlas(Font, fontSize, pixelFont);
                        break;
                    case GlyphRasterizer.EmotionSDFVer3:
                        atlas = new EmotionSDF3DrawableFontAtlas(Font, fontSize, pixelFont);
                        break;
                    case GlyphRasterizer.EmotionSDFVer4:
                        atlas = new EmotionSDFDrawableFontAtlas(Font, fontSize, pixelFont);
                        break;
                    default:
                        return null;
                }

                // Cache default ascii set
                atlas.CacheGlyphs(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
                _loadedAtlases.Add(hash, atlas);
            }

            return atlas;
        }

        /// <inheritdoc cref="GetAtlas(float, bool)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DrawableFontAtlas GetAtlas(float fontSize, bool pixelFont = false)
        {
            var intFontSize = (int) MathF.Ceiling(fontSize); // Ceil so we dont store atlases for every floating deviation.
            return GetAtlas(intFontSize, pixelFont);
        }

        /// <summary>
        /// Free memory by destroying a cached atlas.
        /// </summary>
        public void DestroyAtlas(int fontSize, GlyphRasterizer? rasterizer = null)
        {
            int hash = HashCode.Combine(fontSize, rasterizer ?? GlyphRasterizer);
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
            DestroyAtlas((int) MathF.Ceiling(fontSize));
        }

        public static string DefaultBuiltInFontName = AssetLoader.NameToEngineName("Editor/UbuntuMono-Regular.ttf");

        /// <summary>
        /// Loads and returns the default font shipped with the engine.
        /// This font is only available if Editor assets are included
        /// and should only be used for prototyping and such.
        /// </summary>
        /// <returns></returns>
        public static FontAsset GetDefaultBuiltIn()
        {
            return Engine.AssetLoader.Get<FontAsset>(DefaultBuiltInFontName);
        }
    }
}