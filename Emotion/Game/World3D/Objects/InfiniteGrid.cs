#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D.Objects;

public class ThreeDeeSquareGrid : Quad3D
{
    public float TileSize
    {
        get => _tileSize;
        set
        {
            _tileSize = value;
            _tileSize2 = new Vector2(value);
        }
    }

    protected float _tileSize = 100;
    protected Vector2 _tileSize2 = new Vector2(100);

    public Vector2 GridOffset;

    public ThreeDeeSquareGrid()
    {
        ObjectFlags |= ObjectFlags.Map3DDontReceiveShadow;
        ObjectFlags |= ObjectFlags.Map3DDontThrowShadow;
        ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;
    }

    public override async Task LoadAssetsAsync()
    {
        await base.LoadAssetsAsync();
        await EntityMetaState!.SetShader("Shaders/3DGrid.xml");
    }

    protected override void RenderInternal(RenderComposer c)
    {
        EntityMetaState!.SetShaderParam("squareSize", _tileSize2);
        EntityMetaState.SetShaderParam("cameraPos", GridOffset);
        EntityMetaState.SetShaderParam("totalSize", Size3D.ToVec2());

        base.RenderInternal(c);
    }

    public override bool IsTransparent()
    {
        return true;
    }
}

public sealed class InfiniteGrid : ThreeDeeSquareGrid
{
    public Vector2 Offset;

    private Vector2 _infiniteGridSize = new Vector2(10_000, 10_000);

    public InfiniteGrid()
    {
        Size3D = _infiniteGridSize.ToVec3(1);
    }

    protected override void RenderInternal(RenderComposer c)
    {
        Vector2 cameraPos = c.Camera.Position.ToVec2();
        Position = cameraPos.ToVec3(Z); // Set position to camera position without the Z

        GridOffset = (Position2 + Offset) / _infiniteGridSize;

        base.RenderInternal(c);
    }
}