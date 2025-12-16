#nullable enable

using Emotion.Editor.Editor2D;
using Emotion.Editor.EditorUI.Components;
using Emotion.Primitives.Grids;

namespace Emotion.Editor.EditorUI.GridEditor;

[DontSerialize]
public abstract class GridEditorWindow : UIBaseWindow
{
    public Vector2? CursorTilePos { get; private set; }

    protected UIBaseWindow? _bottomBarToolButtons;
    protected EditorLabel? _bottomText;

    protected GridEditorWindow()
    {
        HandleInput = true;
        OrderInParent = -1;

        Tools = GetTools();
        CurrentTool = Tools[0];
        _lastUsedPlacingTool = CurrentTool;
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // temp
        Engine.UI.SetInputFocus(this);
    }

    public virtual void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        // Bottom text
        {
            var textList = new UIBaseWindow()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(10),
                    AnchorAndParentAnchor = UIAnchor.CenterLeft,
                    SizingY = UISizing.Fit()
                }
            };
            barContent.AddChild(textList);

            var label = new EditorLabel($"{GetGridName()} Editor")
            {
                WindowColor = Color.White * 0.5f
            };
            textList.AddChild(label);

            var labelDynamic = new EditorLabel("");
            textList.AddChild(labelDynamic);
            _bottomText = labelDynamic;
        }

        // Tool buttons
        {
            var buttonList = new UIBaseWindow()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(5),
                    AnchorAndParentAnchor = UIAnchor.CenterRight,
                    Margins = new UISpacing(5, 5, 5, 5)
                }
            };
            barContent.AddChild(buttonList);
            _bottomBarToolButtons = buttonList;

            for (int i = 0; i < Tools.Length; i++)
            {
                GridEditorTool tool = Tools[i];
                buttonList.AddChild(new GridEditorToolButton(this, tool));
            }
        }
    }

    #region Tools

    public GridEditorTool[] Tools;
    public GridEditorTool CurrentTool;
    protected GridEditorTool _lastUsedPlacingTool;

    public void SetCurrentTool(GridEditorTool currentTool)
    {
        CurrentTool = currentTool;
        if (currentTool.IsPlacingTool) _lastUsedPlacingTool = CurrentTool;

        if (_bottomBarToolButtons == null) return;
        foreach (UIBaseWindow child in _bottomBarToolButtons)
        {
            if (child is GridEditorToolButton toolButton)
            {
                toolButton.UpdateStyle();
            }
        }
    }

    public void SetCurrentToolAsLastPlacingTool()
    {
        SetCurrentTool(_lastUsedPlacingTool);
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            _mouseDown = status == KeyState.Down;

            // Instantly responsive on the mouse click event, don't wait for update
            if (_mouseDown) UpdateCurrentTool(mousePos, mousePos);

            // Clear last action to group undos by mouse clicks.
            //if (!_mouseDown) _lastAction = null;
        }

        if (status == KeyState.Down)
        {
            for (int i = 0; i < Tools.Length; i++)
            {
                GridEditorTool tool = Tools[i];
                if (key == tool.HotKey)
                {
                    SetCurrentTool(tool);
                    break;
                }
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    #endregion

    #region Tool Using

    private Vector2 _previousMousePos = new Vector2(-1);
    protected bool _mouseDown;

    public override void OnMouseMove(Vector2 mousePos)
    {
        UpdateCurrentTool(_previousMousePos, mousePos);
        _previousMousePos = mousePos;
    }

    private void UpdateCurrentTool(Vector2 previousPos, Vector2 newPos)
    {
        CursorTilePos = null;

        IGridWorldSpaceTiles? currentGrid = GetCurrentGrid();
        if (currentGrid == null || !MouseInside)
        {
            if (_bottomText != null)
                _bottomText.Text = "";
        }
        else
        {
            var currentTile = UpdateCursor();

            // Correct this for .ToString() reasons
            if (currentTile.X == -0) currentTile.X = 0;
            if (currentTile.Y == -0) currentTile.Y = 0;

            CursorTilePos = currentTile;

            bool inMap = currentGrid.IsValidPosition(currentTile);
            string inMapText = "";
            if (!inMap) inMapText = " (Outside Map)";

            if (_bottomText != null)
                _bottomText.Text = $"Rollover Tile - {currentTile}{inMapText}";
        }

        if (!MouseInside) return;
        if (!_mouseDown) return;
        if (CursorTilePos == null) return;
        if (currentGrid == null) return;
        if (!CanEdit()) return;

        previousPos = Engine.Renderer.Camera.ScreenToWorld(previousPos).ToVec2();
        newPos = Engine.Renderer.Camera.ScreenToWorld(newPos).ToVec2();

        LineSegment moveSegment = new LineSegment(previousPos, newPos);
        float moveSegmentLength = moveSegment.Length();
        if (CurrentTool.IsPrecisePaint && moveSegmentLength > 0)
            PrecisePaintApplyTool(moveSegment);
        else
            UseCurrentToolAtPosition(CursorTilePos.Value);
    }

    private HashSet<Vector2> _preciseDrawDedupe = new();

    private void PrecisePaintApplyTool(LineSegment moveSegment)
    {
        IGridWorldSpaceTiles? currentGrid = GetCurrentGrid();
        AssertNotNull(currentGrid);

        // Draw as a line
        float moveSegmentLength = moveSegment.Length();
        _preciseDrawDedupe.Clear();
        for (float i = 0; i < moveSegmentLength; i += 0.5f)
        {
            Vector2 pointAtLineSegment = moveSegment.PointOnLineAtDistance(i);
            Vector2 tile = currentGrid.GetTilePosOfWorldPos(pointAtLineSegment);
            if (!_preciseDrawDedupe.Add(tile)) continue;

            UseCurrentToolAtPosition(tile);
        }
    }

    #endregion

    protected abstract GridEditorTool[] GetTools();
    protected abstract string GetGridName();
    protected abstract IGridWorldSpaceTiles? GetCurrentGrid();
    protected abstract Vector2 UpdateCursor();
    protected abstract bool CanEdit();
    protected abstract void UseCurrentToolAtPosition(Vector2 tilePos);
}
