// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Objects.Bases;
using Emotion.Primitives;

#endregion

namespace Emotion.Objects.Bases
{
    public class Camera : Transform
    {
        #region Properties

        /// <summary>
        /// The transform the camera should follow.
        /// </summary>
        public Transform Target { get; set; }

        #endregion

        public Camera(Rectangle bounds) : base(bounds)
        {
        }

        public void SnapToTarget()
        {
            Bounds.Center = Target.Bounds.Center;
        }
    }
}