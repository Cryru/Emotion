using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Editor;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D;

public class Editor2DBottomBar : UISolidColor
{
    private UIBaseWindow? _currentEditor;

    public Editor2DBottomBar()
    {
        FillY = false;
        WindowColor = MapEditorColorPalette.BarColor;
        AnchorAndParentAnchor = UIAnchor.BottomLeft;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var accent = new UISolidColor
        {
            WindowColor = MapEditorColorPalette.ActiveButtonColor,
            MinSizeY = 5,
            MaxSizeY = 5,
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.TopLeft
        };
        AddChild(accent);

        UIBaseWindow barContent = new()
        {
            Paddings = new Primitives.Rectangle(5, 10, 5, 5),
            AnchorAndParentAnchor = UIAnchor.BottomLeft,
            Id = "Content",
            HandleInput = true
        };
        AddChild(barContent);

        SpawnEditorChoiceScreen();
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);

        _currentEditor?.Close();
        _currentEditor = null;
    }

    #region Modes

    public void SpawnEditorChoiceScreen()
    {
        UIBaseWindow? barContent = GetWindowById("Content");
        AssertNotNull(barContent);

        barContent.ClearChildren();

        UIBaseWindow toolButtonList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            Margins = new Rectangle(5, 5, 5, 5),
        };
        barContent.AddChild(toolButtonList);

        _currentEditor?.Close();
        _currentEditor = null;

        {
            EditorButton toolButton = new EditorButton("Tile Editor");
            toolButton.OnClickedProxy = (_) =>
            {
                barContent.ClearChildren();

                TileEditorWindow editorWindow = new TileEditorWindow();
                editorWindow.OrderInParent = -1;
                editorWindow.SpawnBottomBarContent(this, barContent);
                _currentEditor = editorWindow;

                EngineEditor.EditorRoot.AddChild(editorWindow);
            };
            toolButtonList.AddChild(toolButton);
        }
    }

    #endregion
}
