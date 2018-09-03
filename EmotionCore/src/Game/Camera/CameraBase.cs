// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Camera
{
    public class CameraBase : Transform
    {
        #region Properties

        /// <summary>
        /// The camera's matrix.
        /// </summary>
        public Matrix4 ViewMatrix { get; protected set; }

        #endregion

        public CameraBase(Rectangle bounds) : base(bounds)
        {
            ViewMatrix = Matrix4.Identity;
        }

        public virtual void Update(Context _)
        {
            if (!_transformUpdated) return;
            ViewMatrix = Matrix4.CreateTranslation(X, Y, Z);
            _transformUpdated = false;
        }
    }
}