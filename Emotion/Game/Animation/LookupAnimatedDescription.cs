#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation
{
    /// <inheritdoc />
    public sealed class LookupAnimatedDescription : AnimationDescriptionBase
    {
        public Rectangle[] Frames;

        public override IAnimatedTexture CreateFrom()
        {
            var t = Engine.AssetLoader.Get<TextureAsset>(SpriteSheetName);
            return t == null ? null : new LookupAnimatedTexture(t.Texture, Frames, LoopType, TimeBetweenFrames, StartingFrame, EndingFrame);
        }
    }
}