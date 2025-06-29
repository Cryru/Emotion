﻿#nullable enable

using Emotion.Common.Input;
using Emotion.Common.Serialization;
using Emotion.UI;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One.Editor2D;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.ThreeDee.GridStreaming;

namespace Emotion.WIPUpdates.One.EditorUI.GridEditor;

[DontSerialize]
public abstract class GridEditorWindow : UIBaseWindow
{
    public Vector2? CursorTilePos { get; private set; }

    protected UIBaseWindow? _bottomBarToolButtons;
    protected UIRichText? _bottomText;

    protected GridEditorWindow()
    {
        HandleInput = true;
        OrderInParent = -1;

        Tools = GetTools();
        CurrentTool = Tools[0];
        _lastUsedPlacingTool = CurrentTool;
    }

    public override void AttachedToController(UIController controller)
    {
        OnOpen();

        base.AttachedToController(controller);

        // temp
        controller.SetInputFocus(this);
    }

    public override void DetachedFromController(UIController controller)
    {
        OnClose();

        base.DetachedFromController(controller);
    }

    public virtual void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        // Bottom text
        {
            var textList = new UIBaseWindow()
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(10, 0),
                AnchorAndParentAnchor = UIAnchor.CenterLeft,
                GrowY = false,
            };
            barContent.AddChild(textList);

            var label = new EditorLabel
            {
                Text = $"{GetGridName()} Editor",
                WindowColor = Color.White * 0.5f
            };
            textList.AddChild(label);

            var labelDynamic = new EditorLabel
            {
                Text = "",
                AllowRenderBatch = false
            };
            textList.AddChild(labelDynamic);
            _bottomText = labelDynamic;
        }

        // Tool buttons
        {
            var buttonList = new UIBaseWindow()
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(5, 0),
                AnchorAndParentAnchor = UIAnchor.CenterRight,
                Margins = new Rectangle(5, 5, 5, 5),
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

            string currentTileText;
            if (currentGrid is IChunkedGrid chunkedGrid)
                currentTileText = $"{currentTile} (Chunk: {chunkedGrid.GetChunkCoordinateOfValueCoordinate(currentTile)})";
            else
                currentTileText = $"{currentTile}";

            if (_bottomText != null)
                _bottomText.Text = $"Rollover Tile - {currentTileText}{inMapText}";
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
    protected abstract void OnOpen();
    protected abstract void OnClose();
    protected abstract string GetGridName();
    protected abstract IGridWorldSpaceTiles? GetCurrentGrid();
    protected abstract Vector2 UpdateCursor();
    protected abstract bool CanEdit();
    protected abstract void UseCurrentToolAtPosition(Vector2 tilePos);
}
