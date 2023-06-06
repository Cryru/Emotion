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

		// Movement
		public float MovementSpeed = 3;
		private Vector2 _lastMousePos;
		private Vector3 _yawPitchRoll = Vector3.Zero;
		private bool _held;
		private Vector2 _inputDirection;
		public float _inputDirectionZ;

		public Camera3D(Vector3 position, float zoom = 1) : base(position, zoom)
		{
			NearZ = 0.1f;
			FarZ = 10_000;
		}

		public override void Attach()
		{
			base.Attach();
			Engine.Host.OnKey.AddListener(CameraKeyHandler, KeyListenerType.Game);
		}

		public override void Detach()
		{
			base.Detach();
			Engine.Host.OnKey.RemoveListener(CameraKeyHandler);
		}

		public virtual bool CameraKeyHandler(Key key, KeyStatus status)
		{
			if (key == Key.MouseKeyLeft)
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
				_lastMousePos = mousePos;

				Engine.Renderer.Camera.RecreateViewMatrix();
			}

			if (_inputDirection != Vector2.Zero || _inputDirectionZ != 0)
			{
				Vector3 movementStraightBack = LookAt * -_inputDirection.Y;
				float len = movementStraightBack.Length();
				movementStraightBack.Z = 0;
				movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

				Vector3 movementUpDown = RenderComposer.Up * _inputDirectionZ;
				Vector3 movementSide = Vector3.Normalize(Vector3.Cross(RenderComposer.Up * -_inputDirection.X, LookAt));
				if (!float.IsNaN(movementStraightBack.X)) Position += movementStraightBack * MovementSpeed;
				if (!float.IsNaN(movementUpDown.X)) Position += movementUpDown * MovementSpeed;
				if (!float.IsNaN(movementSide.X)) Position += movementSide * MovementSpeed;
				// todo: interpolate.

				Engine.Renderer.Camera.RecreateViewMatrix();
			}
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

		public override Ray3D GetCameraMouseRay()
		{
			Vector3 dir = ScreenToWorld(Engine.Host.MousePosition);
			return new Ray3D(Position, dir - Position);
		}
	}
}