#nullable enable

using Emotion.Editor.Editor2D.TileEditor;
using Emotion.Editor.Editor3D.TerrainEditor;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.MapObjectEditor;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.Editor2D;

[DontSerialize]
public class Editor2DBottomBar : UIBaseWindow
{
    private UIBaseWindow? _currentEditor;
    private UIBaseWindow _barContent;

    public Editor2DBottomBar()
    {
        Layout.SizingY = UISizing.Fit();
        Layout.AnchorAndParentAnchor = UIAnchor.BottomLeft;
        Visuals.BackgroundColor = EditorColorPalette.BarColor;
        Layout.LayoutMethod = UILayoutMethod.VerticalList(0);

        var accent = new UIBaseWindow
        {
            Layout =
            {
                SizingY = UISizing.Fixed(5)
            },
            Visuals =
            {
                BackgroundColor =  EditorColorPalette.ActiveButtonColor
            },
        };
        AddChild(accent);

        UIBaseWindow barContent = new()
        {
            Layout =
            {
                Margins = new UISpacing(5, 5, 5, 5),
            },
            Name = "Content"
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
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5),
                Margins = new UISpacing(5, 5, 5, 5)
            },
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
