#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.IO;

#endregion

namespace Emotion.Game.Animation
{
    /// <inheritdoc />
    public sealed class AnimatedTextureDescription : AnimationDescriptionBase
    {
        public Vector2 FrameSize { get; set; }
        public Vector2 Spacing { get; set; }

        public override IAnimatedTexture CreateFrom()
        {
            var t = Engine.AssetLoader.Get<TextureAsset>(SpriteSheetName);
            return t == null ? null : new AnimatedTexture(t.Texture, FrameSize, Spacing, LoopType, TimeBetweenFrames, StartingFrame, EndingFrame);
        }
    }
}