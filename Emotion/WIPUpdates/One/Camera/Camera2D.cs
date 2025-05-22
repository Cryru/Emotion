#nullable enable

using Emotion.Common.Input;
using Emotion.Graphics.Camera;
using Emotion.Utility;
using Emotion.WIPUpdates.TimeUpdate;

namespace Emotion.WIPUpdates.One.Camera;

public class Camera2D : CameraBase
{
    // Movement
    public float MovementSpeed = 3;
    protected Vector2 _inputDirection;

    public bool ZoomAllowed;
    public float MinZoom = 0.01f;
    public float MaxZoom = 4f;

    protected float _zoomDir;

    public Camera2D(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
    {

    }

    /// <inheritdoc />
    public override void RecreateViewMatrix()
    {
        var iX = X;
        var iY = Y;

        Vector2 targetSize = Engine.Configuration.RenderSize;
        Vector2 currentSize = Engine.Renderer.DrawBuffer.Size;

        // Transform the position from the center position to the offset position.
        Vector3 posOffset = new Vector3(iX, iY, 0) - new Vector3(targetSize, 0) / 2;

        // Get the scale relative to the zoom.
        float scale = Engine.Renderer.Scale * Zoom;
        CalculatedScale = scale;

        // Find the camera margin and scale from the center.
        // As the current size expands more of the world will come into view until the integer scale changes at which point everything will be resized.
        Vector2 margin = (currentSize - targetSize) / 2;
        Vector3 pos = posOffset - new Vector3(margin.X, -margin.Y, 0) + new Vector3(0, targetSize.Y, 0);

        Vector3 lookAt = _lookAt;
        Vector3 worldUp = GetCameraWorldUp();
        Matrix4x4 unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos + lookAt, worldUp);

        // We need to flip the Y scale since OpenGL expects a natural origin at the bottom-left corner but we
        // have set up our projection to be top-left. This is also the reason we add targetSize.Y above
        // and flip the Y margin.
        ViewMatrix = Matrix4x4.CreateScale(new Vector3(scale, -scale, 1), new Vector3(iX, iY, 0)) * unscaled;
    }

    /// <inheritdoc />
    public override void RecreateProjectionMatrix()
    {
        ProjectionMatrix = GetDefault2DProjection(NearZ, FarZ);
    }

    protected override bool CameraKeyHandler(Key key, KeyState status)
    {
        Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
        if (keyAxisPart != Vector2.Zero)
        {
            if (status == KeyState.Down)
                _inputDirection += keyAxisPart;
            else if (status == KeyState.Up)
                _inputDirection -= keyAxisPart;

            return false;
        }

        //if (key == Key.LeftShift || key == Key.RightShift)
        //{
        //    Fast = status == KeyStatus.Down;

        //    return false;
        //}

        if (ZoomAllowed && key == Key.MouseWheel)
        {
            float zoomDir = status == KeyState.Up ? 1 : -1;
            _zoomDir = zoomDir;

            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override void Update()
    {
        if (_inputDirection != Vector2.Zero)
        {
            float movementSpeed = (MovementSpeed / 10f) * Engine.DeltaTime;
            float movementSpeedZoomScale = ((MovementSpeed / Zoom) / 10f) * Engine.DeltaTime;
            movementSpeed = Interpolation.SmoothLerp(movementSpeed, movementSpeedZoomScale, 10, Engine.DeltaTime);

            Vector3 worldUp = GetCameraWorldUp();

            Vector3 movementStraightBack = worldUp * -_inputDirection.Y;
            float len = movementStraightBack.Length();
            movementStraightBack.Z = 0;
            movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

            Vector3 movementSide = Vector3.Normalize(Vector3.Cross(worldUp, _lookAt)) * _inputDirection.X;
            if (!float.IsNaN(movementStraightBack.X)) Position += movementStraightBack * movementSpeed;
            if (!float.IsNaN(movementSide.X)) Position += movementSide * movementSpeed;
            // todo: interpolate.

            RecreateViewMatrix();
        }

        if (_zoomDir != 0)
        {
            float movementSpeed = (MovementSpeed / 10f) * Engine.DeltaTime;

            Vector2 mouseScreen = GetZoomMousePos();
            Vector2 mouseWorld = ScreenToWorld(mouseScreen).ToVec2();

            // Zoom is perceived as logarithmic-ish
            float zoomFactorIncrease = 1.0f + ((movementSpeed / 100f) * MathF.Abs(_zoomDir));

            if (_zoomDir < 0)
                Zoom = Zoom / zoomFactorIncrease;
            else
                Zoom = Zoom * zoomFactorIncrease;

            Zoom = Maths.Clamp(Zoom, MinZoom, MaxZoom);
            Zoom = MathF.Round(Zoom, 3);

            Vector2 mouseWorldAfterZoom = ScreenToWorld(mouseScreen).ToVec2();
            Position2 += mouseWorld - mouseWorldAfterZoom;

            _zoomDir = 0;
        }
    }

    /// <inheritdoc />
    public override Vector3 ScreenToWorld(Vector2 position)
    {
        return Vector3.Transform(new Vector3(position, 0f), ViewMatrix.Inverted());
    }

    /// <inheritdoc />
    public override Vector2 WorldToScreen(Vector3 position)
    {
        return Vector2.Transform(position.ToVec2(), ViewMatrix);
    }

    /// <inheritdoc />
    public override Ray3D GetCameraMouseRay()
    {
        Vector3 dir = ScreenToWorld(Engine.Host.MousePosition);
        dir.Z = ushort.MaxValue;
        return new Ray3D(dir, LookAt);
    }

    protected virtual Vector2 GetZoomMousePos()
    {
        return Engine.Host.MousePosition;
    }
}
