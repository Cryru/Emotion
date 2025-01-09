#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Tile;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;
using System.Linq;
using System.Security.Cryptography;

#endregion

namespace Emotion.Game.World2D.Editor;

public sealed class TilesetTileSelector : UIScrollArea
{
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            UITexture? texture = GetWindowById("TilesetTexture") as UITexture;
            if (texture != null)
            {
                texture.ImageScale = new Vector2(_scale);
                texture.InvalidateLayout();
            }
        }
    }

    private float _scale = 1f;

    private Map2DTileMapData _mapData;
    private Map2DTileset? _tileset;

    private HashSet<uint> _rolloverTile = new HashSet<uint>();
    private Vector2? _mouseDragStartPos;

    public List<uint> SelectedTiles = new List<uint>();

    public TilesetTileSelector(Map2DTileMapData mapData)
    {
        _mapData = mapData;
        HandleInput = true;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        var propagate = base.OnKey(key, status, mousePos);
        if (!propagate) return false;

        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyState.Down)
            {
                if (!Engine.Host.IsCtrlModifierHeld())
                    SelectedTiles.Clear();
                _mouseDragStartPos = mousePos;
                OnMouseMove(mousePos);
            }
            else if (status == KeyState.Up)
            {
                _mouseDragStartPos = null;

                if (_rolloverTile.Count == 1 && Engine.Host.IsCtrlModifierHeld())
                {
                    uint val = _rolloverTile.First();
                    if (SelectedTiles.Contains(val))
                        SelectedTiles.Remove(val);
                    else
                        SelectedTiles.Add(val);
                }
                else
                {
                    foreach (var tId in _rolloverTile)
                    {
                        if (!SelectedTiles.Contains(tId))
                            SelectedTiles.Add(tId);
                    }
                }

                _rolloverTile.Clear();
            }
        }

        return true;
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        base.OnMouseMove(mousePos);
        if (_tileset == null) return;

        Vector3 tileSetImageOrigin = new Vector3(0);
        tileSetImageOrigin = Vector3.Transform(tileSetImageOrigin, _content.ScrollTranslationMatrix);
        tileSetImageOrigin += _content.Position;

        Vector2 tileSetImageOrigin2 = tileSetImageOrigin.ToVec2();

        if (_mouseDragStartPos != null)
        {
            _rolloverTile.Clear();

            Vector2 startPos = _mouseDragStartPos.Value - tileSetImageOrigin2;
            Vector2 endPos = mousePos - tileSetImageOrigin2;

            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);
            for (float x = min.X; x <= max.X; x++)
            {
                for (float y = min.Y; y <= max.Y; y++)
                {
                    Vector2 samplePoint = new Vector2(x, y);
                    Vector2 rolloverCoord = TilesetCoordFromUISpace(samplePoint);
                    uint tid = TilesetCoordToTid(rolloverCoord);
                    _rolloverTile.Add(tid);
                }
            }
        }
        else
        {
            _rolloverTile.Clear();

            Vector2 rolloverCoord = TilesetCoordFromUISpace(mousePos - tileSetImageOrigin2);
            _rolloverTile.Add(TilesetCoordToTid(rolloverCoord));
        }
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        _rolloverTile.Clear();
    }

    // Assuming coords can never be out of range.
    // Assuming tid is always in current tileset.
    private uint TilesetCoordToTid(Vector2 coord)
    {
        if (_tileset == null) return 0;

        Vector2 sizeInTiles = _mapData.GetTilesetSizeInTiles(_tileset);
        int tilesetStart = _mapData.GetTilesetTidOffset(_tileset);
        return (uint)(tilesetStart + MathF.Floor((sizeInTiles.X * coord.Y) + coord.X));
    }

    private Vector2 TidToTilesetCoord(uint tid)
    {
        if (_tileset == null) return Vector2.Zero;

        Vector2 sizeInTiles = _mapData.GetTilesetSizeInTiles(_tileset);
        int tilesetStart = _mapData.GetTilesetTidOffset(_tileset);

        uint inThisTileSet = (uint)(tid - tilesetStart);

        var x = (int)(inThisTileSet % sizeInTiles.X);
        var y = (int)(inThisTileSet / sizeInTiles.X);
        return inThisTileSet >= sizeInTiles.X * sizeInTiles.Y ? Vector2.Zero : new Vector2(x, y);
    }

    private Vector2 TilesetCoordToUISpace(Vector2 coord, out Vector2 tileSize)
    {
        tileSize = Vector2.Zero;
        if (_tileset == null) return Vector2.Zero;

        var displayScale = new Vector2(_scale);
        tileSize = _mapData.TileSize * displayScale;
        var margin = new Vector2(_tileset.Margin) * displayScale;
        var spacing = new Vector2(_tileset.Spacing) * displayScale;

        return coord * (tileSize + spacing) + margin + spacing / 2f;
    }

    private Vector2 TilesetCoordFromUISpace(Vector2 uiPos)
    {
        if (_tileset == null) return Vector2.Zero;

        Vector2 displayScale = new Vector2(_scale);
        var tileSize = _mapData.TileSize * displayScale;
        var margin = new Vector2(_tileset.Margin) * displayScale;
        var spacing = new Vector2(_tileset.Spacing) * displayScale;

        Vector2 pos = ((uiPos - margin) / (tileSize + spacing)).Floor();
        pos = Vector2.Max(pos, Vector2.Zero);
        return pos;
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        return base.RenderInternal(c);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);

        c.PushModelMatrix(_content.ScrollTranslationMatrix);
        Rectangle? clip = c.CurrentState.ClipRect;
        c.SetClipRect(_content.Bounds);

        if (_tileset != null)
        {
            foreach (var tId in _rolloverTile)
            {
                Vector2 rolloverCoord = TidToTilesetCoord(tId);
                Vector2 uiSpace = TilesetCoordToUISpace(rolloverCoord, out Vector2 tileSize);

                c.RenderSprite(Position + uiSpace.ToVec3(), tileSize, Color.PrettyPurple * 0.4f);
                c.RenderOutline(Position + uiSpace.ToVec3(), tileSize, Color.Black * 0.4f, 2f);
                c.RenderOutline(Position + uiSpace.ToVec3(), tileSize, Color.White * 0.4f, 1f);
            }

            for (int i = 0; i < SelectedTiles.Count; i++)
            {
                var selectedTileId = SelectedTiles[i];

                Vector2 selectedCoord = TidToTilesetCoord(selectedTileId);
                Vector2 uiSpace = TilesetCoordToUISpace(selectedCoord, out Vector2 tileSize);

                c.RenderOutline(Position + uiSpace.ToVec3(), tileSize, Color.Black, 2f);
                c.RenderOutline(Position + uiSpace.ToVec3(), tileSize, Color.White, 1f);
            }
        }

        c.SetClipRect(clip);
        c.PopModelMatrix();
    }

    public void SetTileset(Map2DTileset? tileset)
    {
        _rolloverTile.Clear();
        SelectedTiles.Clear();
        _tileset = tileset;

        ClearChildrenInside();
        if (_tileset == null) return;

        var textureUI = new UITexture();
        textureUI.Id = "TilesetTexture";
        textureUI.TextureFile = _tileset.AssetFile;
        textureUI.ImageScale = new Vector2(Scale);
        textureUI.ScaleMode = UIScaleMode.NoScale;
        AddChildInside(textureUI);
        _content.ScrollToPos(Vector2.Zero);
    }

    public uint? GetSelectedTidToPlaceAbsolute()
    {
        if (_tileset == null) return null;
        if (SelectedTiles.Count == 0) return null;

        uint firstSel = SelectedTiles[0];
        int tIdStart = _mapData.GetTilesetTidOffset(_tileset);
        return (uint)tIdStart + firstSel;
    }
}
