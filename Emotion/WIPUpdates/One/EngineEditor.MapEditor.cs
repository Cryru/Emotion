using Emotion.Game.World3D.Objects;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;
using Emotion.WIPUpdates.One.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One;

#nullable enable
public enum MapEditorMode
{
    Off,
    TwoDee,
    ThreeDee
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

    private static void EnterMapEditor()
    {
        Assert(MapEditorMode != MapEditorMode.Off);

        UI.UIBaseWindow? gameUI = Engine.UI.GetWindowById("GameUIRoot");
        gameUI?.SetVisible(false);

        if (_grid == null)
            Task.Run(async () =>
            {
                var grid = new InfiniteGrid();
                await grid.LoadAssetsAsync();
                grid.TileSize = 100;
                grid.Offset = new Vector2(grid.TileSize / 2f + 0.5f);
                grid.Tint = Color.White.SetAlpha(125);
                _grid = grid;
            });

        _cameraOutsideEditor = Engine.Renderer.Camera;
        if (MapEditorMode == MapEditorMode.ThreeDee)
        {
            Vector3 pos = _cameraOutsideEditor.Position;
            if (_cameraOutsideEditor is Camera2D && pos.Z == 0) pos.Z = 550; // Put away from grid.

            _editorCamera = new Camera3D(pos, 1f)
            {
                LookAt = _cameraOutsideEditor.LookAt,
                DragKey = Key.MouseKeyLeft,
                MovementSpeed = 10
            };
        }
        else if (MapEditorMode == MapEditorMode.TwoDee)
        {
            _editorCamera = new Camera2D(_cameraOutsideEditor.Position, 1f)
            {
                LookAt = _cameraOutsideEditor.LookAt,
                MovementSpeed = 10
            };
        }
        AssertNotNull(_editorCamera);
        Engine.Renderer.Camera = _editorCamera;
    }

    private static void ExitMapEditor()
    {
        UI.UIBaseWindow? gameUI = Engine.UI.GetWindowById("GameUIRoot");
        gameUI?.SetVisible(true);

        Engine.Renderer.Camera = _cameraOutsideEditor;
        _editorCamera = null;
        _cameraOutsideEditor = null;
    }

    private static void UpdateMapEditor()
    {
        if (MapEditorMode == MapEditorMode.Off) return;
        _grid?.Update(Engine.DeltaTime);
    }

    private static void RenderMapEditor(RenderComposer c)
    {
        if (MapEditorMode == MapEditorMode.Off) return;
        _grid?.Render(c);
    }
}
