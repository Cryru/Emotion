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

        /// <summary>
        /// How zoomed the camera is.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                _transformUpdated = true;
            }
        }

        private float _zoom = 1f;

        #endregion

        public CameraBase(Rectangle bounds) : base(bounds)
        {
            UpdateMatrix();
        }

        public virtual void Update(Context _)
        {
            UpdateMatrix();
        }

        protected virtual void UpdateMatrix()
        {
            if (!_transformUpdated) return;
            ViewMatrix = Matrix4.CreateTranslation(Width / 2, Height / 2, Z).Inverted() * Matrix4.CreateScale(Zoom) * Matrix4.CreateTranslation(Width / 2, Height / 2, Z) *
                         Matrix4.CreateTranslation(-(int) X, -(int) Y, Z);
            _transformUpdated = false;
        }
    }
}