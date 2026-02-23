#nullable enable

using Emotion.Editor.Editor3D.TerrainEditor;
using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.World.Terrain;
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

        UIBaseWindow controls = new UIBaseWindow();
        parent.AddChild(controls);

        controls.AddChild(new MapGridDisplay(this));
        //controls.AddChild(new ObjectSelection(this));

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
    private class TerrainMeshBrushOverlay : UIBaseWindow
    {
        private LevelDesignWorkflow _workFlow;
        private static float _brushSize = 2;
        private static float _brushStr = 0.75f;

        public TerrainMeshBrushOverlay(LevelDesignWorkflow workflow)
        {
            HandleInput = true;
            _workFlow = workflow;

            var controls = new UIBaseWindow()
            {
                Layout =
                {
                    Margins = new UISpacing(0, 50, 0, 0),
                    LayoutMethod = UILayoutMethod.VerticalList(1)
                }
            };
            AddChild(controls);

            UIBaseWindow brushSize = TypeEditor.CreateCustomWithLabel("Brush Size", _brushSize, static (v) => _brushSize = v, LabelStyle.MapEditor);
            brushSize.Layout.SizingX = UISizing.Fit();
            controls.AddChild(brushSize);

            UIBaseWindow brushStr = TypeEditor.CreateCustomWithLabel("Brush Strength", _brushStr, static (v) => _brushStr = v, LabelStyle.MapEditor);
            brushStr.Layout.SizingX = UISizing.Fit();
            controls.AddChild(brushStr);
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
            GameMap? map = EngineEditor.GetCurrentMap();
            TerrainMeshGridNew? terrainGrid = map?.GetFirstGridOfType<TerrainMeshGridNew>();
            if (terrainGrid == null) return;

            terrainGrid.SetEditorBrush(!clear, _brushSize, _brushStr);
            if (clear) return;
        }

        protected override bool UpdateInternal()
        {
            if (_mouseDown)
            {
                GameMap? map = EngineEditor.GetCurrentMap();
                TerrainMeshGridNew? terrainGrid = map?.GetFirstGridOfType<TerrainMeshGridNew>();
                if (terrainGrid == null) return true;

                terrainGrid.ApplyBrushHeight(
                   Engine.Host.IsAltModifierHeld() ?
                       TerrainMeshGridNew.BrushOperation.Lower :
                       TerrainMeshGridNew.BrushOperation.Rise
               );
            }

            return base.UpdateInternal();
        }

        private bool _mouseDown;

        public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft)
            {
                _mouseDown = status == KeyState.Down;
                return true;
            }

            return base.OnKey(key, status, mousePos);
        }
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

            //NewUIScrollArea layerList = new NewUIScrollArea()
            //{
            //    Layout =
            //    {
            //        LayoutMethod = UILayoutMethod.VerticalList(1)
            //    }
            //};
            //AddChild(layerList);
            //_layerList = layerList;

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
                "(temp) Old Terrain Editor",
                (_) =>
                {
                    Parent.AddChild(new TerrainMeshBrushOverlay(_workFlow));
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