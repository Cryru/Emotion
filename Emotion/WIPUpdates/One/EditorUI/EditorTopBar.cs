#nullable enable

using Emotion.Game.Data;
using Emotion.Game.World.Editor;
using Emotion.Platform.Implementation.Win32;
using Emotion.Scenography;
using Emotion.Standard.Reflector;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.Tools;
using Emotion.WIPUpdates.One.Tools.ChunkStreamVisualizer;
using Emotion.WIPUpdates.One.Tools.GameDataTool;
using Emotion.WIPUpdates.One.Tools.InterfaceTool;
using Emotion.WIPUpdates.One.Tools.SpriteEntityTool;
using Emotion.WIPUpdates.ThreeDee.GridStreaming;

namespace Emotion.WIPUpdates.One.EditorUI;

public class EditorTopBar : UISolidColor
{
    public EditorTopBar()
    {
        GrowY = false;
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

        AttachMapMenu(buttonContainer);

        //{
        //    EditorButton toolButton = new EditorButton("Test Tool");
        //    toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new TestTool());
        //    buttonContainer.AddChild(toolButton);
        //}

        //{
        //    EditorButton toolButton = new EditorButton("Coroutines");
        //    toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new CoroutineViewerTool());
        //    buttonContainer.AddChild(toolButton);
        //}

        {
            EditorButton toolButton = new EditorButton("Sprite Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new SpriteEntityEditor());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("UI Debug Tool");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new UIDebugTool());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Interface Editor");
            toolButton.OnClickedProxy = (_) => EngineEditor.OpenToolWindowUnique(new InterfaceTool());
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Game Data");
            toolButton.OnClickedProxy = (me) =>
            {
                UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

                Type[] gameDataObjectTypes = ReflectorEngine.GetTypesDescendedFrom(typeof(GameDataObject), true);
                foreach (Type typ in gameDataObjectTypes)
                {
                    EditorButton button = new EditorButton(typ.Name);
                    button.GrowX = true;
                    button.OnClickedProxy = (_) =>
                    {
                        EngineEditor.OpenToolWindowUnique(new GameDataEditor(typ));
                        Controller?.DropDown?.Close();
                    };
                    dropDown.AddChild(button);
                }
            };
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Chunk Stream Visualizer");
            toolButton.OnClickedProxy = (_) =>
            {
                GameMap? currentMap = EngineEditor.GetCurrentMap();
                if (currentMap == null || currentMap.TerrainGrid == null) return;
                if (currentMap.TerrainGrid is IStreamableGrid streamGrid)
                    EngineEditor.OpenToolWindowUnique(new ChunkStreamVisualizer(streamGrid));
            };
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Open Folder");
            toolButton.OnClickedProxy = (_) => Process.Start("explorer.exe", ".");
            toolButton.Enabled = Engine.Host is Win32Platform;
            buttonContainer.AddChild(toolButton);
        }
    }

    #region Map Menu

    private void AttachMapMenu(UIBaseWindow buttonContainer)
    {
        {
            EditorButton toolButton = new EditorButton("Map");
            toolButton.OnClickedProxy = (me) =>
            {
                UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

                {
                    EditorButton button = new EditorButton("New");
                    button.GrowX = true;
                    button.OnClickedProxy = (_) =>
                    {
                        //NewFile();
                        dropDown.Close();
                    };
                    dropDown.AddChild(button);
                }

                {
                    EditorButton button = new EditorButton("Open...");
                    button.GrowX = true;
                    button.OnClickedProxy = (_) =>
                    {
                        dropDown.Close();
                    };
                    dropDown.AddChild(button);
                }

                {
                    EditorButton button = new EditorButton("Save");
                    button.GrowX = true;
                    button.OnClickedProxy = (_) =>
                    {
                        dropDown.Close();
                    };
                    button.Enabled = EngineEditor.GetCurrentMap() != null;
                    dropDown.AddChild(button);
                }

                {
                    EditorButton button = new EditorButton("Properties");
                    button.GrowX = true;
                    button.OnClickedProxy = (_) =>
                    {
                        dropDown.Close();

                        GameMap? map = EngineEditor.GetCurrentMap();
                        if (map == null) return;

                        var win = new ObjectPropertyEditorWindow(map);
                        EngineEditor.EditorRoot.AddChild(win);
                    };
                    button.Enabled = EngineEditor.GetCurrentMap() != null;
                    dropDown.AddChild(button);
                }

                //{
                //    EditorButton button = new EditorButton("Save As");
                //    button.FillX = true;
                //    dropDown.AddChild(button);
                //}
            };
            toolButton.Enabled = Engine.SceneManager.Current is SceneWithMap;
            buttonContainer.AddChild(toolButton);
        }
    }

    #endregion
}
