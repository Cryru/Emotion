#nullable enable

using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.Workflow.LevelDesign.Terrain;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.Workflow;

public partial class LevelDesignWorkflow : EditorWorkflow
{
    public override string Name { get; } = "Level Design";

    public override void Init(UIBaseWindow parent)
    {
        base.Init(parent);

        UIBaseWindow controls = new UIBaseWindow();
        parent.AddChild(controls);

        controls.AddChild(new MapGridDisplay(this));
        //controls.AddChild(new ObjectSelection(this));

        var cameraViewMode = new MapEditorViewMode();
        cameraViewMode.Layout.AnchorAndParentAnchor = UIAnchor.BottomLeft;
        cameraViewMode.Layout.Margins = new UISpacing(0, 0, 0, 10);
        parent.AddChild(cameraViewMode);
    }
}

public partial class LevelDesignWorkflow
{
    private class MapGridDisplay : UIBaseWindow
    {
        private LevelDesignWorkflow _workFlow;
        private UIBaseWindow _layerList;

        public MapGridDisplay(LevelDesignWorkflow workFlow)
        {
            _workFlow = workFlow;

            var buttonList = new UIBaseButton()
            {
                Visuals =
                {
                    BackgroundColor = EditorColorPalette.BarColor
                },
                Layout =
                {
                    SizingY = UISizing.Fit(),
                    Padding = new UISpacing(5, 5, 5, 5),
                    LayoutMethod = UILayoutMethod.HorizontalList(3)
                }
            };
            AddChild(buttonList);

            var secondBar = new UIBaseButton()
            {
                Visuals =
                {
                    BackgroundColor = EditorColorPalette.BarColor
                },
                Layout =
                {
                    SizingY = UISizing.Fit(),
                    Padding = new UISpacing(5, 5, 5, 5),
                    LayoutMethod = UILayoutMethod.HorizontalList(3)
                }
            };
            AddChild(secondBar);

            EditorButton newLayer = new("+ New")
            {
                Name = "NewLayerButton",
                OnClickedUpProxy = (_) => NewLayer()
            };
            buttonList.AddChild(newLayer);
            buttonList.AddChild(new OneButton(
                "(temp) Terrain Editor",
                (_) =>
                {
                    Parent.AddChild(new TerrainMeshEditor());
                },
                ButtonType.Outlined
            ));
        }

        protected override void OnOpen()
        {
            base.OnOpen();



        }

        private void NewLayer()
        {
            GameMap? currentMap = EngineEditor.GetCurrentMap();
            if (currentMap == null) return;

            UIBaseWindow? newLayerButton = GetWindowById("NewLayerButton");
            if (newLayerButton == null) return;

            EditorDropDown dropDown = EditorDropDown.OpenListDropdown(newLayerButton);

            IGenericReflectorTypeHandler[] gridTypes = ReflectorEngine.GetDescendantsOf<IMapGrid>();
            foreach (IGenericReflectorTypeHandler typeHandler in gridTypes)
            {
                EditorButton button = new EditorButton(typeHandler.TypeName);
                //button.GrowX = true;
                button.OnClickedProxy = (_) =>
                {
                    IGenericReflectorComplexTypeHandler? complexHandler = typeHandler as IGenericReflectorComplexTypeHandler;
                    object? newGrid = complexHandler?.CreateNew();
                    if (newGrid is IMapGrid grid)
                        currentMap.AddGrid(grid);

                    Engine.UI.CloseDropdown();
                };
                dropDown.AddChild(button);
            }
        }

        private void UpdateLayers()
        {
            _layerList.ClearChildren();

            GameMap? map = EngineEditor.GetCurrentMap();



        }
    }
}