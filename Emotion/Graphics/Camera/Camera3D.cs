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
                RecreateProjectionMatrix();
            }
        }

        private float _fieldOfView = 45;

        // Debug camera movement.
        public float DebugMovementSpeed = 3;
        private Vector2 _lastMousePos;
        private Vector3 _yawPitchRoll = Vector3.Zero;

        public Camera3D(Vector3 position, float zoom = 1) : base(position, zoom)
        {
            NearZ = 0.1f;
            FarZ = 10_000;
        }

        /// <inheritdoc />
        public override void Update()
        {
        }

        public void LookAtPoint(Vector3 point)
        {
            LookAt = Vector3.Normalize(point - Position);
        }

        protected override void LookAtChanged(Vector3 oldVal, Vector3 newVal)
        {
            float roll = MathF.Asin(newVal.Z);
            float yaw;
            if (newVal.Z < 0)
                yaw = MathF.PI + MathF.Atan2(-newVal.Y, -newVal.X);
            else
                yaw = MathF.Atan2(newVal.Y, newVal.X);

            _yawPitchRoll = new Vector3(Maths.RadiansToDegrees(yaw), 0, Maths.RadiansToDegrees(roll));
            base.LookAtChanged(oldVal, newVal);
        }

        public void DefaultMovementLogicUpdate()
        {
            Vector2 mousePos = Engine.Host.MousePosition;
            if (Engine.Host.IsKeyHeld(Key.MouseKeyLeft) || Engine.Host.IsKeyHeld(Key.MouseKeyMiddle))
            {
                float xOffset = mousePos.X - _lastMousePos.X;
                float yOffset = mousePos.Y - _lastMousePos.Y;
                _yawPitchRoll.X += xOffset * 0.1f;
                _yawPitchRoll.Z += yOffset * 0.1f;
                _yawPitchRoll.Z = Maths.Clamp(_yawPitchRoll.Z, -89, 89); // Prevent flip.
                var direction = new Vector3
                {
                    X = MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.X)) * MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.Z)),
                    Y = MathF.Sin(Maths.DegreesToRadians(_yawPitchRoll.X)) * MathF.Cos(Maths.DegreesToRadians(_yawPitchRoll.Z)),
                    Z = MathF.Sin(Maths.DegreesToRadians(_yawPitchRoll.Z))
                };
                direction = Vector3.Normalize(direction);
                _lookAt = direction;
            }

            _lastMousePos = mousePos;

            float dirX = 0;
            float dirZ = 0;
            float dirY = 0;
            if (Engine.Host.IsKeyHeld(Key.W))
                dirX += 1;
            else if (Engine.Host.IsKeyHeld(Key.S))
	            dirX -= 1;

            if (Engine.Host.IsKeyHeld(Key.Space))
                dirZ += 1;
            else if (Engine.Host.IsKeyHeld(Key.LeftShift))
                dirZ -= 1;

            if (Engine.Host.IsKeyHeld(Key.A))
                dirY += 1;
            else if (Engine.Host.IsKeyHeld(Key.D))
	            dirY -= 1;

            Vector3 movementStraightBack = Engine.Renderer.Camera.LookAt * dirX;
            float len = movementStraightBack.Length();
            movementStraightBack.Z = 0;
            movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

            Vector3 movementUpDown = RenderComposer.Up * dirZ;
            Vector3 movementSide = Vector3.Normalize(Vector3.Cross(RenderComposer.Up * dirY, Engine.Renderer.Camera.LookAt));
            if (!float.IsNaN(movementStraightBack.X)) Engine.Renderer.Camera.Position += movementStraightBack * DebugMovementSpeed;
            if (!float.IsNaN(movementUpDown.X)) Engine.Renderer.Camera.Position += movementUpDown * DebugMovementSpeed;
            if (!float.IsNaN(movementSide.X)) Engine.Renderer.Camera.Position += movementSide * DebugMovementSpeed;
            // todo: interpolate.

            Engine.Renderer.Camera.RecreateViewMatrix();
        }

        /// <inheritdoc />
        public override void RecreateViewMatrix()
        {
            Vector3 pos = Position;
            var unscaled = Matrix4x4.CreateLookAt(pos, pos + LookAt, RenderComposer.Up);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1), pos) * unscaled;
        }

        /// <inheritdoc />
        public override void RecreateProjectionMatrix()
        {
            RenderComposer renderer = Engine.Renderer;
            float aspectRatio = renderer.CurrentTarget.Size.X / renderer.CurrentTarget.Size.Y;
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(_fieldOfView), aspectRatio, Maths.Clamp(NearZ, 0.1f, FarZ), FarZ);
        }

		public override Vector3 ScreenToWorld(Vector2 position)
		{
			// Calculate the normalized device coordinates (-1 to 1)
			float screenWidth = Engine.Renderer.CurrentTarget.Size.X;
			float screenHeight = Engine.Renderer.CurrentTarget.Size.Y;
			float x = (2.0f * position.X) / screenWidth - 1.0f;
			float y = 1.0f - (2.0f * position.Y) / screenHeight;

			// Reverse projection
			var clipSpace = new Vector4(x, y, 1f, 1f);
			Vector4 rayEye = Vector4.Transform(clipSpace, ProjectionMatrix.Inverted());
			rayEye.Z = -1f;
			rayEye.W = 0f;

			// Reverse view
			Vector3 direction = Vector4.Transform(rayEye, ViewMatrix.Inverted()).ToVec3();
			direction = Vector3.Normalize(direction);

            return Position + direction;
		}

		public Ray3D GetCameraMouseRay()
		{
			Vector3 dir = ScreenToWorld(Engine.Host.MousePosition);
			return new Ray3D(Position, dir - Position);
		}
	}
}