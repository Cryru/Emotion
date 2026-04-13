#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Game.World.Terrain;
using Emotion.Graphics.Camera;

namespace Emotion.Game.PremadeControllers.WorldOfWarcraft;

public class WoWMovementController : IGameObjectComponent, IUpdateableComponent
{
    public float WalkingSpeed = 0.007f; // Per millisecond
    public WoWCamera Camera = new WoWCamera(Vector3.Zero);

    private GameObject? _character;
    private string? _idleAnim;
    private string? _walkAnim;
    private string? _strafeLegBone;
    private string? _strafeTorsoBone;
    private string? _walkBackAnim;

    private Vector2 _input;
    private bool _leftClickHeld;
    private bool _rightClickHeld;

    private bool _inAir = true;
    private bool _jumpHeld = false;
    private float _velocityZ;

    private float _jumpVelocity = 0.0126f;
    private float _gravityPerMs = -0.0000426f;
    private float _airTimeMaxPosiveVelMod = 0.6f;

    public WoWMovementController(string idleAnim, string walkAnim, string walkBackAnim, string strafeLegBone, string strafeTorsoBone)
    {
        _idleAnim = idleAnim;
        _walkAnim = walkAnim;
        _walkBackAnim = walkBackAnim;
        _strafeLegBone = strafeLegBone;
        _strafeTorsoBone = strafeTorsoBone;
    }

    public void Attach()
    {
        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
        Engine.Renderer.Camera = Camera;
    }

    public void Dettach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
    }

    public void SetCharacter(GameObject obj)
    {
        Camera.SetTarget(obj, new Vector3(0, 0, 2));
        _character = obj;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        if (map == null) return;

        TerrainMeshGridNew? terrain = map.GetFirstGridOfType<TerrainMeshGridNew>();
        if (terrain == null) return;

        Vector3 charPos = _character.Position3D;
        Vector2 charPos2D = charPos.ToVec2();

        // Add gravity
        _velocityZ += _gravityPerMs * dt;

        if (_jumpHeld && !_inAir)
        {
            _velocityZ += _jumpVelocity;
            _inAir = true;
        }

        // Gather movement
        Vector3 moveRequest = Vector3.Zero;
        moveRequest.Z += MathF.Min(_velocityZ, _jumpVelocity * _airTimeMaxPosiveVelMod) * dt;

        // Input
        Vector2 wasd = _input;
        if (_rightClickHeld && _leftClickHeld)
            wasd = new Vector2(0, -1);

        if (wasd != Vector2.Zero)
        {
            wasd = wasd.Perpendicular();
            wasd = -wasd;
            wasd = _character.RotateVectorToObjectFacing(wasd);
        }

        // Move in the look direction
        Vector2 forward = Renderer.Forward.ToVec2();
        forward = _character.RotateVectorToObjectFacing(forward);

        bool moved = wasd != Vector2.Zero;
        bool walkingBack = Vector2.Dot(wasd, forward) < 0;

        Vector2 right = Renderer.Right.ToVec2();
        right = _character.RotateVectorToObjectFacing(right);
        bool walkingRight = Vector2.Dot(wasd, right) > 0;
        bool walkingLeft = Vector2.Dot(wasd, -right) > 0;
        float walkingLeftRight = Vector2.Dot(wasd, right);

        float movementSpeed = WalkingSpeed;
        if (walkingBack) movementSpeed /= 2f;

        // Prevent adding 1 to both X and Y
        if (wasd != Vector2.Zero)
            wasd = Vector2.Normalize(wasd);
        moveRequest += (wasd * movementSpeed * dt).ToVec3();

        // Check for collision
        if (moveRequest.Z != 0 && (_inAir || moved))
        {
            float height = terrain.GetHeightAt(charPos2D);
            if (charPos.Z + moveRequest.Z < height)
            {
                moveRequest.Z = 0;
                _inAir = false;
            }
        }

        // Check if going into invalid chunk.
        Vector2 tile = terrain.GetTilePosOfWorldPos(charPos2D + moveRequest.ToVec2());
        TerrainChunk? chunk = terrain.GetChunkAt(tile, out Vector2 _);
        if (chunk == null || !chunk.CanBeSimulated)
        {
            moveRequest.X = 0;
            moveRequest.Y = 0;
        }

        // Add move
        Vector3 newPos = charPos + moveRequest;

        // Move along the terrain height
        if (!_inAir)
        {
            _velocityZ = 0;

            float height = terrain.GetHeightAt(newPos.ToVec2());
            newPos.Z = height;
        }

        _character.Position3D = newPos;

        // Update look
        if (_rightClickHeld)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Vector3 lookAtPos = camera.LookAt;
            _character.RotateToFacePoint(newPos + lookAtPos);
        }

        string? animToSet = moved ? _walkAnim : _idleAnim;

        //if (wasd.X < 0) animToSet = _strafeLeftAnim;
        //if (wasd.X > 0) animToSet = _strafeRightAnim;
        if (walkingBack) animToSet = _walkBackAnim;

        _character.GetComponent<MeshComponent>(out MeshComponent? meshComponent);
        if (meshComponent != null)
        {
            if (animToSet != null && animToSet != meshComponent.GetCurrentAnimation())
            {
                meshComponent.SetAnimation(animToSet, 0, true, 250); // Crossfade - maybe should be in some kind of animation graph?
            }

            // The logic here is very controversial xd
            float strafeAngle = Maths.DegreesToRadians(50.0f);
            float leanAngle = Maths.DegreesToRadians(10.0f);

            float walkingLeftRightTilt = walkingLeftRight;
            if (walkingBack)
            {
                walkingLeftRightTilt *= -1;
                strafeAngle *= 0.5f;
                leanAngle *= 0.5f;
            }

            if (_strafeLegBone != null)
            {
                meshComponent.RenderState.SetCustomTransformForJoint(
                    _strafeLegBone,
                    Matrix4x4.CreateRotationY(strafeAngle * walkingLeftRightTilt),
                    200
               );
            }
            if (_strafeTorsoBone != null)
            {
                meshComponent.RenderState.SetCustomTransformForJoint(_strafeTorsoBone,
                    Matrix4x4.CreateRotationX(leanAngle * walkingLeftRightTilt) *
                    Matrix4x4.CreateRotationY(-strafeAngle * walkingLeftRightTilt),
                    200
                );
            }
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

        if (key == Key.Space)
        {
            _jumpHeld = status == KeyState.Down;
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

    #region Component API

    public void Done(GameObject obj)
    {
        Dettach();
    }

    public IRoutineWaiter? Init(GameObject obj)
    {
        SetCharacter(obj);
        Attach();
        return null;
    }

    #endregion
}
