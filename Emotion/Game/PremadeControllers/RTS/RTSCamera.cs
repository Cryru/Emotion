#nullable enable

using Emotion.Graphics.Camera;

namespace Emotion.Game.PremadeControllers.RTS;

public class RTSCamera : Camera3D
{
    public float RotateSpeed = 1f;

    private Vector3 _lookAtPoint;
    private float _angle;
    private float _inputRotate;

    public RTSCamera(Vector3 position, float angle = 40, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
    {
        DragKey = Key.None;

        _invertXMouseMovement = true;
        _angle = angle;
        MovementSpeed = 0.5f;
        InitializeCameraAngle();
    }

    public override void Update()
    {
        base.Update();
    }

    private void InitializeCameraAngle()
    {
        // The angle between lookat and position.
        float angleRadians = Maths.DegreesToRadians(_angle);

        Vector3 oldDiff = LookAt - Position;
        float angleXY = MathF.Atan2(oldDiff.Y, oldDiff.X);
        float cosAngleXY = MathF.Cos(angleXY);
        float sinAngleXY = MathF.Sin(angleXY);

        float distanceZ = Math.Abs(oldDiff.Z);
        float distanceX = MathF.Tan(angleRadians) * distanceZ;
        float x = MathF.Sqrt(distanceX * distanceX + distanceZ * distanceZ) * MathF.Sin(angleRadians);
        float y = 0.0f;

        _lookAtPoint = Position + new Vector3(x * cosAngleXY - y * sinAngleXY, x * sinAngleXY + y * cosAngleXY, -Z);
        LookAtPoint(_lookAtPoint);
    }

    protected override bool CameraKeyHandler(Key key, KeyState state)
    {
        if (key == Key.Q || key == Key.E)
        {
            int dir = state == KeyState.Down ? 1 : -1;
            int keyDir = key == Key.Q ? 1 : -1;
            _inputRotate += dir * keyDir;
            return false;
        }

        return base.CameraKeyHandler(key, state);
    }
}
