#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Serialization;
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

        [DontSerialize]
        public TextureAsset TextureAsset { get; protected set; }

        public UITexture()
        {

        }

        public UITexture(TextureAsset texture)
        {
            TextureAsset = texture;
        }

        public override async Task Preload()
        {
            await base.Preload();
            if (TextureFile == null) return;

            TextureAsset = await Engine.AssetLoader.GetAsync<TextureAsset>(TextureFile);
            if (TextureAsset == null) return;
            if (Smooth != TextureAsset.Texture.Smooth) GLThread.ExecuteGLThread(() => { TextureAsset.Texture.Smooth = Smooth; });
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            float scale = GetScale();
            Vector2 size;
            if (RenderSize != null)
                size = GetRenderSizeProcessed(space);
            else if (UV != null)
                size = UV.Value.Size * scale;
            else if (TextureAsset != null)
                size = TextureAsset.Texture.Size * scale;
            else
                size = Vector2.Zero;

            if (ImageScale != null) size *= ImageScale.Value;
            return size;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (TextureAsset == null) return base.RenderInternal(c);
            c.RenderSprite(Position, Size, _calculatedColor, TextureAsset.Texture, UV);
            return true;
        }

        private Vector2 GetRenderSizeProcessed(Vector2 space)
        {
            float scale = GetScale();
            float xVal = RenderSize!.Value.X;
            float yVal = RenderSize!.Value.Y;

            // Percentage of space.
            if (xVal < 0)
            {
                // Percentage of height.
                if (xVal < -100)
                {
                    xVal = space.Y * ((-xVal - 100) / 100.0f);
                }
                else
                {
                    xVal = space.X * (-xVal / 100.0f);
                }
            }
            else
            {
                xVal *= scale;
            }

            if (yVal < 0)
            {
                if (yVal < -100)
                {
                    yVal = space.X * ((-yVal - 100) / 100.0f);
                }
                else
                {
                    yVal = space.Y * (-yVal / 100.0f);
                }
            }
            else
            {
                yVal *= scale;
            }

            return new Vector2(xVal, yVal);
        }
    }
}