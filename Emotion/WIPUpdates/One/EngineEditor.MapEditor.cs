#nullable enable

using Emotion.Game.World3D.Objects;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;
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
    private static InfiniteGrid? _grid;

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

        if (_grid == null)
        {
            var grid = new InfiniteGrid
            {
                TileSize = 0,
                Tint = Color.White.SetAlpha(125)
            };
            _grid = grid;
            Task.Run(grid.LoadAssetsAsync);
        }

        _cameraOutsideEditor = Engine.Renderer.Camera;
        if (MapEditorMode == MapEditorMode.ThreeDee)
        {
            Vector3 pos = _cameraOutsideEditor.Position;
            if (_cameraOutsideEditor is Camera2D && pos.Z == 0) pos.Z = 550; // Put away from grid.

            _editorCamera = new Camera3D(pos, 1f)
            {
                LookAt = _cameraOutsideEditor.LookAt,
                DragKey = Key.MouseKeyLeft,
                MovementSpeed = 10,
            };

            SetGridSize(100);
        }
        else if (MapEditorMode == MapEditorMode.TwoDee)
        {
            _editorCamera = new Camera2D(_cameraOutsideEditor.Position, 1f)
            {
                MovementSpeed = 7, // todo: modify speed property in camera to make more sense (check its Update)
                ZoomAllowed = true
            };

            Editor2DBottomBar bottomBar = new Editor2DBottomBar
            {
                Id = "BottomBar"
            };
            EditorRoot.AddChild(bottomBar);

            _enableDragWithMiddleMouse = true;

            SetGridSize(0);
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
        _grid?.Update(Engine.DeltaTime);

        if (_draggingWithMiddleMouse.HasValue)
        {
            CameraBase camera = Engine.Renderer.Camera;

            Vector2 mousePos = camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            Vector2 diff = _draggingWithMiddleMouse.Value - mousePos;
            _draggingWithMiddleMouse = mousePos;

            Engine.Renderer.Camera.Position2 += diff;

            mousePos = camera.ScreenToWorld(Engine.Host.MousePosition).ToVec2();
            _draggingWithMiddleMouse = mousePos;
        }
    }

    private static void RenderMapEditor(RenderComposer c)
    {
        if (MapEditorMode == MapEditorMode.Off) return;
        _grid?.Render(c);
    }

    public static void SetGridSize(float size)
    {
        if (_grid == null) return;
        _grid.TileSize = size;
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
