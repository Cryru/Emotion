using System;
using System.Numerics;
using Emotion.Utility;

namespace Emotion.Primitives
{
    public class Transform3D : Transform
    {
         public float Depth
        {
            get => _depth;
            set
            {
                if (value == _depth) return;
                _depth = value;
                Resized();
            }
        }

        private float _depth = 1;

        public float Scale
        {
            get
            {
                if (_width == _height && _height == _depth) return _width;
                return float.NaN;
            }
            set
            {
                _width = value;
                _height = value;
                _depth = value;
                Resized();
            }
        }

        public Vector3 Rotation
        {
            get => _rotationRad;
            set
            {
                if (_rotationRad == value) return;
                _rotationRad = value;
                Rotated();
            }
        }

        public Vector3 RotationDeg
        {
            get => new Vector3(Maths.RadiansToDegrees(_rotationRad.X), Maths.RadiansToDegrees(_rotationRad.Y), Maths.RadiansToDegrees(_rotationRad.Z));
            set
            {
                _rotationRad = new Vector3(Maths.DegreesToRadians(value.X), Maths.DegreesToRadians(value.Y), Maths.DegreesToRadians(value.Z));
                Rotated();
            }
        }

        private Vector3 _rotationRad;

        public event EventHandler OnRotate;

        protected override void Resized()
        {
            _scaleMatrix = Matrix4x4.CreateScale(_width, _height, _depth);
            base.Resized();
        }

        protected override void Moved()
        {
            _translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
            base.Moved();
        }

        protected virtual void Rotated()
        {
            _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotationRad.Y, _rotationRad.X, _rotationRad.Z);
            OnRotate?.Invoke(this, EventArgs.Empty);
        }

        protected Matrix4x4 _rotationMatrix;
        protected Matrix4x4 _scaleMatrix;
        protected Matrix4x4 _translationMatrix;

        public Transform3D()
        {
            _width = 1;
            _height = 1;
            Resized();
            Moved();
            Rotated();
        }
    }
}