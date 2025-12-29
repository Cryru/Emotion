#nullable enable

using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Workflow;

namespace Emotion.Editor;

public static partial class EngineEditor
{
    public class DefaultWorkflow : EditorWorkflow
    {
        public override string Name => "Default";

        private static EditorLabel? _textVisualization;

        public override void Init(UIBaseWindow parent)
        {
            var cont = new UIBaseWindow()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(0)
                }
            };
            parent.AddChild(cont);

            cont.AddChild(new MapEditorViewMode());
            SetupDebugCameraUI(cont);
            SetupGameEditorVisualizations(cont);
        }

        public override void Done()
        {

        }

        public override void Update()
        {
            base.Update();

            // This is really bad allocation wise, but its for debug so whatever
            if (_textVisualization != null && IsOpen && MapEditorMode == MapEditorMode.Off)
            {
                string? text = GetEditorTextDebugVisualizations();
                if (text != null)
                    _textVisualization.Text = text;
            }
        }

        private static void SetupDebugCameraUI(UIBaseWindow barContainer)
        {
            var container = new ContainerVisibleInEditorMode
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(5),
                    Margins = new UISpacing(10, 5, 0, 0),
                    SizingY = UISizing.Fit(),
                },
                Visuals =
                {
                    DontTakeSpaceWhenHidden = true
                },

                VisibleIn = MapEditorMode.TwoDee | MapEditorMode.ThreeDee,
            };
            barContainer.AddChild(container);

            // View game camera
            UIBaseWindow viewGameCamera = TypeEditor.CreateCustomWithLabel("View Game Camera", _debugCameraOptionOn, SetDebugCameraOption, LabelStyle.MapEditor);
            container.AddChild(viewGameCamera);

            // Camera speed
            UIBaseWindow cameraSpeed = TypeEditor.CreateCustomWithLabel("Camera Speed", (float)0, SetDebugCameraSpeed, LabelStyle.MapEditor);
            cameraSpeed.Layout.MaxSizeX = 220;
            container.OnModeChanged = (_) =>
            {
                // The speed changes when the camera changes, so we need to update it on mode changed.
                float currentSpeed = EngineEditor.GetDebugCameraSpeed();
                var camSpeedTypeEditor = cameraSpeed.GetWindowById<TypeEditor>("Editor");
                camSpeedTypeEditor?.SetValue(currentSpeed);
            };
            container.AddChild(cameraSpeed);
        }

        private static void SetupGameEditorVisualizations(UIBaseWindow barContainer)
        {
            UIBaseWindow? oldContainer = barContainer.GetWindowById("GameEditorVisualizations");
            oldContainer?.Close();

            var container = new UIBaseWindow
            {
                Name = "GameEditorVisualizations",
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(5),
                    Margins = new UISpacing(10, 15, 0, 0),
                    SizingY = UISizing.Fit()
                }
            };
            barContainer.AddChild(container);

            var textVisualizationContainer = new ContainerVisibleInEditorMode
            {
                VisibleIn = MapEditorMode.Off
            };
            container.AddChild(textVisualizationContainer);
            EditorLabel textVisualization = EditorLabel.GetLabel(LabelStyle.MapEditor, "");

            textVisualizationContainer.AddChild(textVisualization);
            _textVisualization = textVisualization;

            object? lastOwningObject = null;
            foreach (EditorVisualizationBase visualization in _editorVisualization)
            {
                // Assumes items with the same owning object follow each other.
                object owningObject = visualization.OwningObject;
                if (owningObject != lastOwningObject)
                {
                    EditorLabel objSeparator = EditorLabel.GetLabel(LabelStyle.MapEditor, owningObject.ToString() + "\n-------------");
                    objSeparator.Layout.Margins = new UISpacing(0, lastOwningObject != null ? 30 : 0, 0, 0);
                    container.AddChild(objSeparator);
                    lastOwningObject = owningObject;
                }

                UIBaseWindow visualizationCheckbox = TypeEditor.CreateCustomWithLabel(visualization.Name, visualization.Enabled, (val) => visualization.Enabled = val, LabelStyle.MapEditor);
                container.AddChild(visualizationCheckbox);
            }
        }
    }
}