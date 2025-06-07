#nullable enable

using Emotion.Common.Input;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics.Camera;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.Editor2D;
using Emotion.WIPUpdates.One.TileMap;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One;

public enum MapEditorMode
{
    Off = 0,
    TwoDee = 2 << 0,
    ThreeDee = 2 << 1
}

public static partial class EngineEditor
{
    public static MapEditorMode MapEditorMode { get; private set; }

    public static event Action<MapEditorMode>? OnMapEditorModeChanged;

    public static void SetMapEditorMode(MapEditorMode mode)
    {
        MapEditorMode oldMode = MapEditorMode;
        MapEditorMode = mode;

        if (oldMode != MapEditorMode.Off)
            ExitMapEditor();
        if (mode != MapEditorMode.Off)
            EnterMapEditor();

        OnMapEditorModeChanged?.Invoke(MapEditorMode);
    }

    private static CameraBase? _cameraOutsideEditor;
    private static CameraBase? _editorCamera;
    private static Vector2 _gridTileSize;

    private static bool _enableDragWithMiddleMouse;
    private static Vector2? _draggingWithMiddleMouse;

    private static void EnterMapEditor()
    {
        Assert(MapEditorMode != MapEditorMode.Off);

        UIBaseWindow? gameUI = Engine.UI.GetWindowById("GameUIRoot");
        gameUI?.SetVisible(false);

        UIBaseWindow? bottomBarCurrent = EditorRoot.GetWindowById("BottomBar");
        bottomBarCurrent?.Close();

        _enableDragWithMiddleMouse = false;

        _cameraOutsideEditor = Engine.Renderer.Camera;
        if (MapEditorMode == MapEditorMode.ThreeDee)
        {
            Vector3 pos = _cameraOutsideEditor.Position;
            if (_cameraOutsideEditor is Camera2D && pos.Z == 0) pos.Z = 5; // Put away from grid.

            _editorCamera = new Camera3D(pos, 1f, KeyListenerType.EditorCamera)
            {
                LookAt = _cameraOutsideEditor.LookAt,
                DragKey = Key.MouseKeyMiddle,
                MovementSpeed = 1f,
                FarZ = 10_000
            };

            var bottomBar = new Editor2DBottomBar();
            EditorRoot.AddChild(bottomBar);

            GameMap? map = GetCurrentMap();
            if (map?.TerrainGrid != null)
                SetGridSize(map.TerrainGrid.TileSize);
            else
                SetGridSize(Vector2.Zero);
        }
        else if (MapEditorMode == MapEditorMode.TwoDee)
        {
            _editorCamera = new Camera2D(_cameraOutsideEditor.Position, 1f, KeyListenerType.EditorCamera)
            {
                MovementSpeed = 7, // todo: modify speed property in camera to make more sense (check its Update)
                ZoomAllowed = true
            };

            var bottomBar = new Editor2DBottomBar();
            EditorRoot.AddChild(bottomBar);

            _enableDragWithMiddleMouse = true;

            SetGridSize(Vector2.Zero);
        }
        AssertNotNull(_editorCamera);
        Engine.Renderer.Camera = _editorCamera;

        Engine.Host.OnKey.AddListener(InputHandler);
    }

    private static void ExitMapEditor()
    {
        UIBaseWindow? gameUI = Engine.UI.GetWindowById("GameUIRoot");
        gameUI?.SetVisible(true);

        UIBaseWindow? bottomBarCurrent = EditorRoot.GetWindowById("BottomBar");
        bottomBarCurrent?.Close();

        Engine.Renderer.Camera = _cameraOutsideEditor;
        _editorCamera = null;
        _cameraOutsideEditor = null;

        Engine.Host.OnKey.RemoveListener(InputHandler);
    }

    private static void UpdateMapEditor()
    {
        if (MapEditorMode == MapEditorMode.Off) return;

        if (_draggingWithMiddleMouse.HasValue)
        {
            CameraBase camera = Engine.Renderer.Camera;

            Vector2 mousePos = camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            Vector2 diff = _draggingWithMiddleMouse.Value - mousePos;
            _draggingWithMiddleMouse = mousePos;

            Engine.Renderer.Camera.Position2 += diff;
        }
    }

    private static void RenderMapEditor(RenderComposer c)
    {
        if (MapEditorMode == MapEditorMode.Off) return;

        Rectangle grid = new Primitives.Rectangle(0, 0, 10_000, 10_000);
        grid.Center = c.Camera.Position2;
        c.RenderGrid(grid.PositionZ(0.1f), grid.Size, _gridTileSize, Color.White.SetAlpha(125), c.Camera.Position2);
    }

    public static void SetGridSize(Vector2 size)
    {
        _gridTileSize = size;
    }

    private static bool InputHandler(Key key, KeyState state)
    {
        if (key == Key.MouseKeyMiddle)
        {
            if (_enableDragWithMiddleMouse && state == KeyState.Down)
            {
                CameraBase camera = Engine.Renderer.Camera;
                _draggingWithMiddleMouse = camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            }
            else
            {
                _draggingWithMiddleMouse = null;
            }
        }

        return true;
    }

    public static GameMap? GetCurrentMap()
    {
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap) return sceneWithMap.Map;
        return null;
    }
}
