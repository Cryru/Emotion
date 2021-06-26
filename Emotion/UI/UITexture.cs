#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UITexture : UISolidColor
    {
        /// <summary>
        /// Path to the texture file to load.
        /// </summary>
        public string TextureFile;

        /// <summary>
        /// The size of the window. If not specified either the UV or parent size will be taken.
        /// </summary>
        public Vector2? RenderSize;

        /// <summary>
        /// An override scale for the image. By default it is drawn either in full size, uv size, or render size.
        /// </summary>
        public Vector2? ImageScale;

        /// <summary>
        /// Custom UVs for the texture. By default the whole texture is drawn.
        /// </summary>
        public Rectangle? UV;

        /// <summary>
        /// Whether to enable smoothing on the texture. This is only done when loaded and will overwrite the
        /// texture setting for other users using the texture.
        /// </summary>
        public bool Smooth;

        protected TextureAsset _textureFile;

        public override async Task Preload()
        {
            await base.Preload();
            _textureFile = await Engine.AssetLoader.GetAsync<TextureAsset>(TextureFile);

            if (_textureFile == null) return;

            if (Smooth != _textureFile.Texture.Smooth) GLThread.ExecuteGLThread(() => { _textureFile.Texture.Smooth = Smooth; });
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            float scale = GetScale();
            Vector2 size;
            if (RenderSize != null)
                size = RenderSize.Value;
            else if (UV != null)
                size = UV.Value.Size * scale;
            else if (_textureFile != null)
                size = _textureFile.Texture.Size * scale;
            else
                size = Vector2.Zero;

            if (ImageScale != null) size *= ImageScale.Value;
            return size;
        }

        protected override bool RenderInternal(RenderComposer c, ref Color windowColor)
        {
            if (_textureFile == null) return base.RenderInternal(c, ref windowColor);
            c.RenderSprite(Position, Size, windowColor, _textureFile.Texture, UV);
            return true;
        }
    }
}