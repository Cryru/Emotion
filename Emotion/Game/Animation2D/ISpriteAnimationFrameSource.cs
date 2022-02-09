#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation2D
{
    public interface ISpriteAnimationFrameSource
    {
        public int GetFrameCount();
        public Rectangle GetFrameUV(int i);
    }
}