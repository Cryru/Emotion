#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.TileMap;
using System.Linq;
using static Emotion.UI.UIBaseWindow;

#endregion

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor;

public sealed class TileEditorTileTextureSelector : EditorScrollArea
{
    public float TilesetScale
    {
        get => _tilesetScale;
        set
        {
            _tilesetScale = value;
            UITexture? texture = GetWindowById("TilesetTexture") as UITexture;
            if (texture != null)
            {
                texture.ImageScale = new Vector2(_tilesetScale);
                texture.InvalidateLayout();
            }
        }
    }

    private float _tilesetScale = 0.5f;

    private TileMapTileset? _tileset;
    private Vector2 _tilesetSizeInTiles;

    private HashSet<Vector2> _rolloverTiles = new HashSet<Vector2>();
    private List<Vector2> _selectedTiles = new List<Vector2>();

    private Vector2? _mouseDragStartPos;

    public TileEditorTileTextureSelector(TileEditorWindow editor)
    {
        HandleInput = true;
        Paddings = new Primitives.Rectangle(5, 5, 5, 5);
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
                    _selectedTiles.Clear();
                _mouseDragStartPos = mousePos;
                OnMouseMove(mousePos);
            }
            else if (status == KeyState.Up)
            {
                _mouseDragStartPos = null;

                if (_rolloverTiles.Count == 1 && Engine.Host.IsCtrlModifierHeld())
                {
                    Vector2 val = _rolloverTiles.First();
                    if (_selectedTiles.Contains(val))
                        _selectedTiles.Remove(val);
                    else
                        _selectedTiles.Add(val);
                }
                else
                {
                    foreach (var tId in _rolloverTiles)
                    {
                        if (!_selectedTiles.Contains(tId))
                            _selectedTiles.Add(tId);
                    }
                }

                _rolloverTiles.Clear();
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
            _rolloverTiles.Clear();

            Vector2 tileSize = _tileset.TileSize * new Vector2(_tilesetScale);
            Vector2 tileSizeHalf = tileSize / 2f;

            Vector2 startPosRelative = _mouseDragStartPos.Value - tileSetImageOrigin2;
            Vector2 endPosRelative = mousePos - tileSetImageOrigin2;

            Rectangle relativeSelectionRect = Rectangle.FromMinMaxPointsChecked(startPosRelative, endPosRelative);
            relativeSelectionRect.SnapToGrid(tileSize);
            relativeSelectionRect.GetMinMaxPoints(out Vector2 min, out Vector2 max);

            for (float x = min.X; x < max.X; x += tileSize.X)
            {
                for (float y = min.Y; y < max.Y; y += tileSize.Y)
                {
                    Vector2 samplePoint = new Vector2(x, y) + tileSizeHalf;
                    Vector2 rolloverCoord = TilesetCoordFromUISpace(samplePoint);
                    _rolloverTiles.Add(rolloverCoord);
                }
            }
        }
        else
        {
            _rolloverTiles.Clear();

            Vector2 rolloverCoord = TilesetCoordFromUISpace(mousePos - tileSetImageOrigin2);
            _rolloverTiles.Add(rolloverCoord);
        }
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        _rolloverTiles.Clear();
    }

    private Vector2 TilesetCoordToUISpace(Vector2 coord, out Vector2 tileSize)
    {
        tileSize = Vector2.Zero;
        if (_tileset == null) return Vector2.Zero;

        Vector2 displayScale = new Vector2(_tilesetScale);

        tileSize = _tileset.TileSize * displayScale;
        Vector2 margin = _tileset.Margin * displayScale;
        Vector2 spacing = _tileset.Spacing * displayScale;

        return coord * (tileSize + spacing) + margin + spacing / 2f;
    }

    private Vector2 TilesetCoordFromUISpace(Vector2 uiPos)
    {
        if (_tileset == null) return Vector2.Zero;

        Vector2 displayScale = new Vector2(_tilesetScale);
        var tileSize = _tileset.TileSize * displayScale;
        var margin = _tileset.Margin * displayScale;
        var spacing = _tileset.Spacing * displayScale;

        Vector2 pos = ((uiPos - margin) / (tileSize + spacing)).Floor();
        pos = Vector2.Max(pos, Vector2.Zero);
        return pos;
    }

    private TileTextureId GetTIdFromTilesetCoord(Vector2 coord)
    {
        if (_tileset == null) return 0;

        float tilesPerRow = _tilesetSizeInTiles.X;
        var tileOneD = (coord.Y * tilesPerRow) + coord.X;
        tileOneD += 1; // 0 is empty

        return (TileTextureId) tileOneD;
    }

    protected override void RenderChildren(RenderComposer c)
    {
        List<UIBaseWindow> children = GetWindowChildren();
        for (var i = 0; i < children.Count; i++)
        {
            UIBaseWindow child = children[i];
            if (!child.Visible) continue;
            if (child.OverlayWindow) continue;

            child.Render(c);

            if (i == 0)
                AfterContentRendered(c);
        }
    }

    private void AfterContentRendered(RenderComposer c)
    {
        c.PushModelMatrix(_content.ScrollTranslationMatrix);
        Rectangle? clip = c.CurrentState.ClipRect;
        c.SetClipRect(_content.Bounds);

        if (_tileset != null)
        {
            Vector3 contentPos = _content.Position;

            foreach (Vector2 rolloverTileCoord in _rolloverTiles)
            {
                Vector2 uiSpace = TilesetCoordToUISpace(rolloverTileCoord, out Vector2 tileSize);

                c.RenderSprite(contentPos + uiSpace.ToVec3(), tileSize, Color.PrettyPurple * 0.4f);
                c.RenderOutline(contentPos + uiSpace.ToVec3(), tileSize, Color.Black * 0.4f, 2f);
                c.RenderOutline(contentPos + uiSpace.ToVec3(), tileSize, Color.White * 0.4f, 1f);
            }

            foreach (Vector2 selectedTileCoord in _selectedTiles)
            {
                Vector2 uiSpace = TilesetCoordToUISpace(selectedTileCoord, out Vector2 tileSize);

                c.RenderOutline(contentPos + uiSpace.ToVec3(), tileSize, Color.Black, 2f);
                c.RenderOutline(contentPos + uiSpace.ToVec3(), tileSize, Color.White, 1f);
            }
        }

        c.SetClipRect(clip);
        c.PopModelMatrix();
    }

    public void SetTileset(TileMapTileset? tileset)
    {
        _rolloverTiles.Clear();
        _selectedTiles.Clear();
        _tileset = tileset;

        ClearChildrenInside();
        _content.ScrollToPos(Vector2.Zero);
        if (_tileset == null) return;

        // todo: load asset, support hot reload
        _tilesetSizeInTiles = _tileset.GetTilesetSizeInTiles();

        var textureUI = new UITexture
        {
            Id = "TilesetTexture",
            TextureFile = _tileset.Texture,
            ImageScale = new Vector2(TilesetScale),
            ScaleMode = UIScaleMode.NoScale,
            Smooth = true
        };
        AddChildInside(textureUI);
    }

    public (TileTextureId, Vector2)[] GetSelectedTileTextures(out Vector2 center)
    {
        center = Vector2.Zero;
        if (_selectedTiles.Count == 0)
            return [(0, new Vector2(0))];

        var pattern = new (TileTextureId, Vector2)[_selectedTiles.Count];
        Vector2 originPos = Vector2.Zero;
        for (int i = 0; i < _selectedTiles.Count; i++)
        {
            Vector2 tileCoord = _selectedTiles[i];
            TileTextureId tId = GetTIdFromTilesetCoord(tileCoord);

            if (i == 0)
            {
                originPos = tileCoord;
                pattern[i] = (tId, Vector2.Zero);
                continue;
            }
            Vector2 offsetFromOriginInTiles = tileCoord - originPos;
            center += offsetFromOriginInTiles;
            pattern[i] = (tId, offsetFromOriginInTiles);
        }
        center /= _selectedTiles.Count;
        center = center.Floor();

        return pattern;
    }

    //public uint? GetSelectedTidToPlaceAbsolute()
    //{
    //    if (_tileset == null) return null;
    //    if (SelectedTiles.Count == 0) return null;

    //    uint firstSel = SelectedTiles[0];
    //    int tIdStart = _mapData.GetTilesetTidOffset(_tileset);
    //    return (uint)tIdStart + firstSel;
    //}
}
