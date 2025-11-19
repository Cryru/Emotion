#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Tools;
using Emotion.Editor.Tools.ChunkStreamVisualizer;
using Emotion.Editor.Tools.GameDataTool;
using Emotion.Editor.Tools.InterfaceTool;
using Emotion.Editor.Tools.SpriteEntityTool;
using Emotion.Core.Platform.Implementation.Win32;
using Emotion.Standard.Reflector;
using Emotion.Editor.Workflow;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.EditorUI;

public class EditorTopBar : UIBaseWindow
{
    public EditorTopBar()
    {
        Name = "EditorTopBar";
        Layout.SizingX = UISizing.Grow();
        Layout.SizingY = UISizing.Fit();
        Visuals.BackgroundColor = EditorColorPalette.BarColor;
        Layout.LayoutMethod = UILayoutMethod.VerticalList(0);

        UIBaseWindow buttonContainer = new()
        {
            Name = "ButtonContainer",
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5),
                Margins = new UISpacing(5, 5, 5, 5)
            }
        };
        AddChild(buttonContainer);

        var accent = new UIBaseWindow
        {
            Layout =
            {
                SizingY = UISizing.Fixed(5),
            },
            Visuals =
            {
                BackgroundColor = EditorColorPalette.ActiveButtonColor
            },
        };
        AddChild(accent);
    }

    protected override bool RenderInternal(Renderer c)
    {
        return base.RenderInternal(c);
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow? buttonContainer = GetWindowById("ButtonContainer");
        if (buttonContainer == null) return;
        AttachWorkflowMenu(buttonContainer);
        AttachMapMenu(buttonContainer);

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

                IGenericReflectorTypeHandler[] gameDataTypes = ReflectorEngine.GetDescendantsOf<GameDataObject>(true);
                foreach (IGenericReflectorTypeHandler handler in gameDataTypes)
                {
                    EditorButton button = new EditorButton(handler.TypeName);
                    button.Layout.SizingX = UISizing.Grow();
                    button.OnClickedProxy = (_) =>
                    {
                        IGenericReflectorComplexTypeHandler? complexHandler = handler as IGenericReflectorComplexTypeHandler;
                        if (complexHandler != null)
                            EngineEditor.OpenToolWindowUnique(new GameDataEditor(complexHandler));
                        Engine.UI.CloseDropdown();
                    };
                    dropDown.AddChild(button);
                }
            };
            buttonContainer.AddChild(toolButton);
        }

        //{
        //    EditorButton toolButton = new EditorButton("Chunk Stream Visualizer");
        //    toolButton.OnClickedProxy = (_) =>
        //    {
        //        GameMap? currentMap = EngineEditor.GetCurrentMap();
        //        if (currentMap?.TerrainGrid != null)
        //            EngineEditor.OpenToolWindowUnique(new ChunkStreamVisualizer(currentMap.TerrainGrid));
        //    };
        //    buttonContainer.AddChild(toolButton);
        //}

        {
            EditorButton toolButton = new EditorButton("Job System Visualizer");
            toolButton.OnClickedProxy = (_) =>
            {
                EngineEditor.OpenToolWindowUnique(new JobSystemVisualizer());
            };
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("Open Folder");
            toolButton.OnClickedProxy = (_) => Process.Start("explorer.exe", ".");
            toolButton.Enabled = Engine.Host is Win32Platform;
            buttonContainer.AddChild(toolButton);
        }

        {
            EditorButton toolButton = new EditorButton("EditorUI Style Guide");
            toolButton.OnClickedProxy = (_) =>
            {
                EngineEditor.OpenToolWindowUnique(new StyleGuideViewer());
            };
            buttonContainer.AddChild(toolButton);
        }

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
    }

    private void AttachMapMenu(UIBaseWindow buttonContainer)
    {
        EditorButton toolButton = new EditorButton("Map");
        toolButton.OnClickedProxy = (me) =>
        {
            UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

            {
                EditorButton button = new EditorButton("New");
                button.Layout.SizingX = UISizing.Grow();
                button.OnClickedProxy = (_) =>
                {
                    //NewFile();
                    dropDown.Close();
                };
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Open...");
                button.Layout.SizingX = UISizing.Grow();
                button.OnClickedProxy = (_) =>
                {
                    dropDown.Close();
                };
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.Layout.SizingX = UISizing.Grow();
                button.OnClickedProxy = (_) =>
                {
                    dropDown.Close();
                };
                button.Enabled = EngineEditor.GetCurrentMap() != null;
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Properties");
                button.Layout.SizingX = UISizing.Grow();
                button.OnClickedProxy = (_) =>
                {
                    dropDown.Close();

                    GameMap? map = EngineEditor.GetCurrentMap();
                    if (map == null) return;

                    var win = new ObjectPropertyEditorWindow(map);
                    EngineEditor.EditorUI.AddChild(win);
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

    private void AttachWorkflowMenu(UIBaseWindow buttonContainer)
    {
        EditorButton toolButton = new EditorButton("Workflow");
        toolButton.OnClickedProxy = (me) =>
        {
            UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

            IGenericReflectorTypeHandler[] workflows = ReflectorEngine.GetDescendantsOf<EditorWorkflow>();
            foreach (IGenericReflectorTypeHandler workflow in workflows)
            {
                IGenericReflectorComplexTypeHandler? complexHandler = workflow as IGenericReflectorComplexTypeHandler;
                EditorWorkflow? workflowInstance = complexHandler?.CreateNew() as EditorWorkflow;
                if (workflowInstance == null) continue;

                EditorButton button = new(workflowInstance.Name)
                {
                    Layout =
                    {
                        SizingX = UISizing.Grow()
                    },
                    OnClickedProxy = (_) =>
                    {
                        EngineEditor.SetWorkflow(workflowInstance);
                        dropDown.Close();
                    }
                };
                dropDown.AddChild(button);
            }
        };
        buttonContainer.AddChild(toolButton);
    }
}
