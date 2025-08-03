#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Tools;
using Emotion.Editor.Tools.ChunkStreamVisualizer;
using Emotion.Editor.Tools.GameDataTool;
using Emotion.Editor.Tools.InterfaceTool;
using Emotion.Editor.Tools.SpriteEntityTool;
using Emotion.Game.Systems.UI;
using Emotion.Game.World;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Core.Platform.Implementation.Win32;
using Emotion.Standard.Reflector;

namespace Emotion.Editor.EditorUI;

public class EditorTopBar : UISolidColor
{
    public EditorTopBar()
    {
        GrowY = false;
        WindowColor = EditorColorPalette.BarColor;
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
            WindowColor = EditorColorPalette.ActiveButtonColor,
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
