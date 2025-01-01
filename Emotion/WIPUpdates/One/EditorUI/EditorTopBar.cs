using Emotion.Editor.EditorHelpers;
using Emotion.Game.Data;
using Emotion.Game.World.Editor;
using Emotion.Platform.Implementation.Win32;
using Emotion.UI;
using Emotion.WIPUpdates.One.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.EditorUI;

public class ExampleData : GameDataObject
{
}


public class EditorTopBar : UISolidColor
{
    public EditorTopBar()
    {
        FillY = false;
        WindowColor = MapEditorColorPalette.BarColor;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow buttonContainer = new()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
            Margins = new Rectangle(5, 5, 5, 10)
        };
        AddChild(buttonContainer);

        var accent = new UISolidColor
        {
            WindowColor = MapEditorColorPalette.ActiveButtonColor,
            MinSizeY = 5,
            Anchor = UIAnchor.BottomLeft,
            ParentAnchor = UIAnchor.BottomLeft
        };
        AddChild(accent);

        {
            EditorButton toolButton = new EditorButton("Test Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new TestTool());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Coroutines");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new CoroutineViewerTool());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Sprite Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new SpriteEntityTool());
            buttonContainer.AddChild(toolButton);
        }
        
        {
            EditorButton toolButton = new EditorButton("UI Debug Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new UIDebugTool());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("UI File Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new EditorWindowFileSupport("test"));
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Game Data Test");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new GameDataEditor<ExampleData>());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Open Folder");
            toolButton.OnClickedProxy = (_) => Process.Start("explorer.exe", ".");
            toolButton.Enabled = Engine.Host is Win32Platform;
            buttonContainer.AddChild(toolButton);
        }
    }
}
