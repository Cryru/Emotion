#nullable enable

using Emotion.Editor;
using Emotion.Game.World;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;

namespace Emotion.Game.PremadeControllers.Minecraft;

public class MinecraftMovementController
{
    public float WalkingSpeed = 0.005f; // Per millisecond

    public Vector3 TilePosOfCursor { get; protected set; } = Vector3.NaN;

    public Vector3 PlacementPosCursor { get; protected set; } = Vector3.NaN;

    public float CursorReach = 4f;

    private MapObject? _character;
    private Vector2 _input;

    private Camera3D _camera = new Camera3D(Vector3.Zero)
    {
        NearZ = 0.005f,
        FarZ = 200,
        FieldOfView = 90,
        DragKey = Key.World2 // Remove
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
        _camera.LookAt = Renderer.Forward;
        Engine.Input.SetMouseFirstPersonMode(true, nameof(MinecraftMovementController));

        EngineEditor.AddEditorVisualization(this, "View Character Bounds", (c) =>
        {
            if (_character == null) return;
            Cube characterCube = _character.BoundingCube;
            characterCube.RenderOutline(c, Color.PrettyOrange, 0.01f);
        });
        EngineEditor.AddEditorVisualization(this, "View Placement Cursor", (c) =>
        {
            if (_character == null) return;

            GameMap? map = _character.Map;
            VoxelMeshTerrainGrid? voxelTerrain = map?.TerrainGrid as VoxelMeshTerrainGrid;
            if (voxelTerrain == null) return;

            Vector3 placement = PlacementPosCursor;
            if (placement != Vector3.NaN)
            {
                Cube selectedCube = voxelTerrain.GetCubeOfTilePos(placement);
                selectedCube.RenderOutline(c, Color.White * 0.5f, 0.07f);
            }
        });
        EngineEditor.AddEditorVisualizationText(this, () =>
        {
            if (_character == null) return string.Empty;

            GameMap? map = _character.Map;
            VoxelMeshTerrainGrid? voxelTerrain = map?.TerrainGrid as VoxelMeshTerrainGrid;
            if (voxelTerrain == null) return string.Empty;

            var characterPos = _character.Position;
            var characterTilePos = voxelTerrain.GetTilePos3DOfWorldPos(characterPos);
            voxelTerrain.GetChunkAt(characterTilePos.ToVec2(), out Vector2 chunkCoord, out Vector2 tileRelativeCoord);
            return $"Player@ {characterTilePos} (Chunk: {chunkCoord}, Relative: {tileRelativeCoord})\nCursor@ {TilePosOfCursor}";
        });
    }

    public void Detach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
        Engine.Input.SetMouseFirstPersonMode(false, nameof(MinecraftMovementController));
        EngineEditor.RemoveEditorVisualizations(this);
    }

    public void SetCharacter(MapObject obj)
    {
        _character = obj;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        ITerrainGrid3D? terrain = map?.TerrainGrid;
        if (terrain == null) return;

        Vector2 myPos = _character.Position2D;
        Vector2 tile = terrain.GetTilePosOfWorldPos(myPos);
        if (!terrain.IsTileInBounds(tile))
        {
            TilePosOfCursor = Vector3.NaN;
            PlacementPosCursor = TilePosOfCursor;
            return;
        }

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
        wasd = forward * wasd.Y + right * wasd.X;

        float movementSpeed = WalkingSpeed;
        moveRequest += (wasd * movementSpeed * dt).ToVec3();

        // Check for collision
        Vector3 safeMovement = GetSafeMovementCharacter(moveRequest);

        // Check if in the air
        if (moveRequest.Z < 0 || moveRequest.Z == 0) // Moving down
        {
            _inAir = !Maths.Approximately(safeMovement.Z, 0, Maths.EPSILON); // Hit ground
            if (!_inAir)
                _velocityZ = 0; // Kill velocity.
        }
        else if (moveRequest.Z > 0) // Moving up (jumping)
        {
            bool hitCeiling = Maths.Approximately(safeMovement.Z, 0, Maths.EPSILON);
            if (hitCeiling)
                _velocityZ = 0; // Kill upwards velocity.
        }

        _character.Position += safeMovement;
        if (_character.Z < 0) _character.Z = 0;

        Cube playerCube = _character.BoundingCube;
        _camera.Position = playerCube.Origin + new Vector3(0, 0, playerCube.HalfExtents.Z - 0.1f);

        // Find cube pointing at
        bool set = false;
        VoxelMeshTerrainGrid? voxelTerrain = map?.TerrainGrid as VoxelMeshTerrainGrid;
        if (voxelTerrain != null)
        {
            Ray3D mouseRay = _camera.GetCameraMouseRay();
            if (voxelTerrain.CollideRay(mouseRay, out Vector3 collision, out Vector3 surfaceNormal) && Vector3.Distance(_character.Position, collision) < CursorReach)
            {
                TilePosOfCursor = voxelTerrain.GetTilePos3DOfWorldPos(collision + mouseRay.Direction * 0.01f);
                PlacementPosCursor = voxelTerrain.GetTilePos3DOfWorldPos(collision + surfaceNormal * 0.01f);
                set = true;
            }
        }
        if (!set)
        {
            TilePosOfCursor = Vector3.NaN;
            PlacementPosCursor = TilePosOfCursor;
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
