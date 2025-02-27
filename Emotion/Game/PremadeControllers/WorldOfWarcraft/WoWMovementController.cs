#nullable enable

using Emotion.Game.Time;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;

namespace Emotion.Game.PremadeControllers.WorldOfWarcraft;

public class WoWMovementController
{
    public float WalkingSpeed = 0.7f; // Per millisecond

    private MapObject? _character;
    private string? _idleAnim;
    private string? _walkAnim;
    private string? _splitBodyBone;
    private string? _walkBackAnim;

    private Vector2 _input;
    private bool _leftClickHeld;
    private bool _rightClickHeld;

    private WoWCamera _camera = new WoWCamera(Vector3.Zero);

    private bool _jumping = false;
    private After _jumpTimer = new After(750);

    public void Attach()
    {
        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
        Engine.Renderer.Camera = _camera;
    }

    public void Dettach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
    }

    public void SetCharacter(MapObject obj, string idleAnim, string walkAnim, string walkBackAnim, string splitBodyBone)
    {
        _camera.SetTarget(obj, new Vector3(0, 0, 200));

        _character = obj;
        _idleAnim = idleAnim;
        _walkAnim = walkAnim;
        _walkBackAnim = walkBackAnim;
        _splitBodyBone = splitBodyBone;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        if (map == null) return;
        if (map.TerrainGrid == null) return;

        // Super cringe basic jump that doesn't do anything
        if (_jumping)
        {
            _jumpTimer.Update(dt);

            if (_jumpTimer.Progress < 0.5f)
                _character.Z += 0.4f * dt;
            else
                _character.Z -= 0.4f * dt;

            if (_jumpTimer.Finished)
            {
                _jumping = false;

                float height = map.TerrainGrid.GetHeightAt(_character.Position2D);
                _character.Z = height;
            }
        }
       
        Vector2 wasd = _input;
        if (_rightClickHeld && _leftClickHeld)
            wasd = new Vector2(0, -1);

        if (wasd != Vector2.Zero)
        {
            wasd = wasd.Perpendicular();
            wasd = -wasd;
            wasd = _character.RotateVectorToObjectFacing(wasd);
        }

        Vector2 forward = RenderComposer.Forward.ToVec2();
        forward = _character.RotateVectorToObjectFacing(forward);

        bool moved = wasd != Vector2.Zero;
        bool walkingBack = Vector2.Dot(wasd, forward) < 0;

        Vector2 right = RenderComposer.Right.ToVec2();
        right = _character.RotateVectorToObjectFacing(right);
        bool walkingRight = Vector2.Dot(wasd, right) > 0;
        bool walkingLeft = Vector2.Dot(wasd, -right) > 0;

        float movementSpeed = WalkingSpeed;
        if (walkingBack) movementSpeed /= 2f;

        _character.Position += (wasd * movementSpeed * dt).ToVec3();

        if (moved && !_jumping)
        {
            float height = map.TerrainGrid.GetHeightAt(_character.Position2D);
            _character.Z = height;
        }

        if (_character is MapObjectMesh meshObj)
        {
            if (_rightClickHeld)
            {
                CameraBase camera = Engine.Renderer.Camera;
                Vector3 lookAtPos = camera.LookAt;
                meshObj.RotateToFacePoint(_character.Position + lookAtPos);
            }
            else if (moved)
            {
                //animatedObj.RotateToFacePoint(_character.Position + wasd.ToVec3() * 5);
            }

            string? animToSet = moved ? _walkAnim : _idleAnim;

            //if (wasd.X < 0) animToSet = _strafeLeftAnim;
            //if (wasd.X > 0) animToSet = _strafeRightAnim;
            if (walkingBack) animToSet = _walkBackAnim;

            if (animToSet != null && animToSet != meshObj.GetCurrentAnimation())
                meshObj.SetAnimation(animToSet);
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

        if (key == Key.Space && status == KeyState.Down)
        {
            _jumping = true;
            _jumpTimer.Restart();
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
