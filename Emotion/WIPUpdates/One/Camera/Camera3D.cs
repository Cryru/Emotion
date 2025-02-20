#region Using

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
        private bool _held;
        private Vector2 _inputDirection;
        private float _inputDirectionZ;

        protected Vector3 _yawRollPitch = Vector3.Zero;

        public Camera3D(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
        {
        }

        protected override void LookAtChanged(Vector3 _, Vector3 lookAt)
        {
            // Handle the looking straight up/down locks.
            // This happens when transitioning between 2D and 3D
            if (lookAt == RenderComposer.Up)
                lookAt = new Vector3(0, -0.0174523834f, 0.9998477f);
            else if (lookAt == -RenderComposer.Up)
                lookAt = new Vector3(0, -0.0174523834f, -0.9998477f);

            // Init rotation for mouse turning.
            float pitch = MathF.Asin(lookAt.Z);
            float yaw;
            if (lookAt.Z < 0)
                yaw = MathF.PI + MathF.Atan2(-lookAt.Y, -lookAt.X);
            else
                yaw = MathF.Atan2(lookAt.Y, lookAt.X);

            // Prevent look at facing towards or out of RenderComposer.Up (gimbal lock)
            pitch = Maths.Clamp(pitch, Maths.DegreesToRadians(-89), Maths.DegreesToRadians(89));
            _yawRollPitch = new Vector3(Maths.RadiansToDegrees(yaw), 0, Maths.RadiansToDegrees(pitch));

#if DEBUG
            var direction = new Vector3
            {
                X = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.X)),
                Y = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.X)),
                Z = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.Z))
            };
            direction = Vector3.Normalize(direction);
            Engine.Log.Trace($"3D camera look at reconstruction diff: {(direction - lookAt).Length()}", "Camera3D");
#endif
        }

        /// <inheritdoc />
        public override void RecreateViewMatrix()
        {
            Vector3 pos = Position;
            Vector3 lookAt = _lookAt;

            Vector3 worldUp = GetCameraWorldUp();

            Matrix4x4 unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos + lookAt, worldUp);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1), pos) * unscaled;
        }

        /// <inheritdoc />
        public override void RecreateProjectionMatrix()
        {
            RenderComposer renderer = Engine.Renderer;
            float aspectRatio = renderer.CurrentTarget.Size.X / renderer.CurrentTarget.Size.Y;
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(Maths.DegreesToRadians(_fieldOfView), aspectRatio, Maths.Clamp(NearZ, 0.1f, FarZ), FarZ);
        }

        protected override bool CameraKeyHandler(Key key, KeyState status)
        {
            bool dragKey = key == DragKey;
            if (DragKey != Key.MouseKeyLeft) // Secondary way of moving camera in editor.
            {
                bool leftClickWithControlOrLeftClickLetGo = key == Key.MouseKeyLeft && (Engine.Host.IsCtrlModifierHeld() || status == KeyState.Up);
                dragKey = dragKey || leftClickWithControlOrLeftClickLetGo;
            }

            if (dragKey)
            {
                if (status == KeyState.Down)
                {
                    _lastMousePos = Engine.Host.MousePosition;
                    _held = true;
                    return false;
                }

                if (status == KeyState.Up)
                {
                    _held = false;
                    return false;
                }
            }

            Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
            if (keyAxisPart != Vector2.Zero)
            {
                if (status == KeyState.Down)
                    _inputDirection += keyAxisPart;
                else if (status == KeyState.Up)
                    _inputDirection -= keyAxisPart;
                return false;
            }

            if (key == Key.Space || key == Key.LeftShift)
            {
                if (status == KeyState.Down)
                {
                    if (key == Key.Space)
                        _inputDirectionZ += 1;
                    else
                        _inputDirectionZ -= 1;
                }
                else if (status == KeyState.Up)
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
                float xOffset = -(mousePos.X - _lastMousePos.X) * 0.1f;
                float yOffset = (mousePos.Y - _lastMousePos.Y) * 0.1f;

                _yawRollPitch.X += xOffset;
                _yawRollPitch.Z += yOffset;
                _yawRollPitch.Z = Maths.Clamp(_yawRollPitch.Z, -89, 89); // Prevent flip.
                var direction = new Vector3
                {
                    X = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.X)),
                    Y = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.X)),
                    Z = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.Z))
                };
                direction = Vector3.Normalize(direction);
                _lookAt = direction;
                _lastMousePos = mousePos;

                RecreateViewMatrix();
            }

            if (_inputDirection != Vector2.Zero || _inputDirectionZ != 0)
            {
                Vector3 worldUp = GetCameraWorldUp();

                Vector3 forwardMoveVector = _lookAt.X == 0 && _lookAt.Y == 0 ? worldUp : _lookAt;
                Vector3 movementStraightBack = forwardMoveVector * -_inputDirection.Y;
                float len = movementStraightBack.Length();
                movementStraightBack.Z = 0;
                movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

                Vector3 movementUpDown = RenderComposer.Up * _inputDirectionZ;
                Vector3 movementSide = -Vector3.Normalize(Vector3.Cross(_lookAt, worldUp)) * _inputDirection.X;
                if (!float.IsNaN(movementStraightBack.X)) Position += movementStraightBack * MovementSpeed;
                if (!float.IsNaN(movementUpDown.X)) Position += movementUpDown * MovementSpeed;
                if (!float.IsNaN(movementSide.X)) Position += movementSide * MovementSpeed;
                // todo: interpolate.

                RecreateViewMatrix();
            }
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
            rayEye.Z = 1f;
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

        public override Rectangle GetCameraView2D()
        {
            // Get frustum
            Span<Vector3> frustumCorners = stackalloc Vector3[8];
            GetCameraView3D(frustumCorners);

            Span<Vector3> sideA = stackalloc Vector3[4];
            Span<Vector3> sideB = stackalloc Vector3[4];
            GetCameraFrustumSidePlanes(frustumCorners, sideA, sideB);

            //Engine.Renderer.RenderVertices(sideA);
            //Engine.Renderer.RenderVertices(sideB);

            Vector2 minIntersection = new Vector2(float.MaxValue);
            Vector2 maxIntersection = new Vector2(float.MinValue);
            for (int i = 0; i < 3; i++)
            {
                Add2DPlaneIntersection(sideA[i], sideA[i + 1], ref minIntersection, ref maxIntersection);
                Add2DPlaneIntersection(sideB[i], sideB[i + 1], ref minIntersection, ref maxIntersection);
            }
            Add2DPlaneIntersection(sideA[3], sideA[0], ref minIntersection, ref maxIntersection);
            Add2DPlaneIntersection(sideB[3], sideB[0], ref minIntersection, ref maxIntersection);

            if (minIntersection.X == float.MaxValue || maxIntersection.X == float.MinValue)
                return Rectangle.Empty;

            return Rectangle.FromMinMaxPoints(minIntersection, maxIntersection);
        }

        private static void Add2DPlaneIntersection(Vector3 start, Vector3 end, ref Vector2 minIntersection, ref Vector2 maxIntersection)
        {
            if ((start.Z <= 0 || end.Z >= 0) && (start.Z >= 0 || end.Z <= 0)) return;

            float t = -start.Z / (end.Z - start.Z);
            Vector3 intersection = Vector3.Lerp(start, end, t);

            minIntersection = Vector2.Min(intersection.ToVec2(), minIntersection);
            maxIntersection = Vector2.Max(intersection.ToVec2(), maxIntersection);
        }
    }
}