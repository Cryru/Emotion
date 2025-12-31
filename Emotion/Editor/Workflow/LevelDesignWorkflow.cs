#nullable enable

using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.Workflow;

public partial class LevelDesignWorkflow : EditorWorkflow
{
    public override string Name { get; } = "Level Design";

    private GameObject? _objectUnderMouse = null;

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
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Fit(),
                LayoutMethod = UILayoutMethod.HorizontalList(0),
                Padding = new UISpacing(5, 5, 5, 5)
            }
        };
        parent.AddChild(controls);
        parent.AddChild(new ObjectSelection(this));

        var mapLayerDisplay = new MapGridDisplay();
        controls.AddChild(mapLayerDisplay);

        var cameraViewMode = new MapEditorViewMode();
        cameraViewMode.Layout.AnchorAndParentAnchor = UIAnchor.BottomLeft;
        cameraViewMode.Layout.Margins = new UISpacing(0, 0, 0, 10);
        parent.AddChild(cameraViewMode);
    }

    public override void Render(Renderer r)
    {
        if (_objectUnderMouse != null)
        {
            var bound = _objectUnderMouse.GetBoundingCube();
            r.SetUseViewMatrix(true);
            bound.RenderOutline(r, Color.PrettyYellow, 0.03f);
            r.SetUseViewMatrix(false);
        }
    }
}

public partial class LevelDesignWorkflow
{
    private class ObjectSelection : UIBaseWindow
    {
        private LevelDesignWorkflow _workFlow;

        public ObjectSelection(LevelDesignWorkflow workflow)
        {
            HandleInput = true;
            _workFlow = workflow;
        }

        public override void OnMouseEnter(Vector2 mousePos)
        {
            base.OnMouseEnter(mousePos);
            UpdateSelection();
        }

        public override void OnMouseMove(Vector2 mousePos)
        {
            base.OnMouseMove(mousePos);
            UpdateSelection();
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            base.OnMouseLeft(mousePos);
            UpdateSelection(true);
        }

        private void UpdateSelection(bool clear = false)
        {
            var mouseRay = Engine.Renderer.Camera.GetCameraMouseRay();
            var map = EngineEditor.GetCurrentMap();
            if (map == null) return; // huh?

            GameObject? newSel = null;

            if (!clear)
            {
                if (map.CollideWithRayFirst(mouseRay, out newSel))
                {
                    bool a = true;
                }
            }

            //newSel.
            _workFlow._objectUnderMouse = newSel;
        }
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

            IGenericReflectorTypeHandler[] gridTypes = ReflectorEngine.GetDescendantsOf<IMapGrid>();
            foreach (IGenericReflectorTypeHandler typeHandler in gridTypes)
            {
                EditorButton button = new EditorButton(typeHandler.TypeName);
                button.GrowX = true;
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