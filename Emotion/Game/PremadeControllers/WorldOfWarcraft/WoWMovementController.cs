#nullable enable

using Emotion.Graphics.Camera;
using Emotion.Platform.Input;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;

namespace Emotion.Game.PremadeControllers.WorldOfWarcraft;

public class WoWMovementController
{
    public float WalkingSpeed = 0.5f; // Per millisecond

    private MapObject? _character;
    private string? _idleAnim;
    private string? _walkAnim;
    private string? _strafeLeftAnim;
    private string? _strafeRightAnim;
    private string? _walkBackAnim;

    private Vector2 _input;
    private bool _leftClickHeld;
    private bool _rightClickHeld;

    private WoWCamera _camera = new WoWCamera(Vector3.Zero);

    public void Attach()
    {
        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
        Engine.Renderer.Camera = _camera;
    }

    public void Dettach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
    }

    public void SetCharacter(MapObject obj, string idleAnim, string walkAnim, string strafeLeftAnim, string strafeRightAnim, string walkBackAnim)
    {
        _camera.SetTarget(obj, new Vector3(0, 0, -50));

        _character = obj;
        _idleAnim = idleAnim;
        _walkAnim = walkAnim;
        _strafeLeftAnim = strafeLeftAnim;
        _strafeRightAnim = strafeRightAnim;
        _walkBackAnim = walkBackAnim;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        if (map == null) return;

        Vector2 wasd = _input;

        if (_rightClickHeld && _leftClickHeld)
            wasd = new Vector2(0, -1);

        if (wasd != Vector2.Zero)
        {
            wasd = wasd.Perpendicular();
            wasd = -wasd;
            wasd = _character.RotateVectorToObjectFacing(wasd);
        }

        Vector2 forward = new Vector2(1, 0);
        forward = _character.RotateVectorToObjectFacing(forward);

        bool moved = wasd != Vector2.Zero;
        bool walkingBack = Vector2.Dot(wasd, forward) < 0;

        float movementSpeed = WalkingSpeed;
        if (walkingBack) movementSpeed /= 2f;

        _character.Position += (wasd * movementSpeed * dt).ToVec3();

        if (moved && map.TerrainGrid != null)
        {
            float height = map.TerrainGrid.GetHeightAt(_character.Position2D);
            _character.Z = height;
        }

        if (_rightClickHeld && _character is MapObjectMesh meshObj)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Vector3 lookAtPos = camera.LookAt;
            meshObj.RotateToFacePoint(_character.Position + lookAtPos);
        }

        if (_character is MapObjectMesh animatedObj)
        {
            string? animToSet = moved ? _walkAnim : _idleAnim;

            //if (wasd.X < 0) animToSet = _strafeLeftAnim;
            //if (wasd.X > 0) animToSet = _strafeRightAnim;
            if (walkingBack) animToSet = _walkBackAnim;

            if (animToSet != null && animToSet != animatedObj.GetCurrentAnimation())
                animatedObj.SetAnimation(animToSet);
        }
    }

    private bool KeyHandler(Key key, KeyState status)
    {
        Vector2 axis = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
        if (axis != Vector2.Zero)
        {
            if (status == KeyState.Up) axis = -axis;
            _input += axis;
            return false;
        }

        if (key == Key.MouseKeyRight)
        {
            _rightClickHeld = status == KeyState.Down;
        }

        if (key == Key.MouseKeyLeft)
        {
            _leftClickHeld = status == KeyState.Down;
        }

        return true;
    }
}
