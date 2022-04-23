#region Using

using System.Collections.Generic;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation2D
{
    public interface ISpriteAnimationFrameSource
    {
        public int GetFrameCount();
        public Rectangle GetFrameUV(int i);

        public IEnumerator<Rectangle> GetFrameUVEnumerator()
        {
            for (var i = 0; i < GetFrameCount(); i++)
            {
                yield return GetFrameUV(i);
            }
        }
    }
}