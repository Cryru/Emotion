#nullable enable

using Emotion.Game.World;
using Emotion.Graphics.Camera;

namespace Emotion.Game.PremadeControllers.WorldOfWarcraft;

public class WoWCamera : Camera3D
{
    public float Distance = 20;

    public GameObject? Target;
    public Vector3 TargetOffset;

    private bool _mouseHeldLeft = false;
    private bool _mouseHeldRight = false;

    public WoWCamera(Vector3 position, float zoom = 1, KeyListenerType inputPriority = KeyListenerType.Game) : base(position, zoom, inputPriority)
    {
        _invertXMouseMovement = true;
        _cameraMovementSpeed = 0.15f;
    }

    public void SetTarget(GameObject? target, Vector3 targetOffset)
    {
        Target = target;
        TargetOffset = targetOffset;
        if (target != null)
        {
            LookAtPoint(target.Position3D);
            _yawRollPitch = new Vector3(180, 0, 45);
        }
    }

    protected override bool CameraKeyHandler(Key key, KeyState status)
    {
        if (key == Key.MouseKeyRight || key == Key.MouseKeyLeft)
        {
            if (key == Key.MouseKeyRight)
            {
                if (status == KeyState.Down)
                    _mouseHeldRight = true;

                if (status == KeyState.Up)
                    _mouseHeldRight = false;
            }

            if (key == Key.MouseKeyLeft)
            {
                if (status == KeyState.Down)
                    _mouseHeldLeft = true;

                if (status == KeyState.Up)
                    _mouseHeldLeft = false;
            }

            bool anyHeld = _mouseHeldLeft || _mouseHeldRight;
            if (anyHeld != _mouseKeyHeld)
            {
                if (anyHeld)
                    _lastMousePos = Engine.Host.MousePosition;
                _mouseKeyHeld = anyHeld;
            }

            return false;
        }

        if (key == Key.MouseWheel)
        {
            float zoomDir = status == KeyState.Up ? 1 : -1;
            Distance -= zoomDir;
            Distance = Maths.Clamp(Distance, 1, 30);

            return false;
        }

        return true;
    }

    public override void Update()
    {
        base.Update();

        if (Target != null)
        {
            var direction = new Vector3
            {
                X = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.X)),
                Y = MathF.Cos(Maths.DegreesToRadians(_yawRollPitch.Z)) * MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.X)),
                Z = MathF.Sin(Maths.DegreesToRadians(_yawRollPitch.Z))
            };
            direction = Vector3.Normalize(direction);

            Vector3 focusPosition = Target.Position3D + TargetOffset;

            Position = focusPosition + direction * Distance;
            _lookAt = Vector3.Normalize(focusPosition - Position);
            RecreateViewMatrix();
        }
    }
}
