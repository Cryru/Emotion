﻿#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Game.Animation2D
{
    /// <summary>
    /// Contains the information needed to render and animate a sprite.
    /// This class is initialized and managed by the animation editor.
    /// </summary>
    public class AnimatedSprite
    {
        public string AssetFile;
        public SpriteAnimationFrameSource FrameSource;
        public Dictionary<string, SpriteAnimationData> Animations;
    }
}