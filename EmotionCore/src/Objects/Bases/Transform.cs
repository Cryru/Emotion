// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;

#endregion

namespace Emotion.Objects.Bases
{
    public class Transform
    {
        /// <summary>
        /// The object's position and size.
        /// </summary>
        public Rectangle Bounds;

        /// <summary>
        /// The object's center.
        /// </summary>
        public Point Center
        {
            get => new Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
            set
            {
                Bounds.X = value.X - Bounds.Width / 2;
                Bounds.Y = value.Y - Bounds.Height / 2;
            }
        }

        public Transform(Rectangle bounds)
        {
            Bounds = bounds;
        }
    }
}