using Emotion.Graphics.Camera;
using Emotion.Platform.Input;

namespace Emotion.WIPUpdates.One.Camera;

public class Camera2D : CameraBase
{
    // Movement
    public float MovementSpeed = 3;
    private Vector2 _inputDirection;

    public Camera2D(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
    {
    }

    /// <inheritdoc />
    public override void RecreateViewMatrix()
    {
        var iX = (int)X;
        var iY = (int)Y;

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
        Vector3 pos = posOffset - new Vector3((int)margin.X, (int)margin.Y, 0);
        var unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos - LookAt, -RenderComposer.Up2D); // Not sure why the look at and up have to be minused
        ViewMatrix = Matrix4x4.CreateScale(new Vector3(scale, scale, 1), new Vector3(iX, iY, 0)) * unscaled;
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

        //if (key == Key.MouseWheel)
        //{
        //    float zoomDir = status == KeyStatus.MouseWheelScrollUp ? 1 : -1;
        //    _zoomDir = zoomDir;

        //    return false;
        //}

        return true;
    }

    /// <inheritdoc />
    public override void Update()
    {
        if (_inputDirection != Vector2.Zero)
        {
            Vector3 movementStraightBack = RenderComposer.Up2D * -_inputDirection.Y;
            float len = movementStraightBack.Length();
            movementStraightBack.Z = 0;
            movementStraightBack = Vector3.Normalize(movementStraightBack) * len;

            Vector3 movementSide = Vector3.Normalize(Vector3.Cross(RenderComposer.Up2D, LookAt)) * _inputDirection.X;
            if (!float.IsNaN(movementStraightBack.X)) Position += movementStraightBack * MovementSpeed;
            if (!float.IsNaN(movementSide.X)) Position += movementSide * MovementSpeed;
            // todo: interpolate.

            RecreateViewMatrix();
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
}
