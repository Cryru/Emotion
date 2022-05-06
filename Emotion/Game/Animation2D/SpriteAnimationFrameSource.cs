#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Game.Animation2D
{
    public enum OriginPosition
    {
        TopLeft,
        TopCenter,
        TopRight,

        CenterLeft,
        CenterCenter,
        CenterRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public abstract class SpriteAnimationFrameSource
    {
        /// <summary>
        /// Origin of the sprite frames relative to the object's origin.
        /// </summary>
        public OriginPosition[]? FrameOrigins;

        /// <summary>
        /// An additional offset to the frame's position relative to its origin.
        /// </summary>
        public Vector2[]? FrameOffsets;

        /// <summary>
        /// Get the total number of frames within the source.
        /// </summary>
        public abstract int GetFrameCount();

        /// <summary>
        /// Get the UV of the frame index.
        /// </summary>
        public abstract Rectangle GetFrameUV(int i);

        /// <summary>
        /// Enumerate through all frame uvs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Rectangle> GetFrameUVEnumerator()
        {
            for (var i = 0; i < GetFrameCount(); i++)
            {
                yield return GetFrameUV(i);
            }
        }
    }
}