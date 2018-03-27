// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.Components
{
    public class Transform
    {
        /// <summary>
        /// The object's position and size.
        /// </summary>
        public Rectangle Bounds;

        public Transform(Rectangle bounds)
        {
            Bounds = bounds;
        }
    }
}