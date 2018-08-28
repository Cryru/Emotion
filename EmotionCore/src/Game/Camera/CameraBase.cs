// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Camera
{
    public class CameraBase : Transform
    {
        #region Properties

        /// <summary>
        /// The transform the camera should follow.
        /// </summary>
        public Transform Target { get; set; }

        #endregion

        public CameraBase(Rectangle bounds) : base(bounds)
        {
        }

        public void SnapToTarget()
        {
            Center = Target.Bounds.Center;
        }
    }
}