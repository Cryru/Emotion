// Emotion - https://github.com/Cryru/Emotion

#region Using

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
                _updateMatrix = true;
            }
        }

        private float _zoom = 1f;
        private bool _updateMatrix = true;

        #endregion

        public CameraBase(Vector3 position, Vector2 size) : base(position, size)
        {
            UpdateMatrix();
            OnMove += (a, b) => _updateMatrix = true;
            OnResize += (a, b) => _updateMatrix = true;
        }

        public virtual void Update()
        {
            UpdateMatrix();
        }

        protected virtual void UpdateMatrix()
        {
            if (!_updateMatrix) return;
            ViewMatrix = Matrix4.CreateTranslation(Width / 2, Height / 2, Z).Inverted() * Matrix4.CreateScale(Zoom) * Matrix4.CreateTranslation(Width / 2, Height / 2, Z) *
                         Matrix4.CreateTranslation(-(int) X, -(int) Y, Z);
            _updateMatrix = false;
        }
    }
}