using Emotion.Common.Serialization;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Editor;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor;
using Emotion.WIPUpdates.One.Editor3D.TerrainEditor;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.MapObjectEditor;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D;

[DontSerialize]
public class Editor2DBottomBar : UISolidColor
{
    private UIBaseWindow? _currentEditor;

    public Editor2DBottomBar()
    {
        GrowY = false;
        WindowColor = MapEditorColorPalette.BarColor;
        AnchorAndParentAnchor = UIAnchor.BottomLeft;

        Id = "BottomBar";
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
            EditorButton toolButton = new EditorButton("Objects");
            toolButton.OnClickedProxy = (_) =>
            {
                barContent.ClearChildren();
                SpawnBackButton(this, barContent);

                var editorWindow = new MapObjectEditorWindow();
                editorWindow.OrderInParent = -1;
                editorWindow.SpawnBottomBarContent(this, barContent);
                _currentEditor = editorWindow;

                EngineEditor.EditorRoot.AddChild(editorWindow);
            };
            toolButtonList.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Tile Editor");
            toolButton.OnClickedProxy = (_) =>
            {
                barContent.ClearChildren();
                SpawnBackButton(this, barContent);

                var editorWindow = new TileEditorWindow();
                editorWindow.SpawnBottomBarContent(this, barContent);
                _currentEditor = editorWindow;

                EngineEditor.EditorRoot.AddChild(editorWindow);
            };
            toolButtonList.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("3D Terrain Editor");
            toolButton.OnClickedProxy = (_) =>
            {
                barContent.ClearChildren();
                SpawnBackButton(this, barContent);

                var editorWindow = new TerrainEditorWindow();
                editorWindow.SpawnBottomBarContent(this, barContent);
                _currentEditor = editorWindow;

                EngineEditor.EditorRoot.AddChild(editorWindow);
            };
            toolButtonList.AddChild(toolButton);
        }
    }

    private void SpawnBackButton(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        var back = new EditorButton("Back")
        {
            OnClickedProxy = (_) => bar.SpawnEditorChoiceScreen(),
            Anchor = UIAnchor.BottomLeft,
            ParentAnchor = UIAnchor.TopLeft,
        };
        barContent.AddChild(back);
    }

    #endregion
}
