using Emotion.Common.Input;
using Emotion.Graphics.Camera;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;

namespace Game.Controller;

public class MinecraftMovementController
{
    public float WalkingSpeed = 0.005f; // Per millisecond

    public Vector3 TilePosOfCursor { get; protected set; } = Vector3.NaN;

    private MapObject? _character;
    private Vector2 _input;

    private Camera3D _camera = new Camera3D(Vector3.Zero)
    {
        NearZ = 0.005f,
        FarZ = 200,
        FieldOfView = 90
    };

    private bool _inAir = true;
    private bool _jumpHeld = false;
    private float _velocityZ;

    private float _jumpVelocity = 0.0126000009f;
    private float _gravityPerMs = -4.26666629E-05f;
    private float _airTimeMaxPosiveVelMod = 0.6f;

    public void Attach()
    {
        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
        Engine.Renderer.Camera = _camera;
        _camera.LookAt = RenderComposer.Forward;
        Engine.Input.SetMouseFirstPersonMode(true, nameof(MinecraftMovementController));
    }

    public void Dettach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
        Engine.Input.SetMouseFirstPersonMode(false, nameof(MinecraftMovementController));
    }

    public void SetCharacter(MapObject obj)
    {
        _character = obj;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        if (map == null) return;
        if (map.TerrainGrid == null) return;

        VoxelMeshTerrainGrid? voxelTerrain = map.TerrainGrid as VoxelMeshTerrainGrid;
        if (voxelTerrain == null) return;

        // Add gravity
        _velocityZ += _gravityPerMs * dt;

        // Try jumping if held
        if (_jumpHeld && !_inAir)
        {
            _velocityZ += _jumpVelocity;
            _inAir = true;
        }

        // Gather movement
        Vector3 moveRequest = Vector3.Zero;
        moveRequest.Z += MathF.Min(_velocityZ, _jumpVelocity * _airTimeMaxPosiveVelMod) * dt;

        // Add input
        Vector2 wasd = _input;
        if (wasd != Vector2.Zero)
            wasd = -wasd;

        // Move in the look direction.
        Vector2 forward = _camera.LookAt.ToVec2();
        forward = forward.SafeNormalize();
        Vector2 right = forward.Perpendicular();
        wasd = (forward * wasd.Y) + (right * wasd.X);

        float movementSpeed = WalkingSpeed;
        moveRequest += (wasd * movementSpeed * dt).ToVec3();

        // Check for collision
        Vector3 safeMovement = GetSafeMovementCharacter(moveRequest);

        // Check if in the air
        if (moveRequest.Z != 0)
            _inAir = !Maths.Approximately(safeMovement.Z, 0, Maths.EPSILON);
        if (!_inAir && _velocityZ < 0)
            _velocityZ = 0;
        
        _character.Position += safeMovement;
        if (_character.Z < 0) _character.Z = 0;
        _camera.Position = _character.Position + new Vector3(0, 0, voxelTerrain.TileSize3D.Z * 1.5f);

        // Find cube pointing at
        Ray3D mouseRay = _camera.GetCameraMouseRay();
        if (voxelTerrain.CollideRay(mouseRay, out Vector3 collision))
            TilePosOfCursor = voxelTerrain.GetTilePos3DOfWorldPos(collision + mouseRay.Direction * 0.01f);
        else
            TilePosOfCursor = Vector3.NaN;
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

        return true;
    }

    private Vector3 GetSafeMovementCharacter(Vector3 moveAmount)
    {
        if (moveAmount == Vector3.Zero) return Vector3.Zero;
        if (_character == null) return Vector3.Zero;

        // Dont allow movement below 0
        if (_character.Z == 0 && moveAmount.Z < 0)
            moveAmount.Z = 0;

        GameMap? map = _character.Map;
        if (map == null) return Vector3.Zero;

        if (_character is not MapObjectMesh meshObj) return Vector3.Zero;

        Cube moverCube = meshObj.BoundingCube;
        return map.SweepCube(moverCube, moveAmount, meshObj);
    }
}
