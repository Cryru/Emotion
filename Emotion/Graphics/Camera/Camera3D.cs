#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera
{
    public class Camera3D : CameraBase
    {
        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                RecreateMatrix();
            }
        }

        private float _fieldOfView = 45;

        // Debug camera movement.
        private Vector2 _lastMousePos;
        private Vector3 _yawPitchRoll = Vector3.Zero; // todo: First click changes look at a lot.

        public Camera3D(Vector3 position, float zoom = 1) : base(position, zoom)
        {
        }

        /// <inheritdoc />
        public override void Update()
        {
            Vector2 mousePos = Engine.Host.MousePosition;

            if (Engine.Host.IsKeyHeld(Key.MouseKeyLeft))
            {
                float xOffset = _lastMousePos.X - mousePos.X;
                float yOffset = mousePos.Y - _lastMousePos.Y;
                _yawPitchRoll.X += xOffset * 0.1f;
                _yawPitchRoll.Y += yOffset * 0.1f;
                _yawPitchRoll.Y = Maths.Clamp(_yawPitchRoll.Y, -89, 89); // Prevent flip.
                var direction = new Vector3
                {
                    X = MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.X)) * MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.Y)),
                    Y = MathF.Sin(Maths.DegreesToRadians(_yawPitchRoll.Y)),
                    Z = MathF.Sin(Maths.DegreesToRadians(_yawPitchRoll.X)) * MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.Y))
                };
                direction = Vector3.Normalize(direction);
                Engine.Renderer.Camera.LookAt = direction;
            }

            _lastMousePos = mousePos;

            float dirX = 0;
            float dirY = 0;
            if (Engine.Host.IsKeyHeld(Key.W))
                dirX += 1;
            else if (Engine.Host.IsKeyHeld(Key.S)) dirX -= 1;

            if (Engine.Host.IsKeyHeld(Key.A))
                dirY -= 1;
            else if (Engine.Host.IsKeyHeld(Key.D)) dirY = 1;

            Vector3 movementStraightBack = Engine.Renderer.Camera.LookAt * dirX;
            Vector3 movementSide = Vector3.Normalize(Vector3.Cross(new Vector3(0, -dirY, 0), Engine.Renderer.Camera.LookAt));
            if (!float.IsNaN(movementStraightBack.X)) Engine.Renderer.Camera.Position += movementStraightBack * 3;
            if (!float.IsNaN(movementSide.X)) Engine.Renderer.Camera.Position += movementSide * 3;

            Engine.Renderer.Camera.RecreateMatrix();
        }

        /// <inheritdoc />
        public override void RecreateMatrix()
        {
            Vector3 pos = Position;
            ViewMatrixUnscaled = Matrix4x4.CreateLookAt(pos, pos + LookAt, new Vector3(0.0f, 1.0f, 0.0f));
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1), pos) * ViewMatrixUnscaled;
        }

        /// <inheritdoc />
        public override Matrix4x4 GetProjection()
        {
            RenderComposer renderer = Engine.Renderer;
            float aspectRatio = renderer.CurrentTarget.Size.X / renderer.CurrentTarget.Size.Y;
            return Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(_fieldOfView), aspectRatio, Maths.Clamp(renderer.NearZ, 0.1f, renderer.FarZ), renderer.FarZ);
        }
    }
}