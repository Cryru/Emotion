// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using Emotion.Objects.Bases;

#endregion

namespace Emotion.Objects.Game
{
    public class Camera : Transform
    {
        #region Properties

        /// <summary>
        /// The transform the camera should follow.
        /// </summary>
        public Transform Target { get; set; }

        public Rectangle InnerBounds
        {
            get
            {
                // Generate rectangle from the bounds size, and center it on the camera bounds.
                Rectangle temp = new Rectangle(0, 0, _innerBoundSize.X, _innerBoundSize.Y);
                temp.X = Bounds.GetCenter().X - temp.Width / 2;
                temp.Y = Bounds.GetCenter().Y - temp.Height / 2;

                return temp;
            }
            set => _innerBoundSize = new Point(value.Width, value.Height);
        }

        private Point _innerBoundSize;

        #endregion

        public Camera(Rectangle bounds) : base(bounds)
        {
            _innerBoundSize = new Point((int) (bounds.Width - bounds.Width * 0.25), (int) (bounds.Height - bounds.Height * 0.25));
        }

        public void Update()
        {
            // Check if no target.
            if (Target == null) return;

            Center = new Point(Target.Center.X, Target.Center.Y);
        }
    }
}