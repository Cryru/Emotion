#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World.Editor;
using Emotion.Game.World.SceneControl;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Game.World3D.Editor;

public partial class World3DEditor : WorldBaseEditor
{
    public new Map3D? CurrentMap
    {
        get => (Map3D?) base.CurrentMap;
    }

    protected InfiniteGrid? _grid;

    public World3DEditor(IWorldAwareScene scene, Type mapType) : base(scene, mapType)
    {
    }

    protected override CameraBase GetEditorCamera()
    {
        return new Camera3D(Vector3.Zero, 1f, KeyListenerType.EditorCamera)
        {
            DragKey = Key.MouseKeyMiddle
        };
    }

    protected override bool InternalEditorInputHandler(Key key, KeyStatus status)
    {
        bool propagate = true;
        if (MoveGizmo != null) propagate = MoveGizmo.KeyHandler(key, status);
        if (!propagate) return false;

        return propagate;
    }

    protected override void EnterEditorInternal()
    {
        CreateEditorGrid();
    }

    protected override void ExitEditorInternal()
    {
    }

    protected override void UpdateInternal(float dt)
    {
        _grid?.Update(dt);
    }

    protected override void RenderInternal(RenderComposer c)
    {
        _grid?.Render(c);

        c.RenderLine(new Vector3(0, 0, 10), new Vector3(short.MaxValue, 0, 10), Color.Red, snapToPixel: false);
        c.RenderLine(new Vector3(0, 0, 10), new Vector3(0, short.MaxValue, 10), Color.Green, snapToPixel: false);
        c.RenderLine(new Vector3(0, 0, 10), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);
    }

    private void CreateEditorGrid()
    {
        if (_grid == null)
            Task.Run(() =>
            {
                var grid = new InfiniteGrid();
                grid.LoadAssetsAsync().Wait();
                grid.TileSize = 100;
                grid.Offset = new Vector2(grid.TileSize / 2f + 0.5f);
                grid.Tint = Color.Black.Clone().SetAlpha(125);
                _grid = grid;
            });
    }
}