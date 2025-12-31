#nullable enable

#region Using

using Emotion.Core.Utility.Profiling;
using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Tools.GameDataTool;
using Emotion.Editor.Workflow;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Text;
using System.Text;

#endregion

namespace Emotion.Editor;

public static partial class EngineEditor
{
    public static bool IsOpen { get; private set; }

    public static UIBaseWindow EditorUI { get; private set; } = null!;

    private static UIText _perfText = null!;

    private static UIBaseWindow _editorUIHorizontalParent = null!;
    private static UIBaseWindow? _editorBars;
    private static UIBaseWindow? _workflowParent;

    public static EditorWorkflow? CurrentWorkflow { get; private set; } = null;

    public static void Initialize()
    {
        if (!Engine.Configuration.DebugMode) return;
        Engine.Host.OnKey.AddListener(EditorButtonHandler, KeyListenerType.Editor);
        EditorUI = new UIBaseWindow()
        {
            Name = "EditorRoot",
            OrderInParent = 999
            
        };
        Engine.UI.AddChild(EditorUI);

        _editorUIHorizontalParent = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(0)
            }
        };
        EditorUI.AddChild(_editorUIHorizontalParent);
    }

    private static bool EditorButtonHandler(Key key, KeyState status)
    {
        if (key == Key.GraveAccent && status == KeyState.Down)
        {
            if (IsOpen)
                CloseEditor();
            else
                OpenEditor();
            return false;
        }

        return true;
    }

    public static void OpenEditor()
    {
        Engine.Input.SuppressMouseFirstPersonMode(true, "Editor");
        Engine.Host.OnKey.BlockListenersOfType(KeyListenerType.Game);

        _editorBars = new UIBaseWindow();
        _editorUIHorizontalParent.AddChild(_editorBars);

        UIBaseWindow barContainer = new()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(0)
            }
        };
        _editorBars.AddChild(barContainer);
        barContainer.AddChild(new EditorTopBar());

        _workflowParent = new UIBaseWindow()
        {
            Layout =
            {
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Grow(),
            },
            OrderInParent = 10
        };
        barContainer.AddChild(_workflowParent);
        SetWorkflow(new DefaultWorkflow());

        _perfText = new UIText
        {
            Layout =
            {
                AnchorAndParentAnchor = UIAnchor.TopRight,
                Margins = new UISpacing(0, 5, 5, 0)
            },

            FontSize = 25,
            Effect = TextEffect.Outline(Color.Black, 2),
            OrderInParent = 100,
        };
        _editorBars.AddChild(_perfText);

        IsOpen = true;
        Engine.Log.Info($"Editor opened", "Editor");
    }

    public static void CloseEditor()
    {
        Engine.Input.SuppressMouseFirstPersonMode(false, "Editor");
        Engine.Host.OnKey.BlockListenersOfType(null);

        AssertNotNull(_editorBars);
        _editorUIHorizontalParent.RemoveChild(_editorBars);
        _editorBars = null;
        _workflowParent = null;

        SetMapEditorMode(MapEditorMode.Off);

        IsOpen = false;
        Engine.Log.Info($"Editor closed", "Editor");
    }

    public static void UpdateEditor()
    {
        if (!IsOpen) return;
        UpdateMapEditor();
        CurrentWorkflow?.Update();
    }

    public static void RenderEditor(Renderer c)
    {
        RenderEditorVisualizations(c);

        if (!IsOpen) return;
        RenderMapEditor(c);

        c.SetUseViewMatrix(true);
        RenderGameCameraBound(c);
        CurrentWorkflow?.Render(c);
        c.SetUseViewMatrix(false);

        string perfReadoutStr = $"<right>FPS: {PerformanceMetrics.FpsLastSecond}\nDCF: {PerformanceMetrics.DrawCallsLastFrame:00}";
        _perfText.Text = perfReadoutStr;
    }

    public static void OnSceneRenderStart(Renderer c)
    {
        if (IsDebugCameraOn()) c.DebugSetVirtualCamera(_cameraOutsideEditor);
    }

    public static void OnSceneRenderEnd(Renderer c)
    {
        if (IsDebugCameraOn()) c.DebugSetVirtualCamera(null);
    }

    #region Debug Camera

    private static bool _debugCameraOptionOn;

    public static bool IsDebugCameraOn()
    {
        return _debugCameraOptionOn && (MapEditorMode == MapEditorMode.TwoDee || MapEditorMode == MapEditorMode.ThreeDee);
    }

    private static void SetDebugCameraOption(bool on)
    {
        _debugCameraOptionOn = on;
    }

    private static void SetDebugCameraSpeed(float speed)
    {
        speed = Maths.Clamp(speed, 0, 20);

        if (_editorCamera is Camera2D cam2D)
            cam2D.MovementSpeed = speed;
        else if (_editorCamera is Camera3D cam3D)
            cam3D.MovementSpeed = speed;
    }

    private static float GetDebugCameraSpeed()
    {
        return _editorCamera is Camera2D cam2D ? cam2D.MovementSpeed : (_editorCamera is Camera3D cam3D ? cam3D.MovementSpeed : 0f);
    }

    private static void RenderGameCameraBound(Renderer c)
    {
        if (!_debugCameraOptionOn) return;

        CameraBase? gameCam = _cameraOutsideEditor;
        if (gameCam == null) return;

        c.SetDepthTest(false);
        if (MapEditorMode == MapEditorMode.TwoDee || _cameraOutsideEditor is Camera2D)
        {
            Rectangle twoDee = gameCam.GetCameraView2D();
            c.RenderSprite(twoDee.Position, twoDee.Size, Color.Magenta * 0.1f);
            c.RenderRectOutline(twoDee.Position, twoDee.Size, Color.Magenta, 3);
        }

        if (MapEditorMode == MapEditorMode.ThreeDee)
        {
            Frustum frustum = gameCam.GetCameraView3D();
            frustum.Render(c, Color.White, Color.Magenta * 0.1f);
        }
        c.SetDepthTest(true);
    }

    #endregion

    #region Game Debug Hooks

    private class EditorVisualizationBase(object owner, string name)
    {
        public object OwningObject = owner;
        public string Name = name;
        public virtual bool Enabled { get; set; }
    }

    private class EditorVisualizationRender : EditorVisualizationBase
    {
        public Action<Renderer> RenderFunc;

        public EditorVisualizationRender(object owner, string name, Action<Renderer> onRender) : base(owner, name)
        {
            RenderFunc = onRender;
        }
    }

    private class EditorVisualizationBoolean : EditorVisualizationBase
    {
        public override bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                ChangeFunc(value);
            }
        }
        private bool _enabled;

        public Action<bool> ChangeFunc;

        public EditorVisualizationBoolean(object owner, string name, Action<bool> onChange) : base(owner, name)
        {
            ChangeFunc = onChange;
        }
    }

    private class EditorVisualizationText(object owner, Func<string> getText)
    {
        public object OwningObject = owner;
        public Func<string> GetText = getText;
    }

    private static List<EditorVisualizationBase> _editorVisualization = new();
    private static List<EditorVisualizationText> _editorTextVisualization = new();

    public static void AddEditorVisualization(object owningObject, string name, Action<Renderer> func)
    {
        _editorVisualization.Add(new EditorVisualizationRender(owningObject, name, func));
    }

    public static void AddEditorVisualizationBool(object owningObject, string name, Action<bool> func)
    {
        _editorVisualization.Add(new EditorVisualizationBoolean(owningObject, name, func));
    }

    public static void AddEditorVisualizationText(object owningObject, Func<string> getText)
    {
        _editorTextVisualization.Add(new EditorVisualizationText(owningObject, getText));
    }

    public static void RemoveEditorVisualizations(object owningObject)
    {
        _editorVisualization.RemoveAll(x => x.OwningObject == owningObject);
    }

    private static void RenderEditorVisualizations(Renderer c)
    {
        // We render these visualizations regardless if the editor is open.
        foreach (EditorVisualizationBase visualization in _editorVisualization)
        {
            if (!visualization.Enabled) continue;
            if (visualization is EditorVisualizationRender visRender)
                visRender.RenderFunc(c);
        }
    }

    private static string? GetEditorTextDebugVisualizations()
    {
        StringBuilder? b = _editorTextVisualization.Count > 0 ? new StringBuilder() : null;
        foreach (EditorVisualizationText visualization in _editorTextVisualization)
        {
            b!.AppendLine(visualization.GetText());
        }
        return b?.ToString();
    }

    #endregion

    #region Helpers

    public static void OpenToolWindowUnique(UIBaseWindow tool)
    {
        foreach (UIBaseWindow item in EditorUI.Children)
        {
            if (item.GetType() == tool.GetType())
            {
                // Different types of game data editor are allowed to be open at the same time.
                // todo: cleaner
                if (tool is GameDataEditor tgd && item is GameDataEditor igd && tgd.GameDataType != igd.GameDataType)
                    continue;

                Engine.UI.SetInputFocus(item);
                return;
            }
        }
        EditorUI.AddChild(tool);
    }

    #endregion

    #region Workflow

    public static void SetWorkflow(EditorWorkflow? workflow)
    {
        AssertNotNull(_workflowParent);
        _workflowParent.ClearChildren();
        CurrentWorkflow?.Done();
        CurrentWorkflow = workflow;
        workflow?.Init(_workflowParent);
    }

    #endregion
}