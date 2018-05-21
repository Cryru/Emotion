// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

namespace Emotion.Primitives
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