﻿#region Using

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

        public Key DragKey = Key.MouseKeyLeft;

        // Movement
        public float MovementSpeed = 3;
        private Vector2 _lastMousePos;
        private Vector3 _yawRollPitch = Vector3.Zero;
        private bool _held;
        private Vector2 _inputDirection;
        private float _inputDirectionZ;

        public Camera3D(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
        {
            NearZ = 10f;
            FarZ = 10_000;
        }

        /// <inheritdoc />
        public override void RecreateViewMatrix()
        {
            Vector3 pos = Position;
            var unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos + LookAt, RenderComposer.Up);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1), pos) * unscaled;
        }

        /// <inheritdoc />
        public override void RecreateProjectionMatrix()
        {
            RenderComposer renderer = Engine.Renderer;
            float aspectRatio = renderer.CurrentTarget.Size.X / renderer.CurrentTarget.Size.Y;
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(Maths.DegreesToRadians(_fieldOfView), aspectRatio, Maths.Clamp(NearZ, 0.1f, FarZ), FarZ);
        }

        protected override bool CameraKeyHandler(Key key, KeyStatus status)
        {
            bool dragKey = key == DragKey;
            if (DragKey != Key.MouseKeyLeft) // Secondary way of moving camera in editor.
            {
                bool leftClickWithControlOrLeftClickLetGo = key == Key.MouseKeyLeft && (Engine.Host.IsCtrlModifierHeld() || status == KeyStatus.Up);
                dragKey = dragKey || leftClickWithControlOrLeftClickLetGo;
            }

            if (dragKey)
            {
                if (status == KeyStatus.Down)
                {
                    _lastMousePos = Engine.Host.MousePosition;
                    _held = true;
                    return false;
                }

                if (status == KeyStatus.Up)
                {
                    _held = false;
                    return false;
                }
            }

            Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
            if (keyAxisPart != Vector2.Zero)
            {
                if (status == KeyStatus.Down)
                    _inputDirection += keyAxisPart;
                else if (status == KeyStatus.Up)
                    _inputDirection -= keyAxisPart;
                return false;
            }

            if (key == Key.Space || key == Key.LeftShift)
            {
                if (status == KeyStatus.Down)
                {
                    if (key == Key.Space)
                        _inputDirectionZ += 1;
                    else
                        _inputDirectionZ -= 1;
                }
                else if (status == KeyStatus.Up)
                {
                    if (key == Key.Space)
                        _inputDirectionZ -= 1;
                    else
                        _inputDirectionZ += 1;
                }

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (_held)
            {
                Vector2 mousePos = Engine.Host.MousePosition;
                float xOffset = -(mousePos.X - _lastMousePos.X);
                float yOffset = mousePos.Y - _lastMousePos.Y;
                _yawRollPitch.X += xOffset * 0.1f;
                _yawRollPitch.Z += yOffset * 0.1f;
                _yawRollPitch.Z = Maths.Clamp(_yawRollPitch.Z, -89, 89); // Prevent flip.
                var direction = new Vector3
                {
                    X = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.X)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)),
                    Y = -MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.X)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)),
                    Z = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.Z))
                };
                direction = Vector3.Normalize(direction);
                _lookAt = direction;
                _lastMousePos = mousePos;

                RecreateViewMatrix();
            }

            if (_inputDirection != Vector2.Zero || _inputDirectionZ != 0)
            {
                Vector3 movementStraightBack = LookAt * -_inputDirection.Y;
                float len = movementStraightBack.Length();
                movementStraightBack.Z = 0;
                movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

                Vector3 movementUpDown = RenderComposer.Up * _inputDirectionZ;
                Vector3 movementSide = -Vector3.Normalize(Vector3.Cross(LookAt, RenderComposer.Up)) * _inputDirection.X;
                if (!float.IsNaN(movementStraightBack.X)) Position += movementStraightBack * MovementSpeed;
                if (!float.IsNaN(movementUpDown.X)) Position += movementUpDown * MovementSpeed;
                if (!float.IsNaN(movementSide.X)) Position += movementSide * MovementSpeed;
                // todo: interpolate.

                RecreateViewMatrix();
            }
        }

        protected override void LookAtChanged(Vector3 oldVal, Vector3 newVal)
        {
            float roll = MathF.Asin(newVal.Z);
            float yaw;
            if (newVal.Z < 0)
                yaw = MathF.PI + MathF.Atan2(-newVal.Y, -newVal.X);
            else
                yaw = MathF.Atan2(newVal.Y, newVal.X);

            _yawRollPitch = new Vector3(Maths.RadiansToDegrees(yaw), 0, Maths.RadiansToDegrees(roll));

            // Prevent look at facing towards or out of RenderComposer.Up (gimbal lock)
            _yawRollPitch.Z = Maths.Clamp(_yawRollPitch.Z, -89, 89);
            _lookAt.Y = -MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.X)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z));
            _lookAt.Z = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.Z));

            base.LookAtChanged(oldVal, newVal);
        }

        /// <inheritdoc />
        public override Vector3 ScreenToWorld(Vector2 position)
        {
            // Calculate the normalized device coordinates (-1 to 1)
            float screenWidth = Engine.Renderer.CurrentTarget.Size.X;
            float screenHeight = Engine.Renderer.CurrentTarget.Size.Y;
            float x = 2.0f * position.X / screenWidth - 1.0f;
            float y = 1.0f - 2.0f * position.Y / screenHeight;

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

        /// <inheritdoc />
        public override Vector2 WorldToScreen(Vector3 position)
        {
            Vector4 position4D = new Vector4(position, 1.0f); // Create a 4D vector with a W component of 1
            Vector4 clipPosition = Vector4.Transform(position4D, ViewMatrix * ProjectionMatrix); // Perform projection
            clipPosition.W = MathF.Abs(clipPosition.W); // We don't care if the point is in front of behind the camera.

            // Divide by W component to get normalized device coordinates (NDC) which go -1 to 1 with 0 in the middle
            var ndcPosition = clipPosition.ToVec3() / clipPosition.W;

            // Calculate the normalized device coordinates (-1 to 1)
            float screenWidth = Engine.Renderer.CurrentTarget.Size.X;
            float screenHeight = Engine.Renderer.CurrentTarget.Size.Y;
            float screenX = (ndcPosition.X + 1.0f) * 0.5f * screenWidth;
            float screenY = (1.0f - ndcPosition.Y) * 0.5f * screenHeight;

            return new Vector2(screenX, screenY);
        }

        /// <inheritdoc />
        public override Ray3D GetCameraMouseRay()
        {
            Vector3 dir = ScreenToWorld(Engine.Host.MousePosition);
            return new Ray3D(Position, dir - Position);
        }
    }
}