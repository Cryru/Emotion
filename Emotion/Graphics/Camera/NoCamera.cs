#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera
{
    /// <summary>
    /// Empty camera.
    /// </summary>
    public class NoCamera : CameraBase
    {
        public NoCamera(Vector3 position, float zoom = 1f) : base(position, zoom)
        {
        }

        /// <inheritdoc />
        public override void RecreateMatrix()
        {
        }

        /// <inheritdoc />
        public override void Update()
        {
        }
    }
}