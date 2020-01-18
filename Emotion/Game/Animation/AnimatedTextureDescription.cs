#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// A description file which can be serialized/deserialized and from which an animated texture can be created.
    /// </summary>
    public sealed class AnimatedTextureDescription
    {
        public string SpriteSheetName { get; set; }
        public int StartingFrame { get; set; }
        public int EndingFrame { get; set; }
        public int TimeBetweenFrames { get; set; }
        public AnimationLoopType LoopType { get; set; }
        public Rectangle[] Frames { get; set; }
        public Vector2[] Anchors { get; set; }

        public AnimatedTexture CreateFrom()
        {
            var t = Engine.AssetLoader.Get<TextureAsset>(SpriteSheetName);
            return t == null ? null : new AnimatedTexture(t.Texture, Frames, LoopType, TimeBetweenFrames, StartingFrame, EndingFrame) { Anchors = Anchors };
        }
    }
}