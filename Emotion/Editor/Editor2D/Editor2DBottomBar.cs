#nullable enable

using Emotion.Editor.Editor2D.TileEditor;
using Emotion.Editor.Editor3D.TerrainEditor;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.MapObjectEditor;
using Emotion.Game.Systems.UI;

namespace Emotion.Editor.Editor2D;

[DontSerialize]
public class Editor2DBottomBar : UISolidColor
{
    private UIBaseWindow? _currentEditor;
    private UIBaseWindow _barContent;

    public Editor2DBottomBar()
    {
        GrowY = false;
        WindowColor = EditorColorPalette.BarColor;
        AnchorAndParentAnchor = UIAnchor.BottomLeft;

        var accent = new UISolidColor
        {
            WindowColor = EditorColorPalette.ActiveButtonColor,
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
            Name = "Content",
            HandleInput = true
        };
        AddChild(barContent);
        _barContent = barContent;

        SpawnEditorChoiceScreen();

        Name = "BottomBar";
    }

    protected override void OnClose()
    {
        base.OnClose();
        _currentEditor?.Close();
        _currentEditor = null;
    }

    #region Modes

    public void SpawnEditorChoiceScreen()
    {
        UIBaseWindow? barContent = _barContent;
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
