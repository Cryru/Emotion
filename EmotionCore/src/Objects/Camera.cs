// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using Emotion.Objects.Bases;

#endregion

namespace Emotion.Objects
{
    public class Camera : Transform
    {
        private Transform _target;

        public Camera(Rectangle bounds) : base(bounds)
        {
        }

        public void Update()
        {
        }

        public void Follow(Transform target)
        {
            _target = target;
            Center = new Point(target.Center.X, target.Center.Y);
        }
    }
}