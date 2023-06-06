#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera
{
    /// <summary>
    /// 2D camera for non-pixel art games.
    /// Scales using the renderer "Scale" property.
    /// </summary>
    public class WASDMoveCamera2D : FloatScaleCamera2d
    {
	    public Vector2 Velocity { get; protected set; }
		public bool Fast { get; protected set; }

		/// <summary>
		/// How fast the camera will move per millisecond.
		/// </summary>
		public float Speed { get; set; } = 0.35f;

		private float _zoomDir = 0;

	    public WASDMoveCamera2D(Vector3 position, float zoom = 1) : base(position, zoom)
	    {
	    }

	    public virtual bool CameraKeyHandler(Key key, KeyStatus status)
	    {
		    Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
		    if (keyAxisPart != Vector2.Zero)
		    {
			    if (status == KeyStatus.Down)
				    Velocity += keyAxisPart;
			    else if (status == KeyStatus.Up)
				    Velocity -= keyAxisPart;

				return false;
		    }

		    if (key == Key.LeftShift || key == Key.RightShift)
		    {
			    Fast = status == KeyStatus.Down;

				return false;
		    }

		    if (key == Key.MouseWheel)
		    {
			    float zoomDir = status == KeyStatus.MouseWheelScrollUp ? 1 : -1;
			    _zoomDir = zoomDir;

			    return false;
		    }

			return true;
	    }

		public override void Update()
		{
			base.Update();

			var moveSpeedVector = new Vector2(Speed, Speed);
			if (Fast) moveSpeedVector = moveSpeedVector * 2f;
			Position2 += Velocity * moveSpeedVector * Engine.DeltaTime;

			if (_zoomDir != 0)
			{
				Vector2 mouseScreen = Engine.Host.MousePosition;
				Vector2 mouseWorld = ScreenToWorld(mouseScreen).ToVec2();

				float zoom = _zoomDir * (moveSpeedVector.X / 2f);
				Zoom = Maths.Clamp(Zoom + zoom, 0.1f, 4f);
				Vector2 mouseWorldAfterZoom = ScreenToWorld(mouseScreen).ToVec2();
				Position2 += mouseWorld - mouseWorldAfterZoom;

				_zoomDir = 0;
			}
		}
	}
}