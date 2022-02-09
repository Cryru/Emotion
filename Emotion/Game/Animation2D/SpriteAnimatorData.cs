#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Game.Animation2D
{
    public class SpriteAnimatorData
    {
        public string AssetFile;
        public ISpriteAnimationFrameSource FrameSource;
        public Dictionary<string, SpriteAnimationData> Animations;
    }
}