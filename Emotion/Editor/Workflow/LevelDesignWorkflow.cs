#nullable enable

using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.Workflow;

public partial class LevelDesignWorkflow : EditorWorkflow
{
    public override string Name { get; } = "Level Design";

    public override void Init(UIBaseWindow parent)
    {
        base.Init(parent);

        UIBaseWindow controls = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = EditorColorPalette.BarColor,
            },
            Layout =
            {
                SizingX = UISizing.Fit(),
                LayoutMethod = UILayoutMethod.VerticalList(0),
                Padding = new UISpacing(5, 0, 5, 0)
            }
        };
        parent.AddChild(controls);

        MapGridDisplay mapLayerDisplay = new MapGridDisplay();
        controls.AddChild(mapLayerDisplay);
    }
}

public partial class LevelDesignWorkflow
{
    private class MapGridDisplay : UIBaseWindow
    {
        private UIBaseWindow _layerList;

        public MapGridDisplay()
        {
            Layout.LayoutMethod = UILayoutMethod.VerticalList(0);
            Layout.SizingY = UISizing.Fit();

            NewUIScrollArea layerList = new NewUIScrollArea()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(1)
                }
            };
            AddChild(layerList);
            _layerList = layerList;

            EditorButton newLayer = new("+ New")
            {
                Name = "NewLayerButton",
                OnClickedUpProxy = (_) => NewLayer()
            };
            AddChild(newLayer);
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

            Type[] gridTypes = ReflectorEngine.GetTypesDescendedFrom<IMapGrid>();
            foreach (Type type in gridTypes)
            {
                EditorButton button = new EditorButton(type.Name);
                button.GrowX = true;
                button.OnClickedProxy = (_) =>
                {
                    IGenericReflectorComplexTypeHandler? handler = ReflectorEngine.GetComplexTypeHandler(type);
                    object? newGrid = handler?.CreateNew();
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