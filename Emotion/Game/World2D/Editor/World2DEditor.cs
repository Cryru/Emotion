#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World.SceneControl;
using Emotion.Game.World2D.Tile;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor : WorldBaseEditor
{
    public new Map2D? CurrentMap
    {
        get => (Map2D?) base.CurrentMap;
    }

    public World2DEditor(IWorldAwareScene scene, Type mapType) : base(scene, mapType)
    {
    }

    protected override CameraBase GetEditorCamera()
    {
        //return new WASDMoveCamera2D(Vector3.Zero);
        Assert(false);
        return null;
    }

    protected override bool InternalEditorInputHandler(Key key, KeyStatus status)
    {
        bool propagate = TileEditorInputHandler(key, status);
        if (!propagate) return false;
        return true;
    }

    protected override void EnterEditorInternal()
    {
        InitializeTileEditor();
    }

    protected override void ExitEditorInternal()
    {
    }

    protected override void UpdateInternal(float dt)
    {
        UpdateTileEditor();
    }

    protected override void RenderInternal(RenderComposer c)
    {
        c.SetUseViewMatrix(true);
        c.SetDepthTest(true);
        c.SetAlphaBlend(true);

        Map2D? map = CurrentMap;

        // Render invisible tile layers
        // todo: move to tile editor
        if (map?.Tiles != null)
        {
            Rectangle clipRect = c.Camera.GetCameraFrustum();
            for (var i = 0; i < map.Tiles.Layers.Count; i++)
            {
                Map2DTileMapLayer layer = map.Tiles.Layers[i];
                if (!layer.Visible) map.Tiles.RenderLayer(c, i, clipRect);
            }

            c.ClearDepth();
        }

        RenderTileEditor(c);
    }
}