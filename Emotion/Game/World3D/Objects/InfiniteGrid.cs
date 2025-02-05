#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;

#endregion

namespace Emotion.Game.World3D.Objects;

public class SquareGrid3D : Quad3D
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

    /// <summary>
    /// Whether to correct the grid to always keep tile positions in the top left.
    /// Otherwise the grid will just keep its origin position (center of the whole grid)
    /// in the center of a tile and tiles along the border might be cut off.
    /// </summary>
    public bool ApplyTopLeftOriginCorrection = false;

    public SquareGrid3D()
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
        Vector2 size2D = Size3D.ToVec2();
        Vector2 gridOffset = GridOffset;

        // If the tile size is odd/even but the total size isn't the same,
        // then we need to offset by half a tile in order to maintain the center at a tile center.
        if (ApplyTopLeftOriginCorrection)
        {
            bool tileSizeIsEvenX = _tileSize2.X % 2 == 0;
            bool tileSizeIsEvenY = _tileSize2.Y % 2 == 0;
            bool gridSizeIsEvenX = (size2D.X / _tileSize2.X) % 2 == 0;
            bool gridSizeIsEvenY = (size2D.Y / _tileSize2.Y) % 2 == 0;

            if (tileSizeIsEvenX && gridSizeIsEvenX) gridOffset.X += _tileSize2.X / 2f;
            if (tileSizeIsEvenY && gridSizeIsEvenY) gridOffset.Y += _tileSize2.Y / 2f;
        }

        AssertNotNull(EntityMetaState);
        EntityMetaState.SetShaderParam("squareSize", _tileSize2);
        EntityMetaState.SetShaderParam("cameraPos", gridOffset / size2D);
        EntityMetaState.SetShaderParam("totalSize", size2D);

        base.RenderInternal(c);
    }

    public override bool IsTransparent()
    {
        return true;
    }
}

public sealed class InfiniteGrid : SquareGrid3D
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

        GridOffset = Position2 + Offset;

        base.RenderInternal(c);
    }
}