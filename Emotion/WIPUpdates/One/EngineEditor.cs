#region Using

using Emotion.Common.Input;
using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.Tools.GameDataTool;
using System;
using System.Text;
using System.Xml.Linq;

#endregion

namespace Emotion.WIPUpdates.One;

#nullable enable
public static partial class EngineEditor
{
    public static bool IsOpen { get; private set; }

    public static UIBaseWindow EditorRoot = null!;

    private static UIRichText _perfText = null!;

    public static void Attach()
    {
        if (!Engine.Configuration.DebugMode) return;
        Engine.Host.OnKey.AddListener(EditorButtonHandler, KeyListenerType.Editor);
        EditorRoot = new UIBaseWindow()
        {
            Id = "EditorRoot"
        };
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

        Engine.UI.AddChild(EditorRoot);

        UIBaseWindow barContainer = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        EditorRoot.AddChild(barContainer);
        barContainer.AddChild(new EditorTopBar());
        barContainer.AddChild(new MapEditorViewMode());

        SetupDebugCameraUI(barContainer);
        SetupGameEditorVisualizations(barContainer);

        _perfText = new UIRichText
        {
            FontSize = 25,
            AnchorAndParentAnchor = UIAnchor.TopRight,
            OutlineColor = Color.Black,
            OutlineSize = 2,
            Margins = new Primitives.Rectangle(0, 50, 5, 0),
            AllowRenderBatch = false
        };
        EditorRoot.AddChild(_perfText);

        IsOpen = true;
        Engine.Log.Info($"Editor opened", "Editor");
    }

    public static void CloseEditor()
    {
        Engine.Input.SuppressMouseFirstPersonMode(false, "Editor");
        Engine.Host.OnKey.BlockListenersOfType(null);

        Engine.UI.RemoveChild(EditorRoot);
        EditorRoot.ClearChildren();
        SetMapEditorMode(MapEditorMode.Off);

        IsOpen = false;
        Engine.Log.Info($"Editor closed", "Editor");
    }

    public static void UpdateEditor()
    {
        if (!IsOpen) return;
        UpdateMapEditor();
    }

    public static void RenderEditor(RenderComposer c)
    {
        RenderEditorVisualizations(c);

        if (!IsOpen) return;
        RenderMapEditor(c);

        c.SetUseViewMatrix(true);
        RenderGameCameraBound(c);
        c.SetUseViewMatrix(false);

        string perfReadoutStr = $"<right>FPS: {PerformanceMetrics.FpsLastSecond}\nDCF: {PerformanceMetrics.DrawCallsLastFrame:00}";
        _perfText.Text = perfReadoutStr;
    }

    public static void OnSceneRenderStart(RenderComposer c)
    {
        if (IsDebugCameraOn()) c.DebugSetVirtualCamera(_cameraOutsideEditor);
    }

    public static void OnSceneRenderEnd(RenderComposer c)
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

    private static void SetupDebugCameraUI(UIBaseWindow barContainer)
    {
        var container = new ContainerVisibleInEditorMode
        {
            Margins = new Primitives.Rectangle(10, 5, 0, 0),
            VisibleIn = MapEditorMode.TwoDee | MapEditorMode.ThreeDee,
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5)
        };
        barContainer.AddChild(container);

        // View game camera
        UIBaseWindow viewGameCamera = TypeEditor.CreateCustomWithLabel("View Game Camera", _debugCameraOptionOn, SetDebugCameraOption, LabelStyle.MapEditor);
        container.AddChild(viewGameCamera);

        // Camera speed
        UIBaseWindow cameraSpeed = TypeEditor.CreateCustomWithLabel("Camera Speed", (float) 0, SetDebugCameraSpeed, LabelStyle.MapEditor);
        cameraSpeed.MaxSizeX = 220;
        container.OnModeChanged = (_) =>
        {
            // The speed changes when the camera changes, so we need to update it on mode changed.
            float currentSpeed = GetDebugCameraSpeed();
            var camSpeedTypeEditor = cameraSpeed.GetWindowById<TypeEditor>("Editor");
            if (camSpeedTypeEditor != null)
                camSpeedTypeEditor.SetValue(currentSpeed);
        };
        container.AddChild(cameraSpeed);
    }

    private static void RenderGameCameraBound(RenderComposer c)
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

    private class EditorVisualization
    {
        public object OwningObject;
        public string Name;
        public Action<RenderComposer> Function;
        public bool Enabled;
    }

    private class EditorVisualizationText
    {
        public object OwningObject;
        public Func<string> GetText;
    }

    private static List<EditorVisualization> _editorVisualization = new();
    private static List<EditorVisualizationText> _editorTextVisualization = new();
    private static EditorLabel? _textVisualization;

    public static void AddEditorVisualization(object owningObject, string name, Action<RenderComposer> func)
    {
        _editorVisualization.Add(new EditorVisualization()
        {
            OwningObject = owningObject,
            Name = name,
            Function = func
        });
    }

    public static void AddEditorVisualizationText(object owningObject, Func<string> getText)
    {
        _editorTextVisualization.Add(new EditorVisualizationText()
        {
            OwningObject = owningObject,
            GetText = getText
        });
    }

    public static void RemoveEditorVisualizations(object owningObject)
    {
        _editorVisualization.RemoveAll(x => x.OwningObject == owningObject);
    }

    private static void RenderEditorVisualizations(RenderComposer c)
    {
        // We render these visualizations regardless if the editor is open.
        foreach (EditorVisualization visualization in _editorVisualization)
        {
            if (!visualization.Enabled) continue;
            visualization.Function(c);
        }

        // This is really bad allocation wise, but its for debug so whatever
        if (_textVisualization != null && IsOpen && MapEditorMode == MapEditorMode.Off)
        {
            StringBuilder b = new StringBuilder();
            foreach (var visualization in _editorTextVisualization)
            {
                b.AppendLine(visualization.GetText());
            }
            _textVisualization.Text = b.ToString();
        }
    }

    private static void SetupGameEditorVisualizations(UIBaseWindow barContainer)
    {
        UIBaseWindow? oldContainer = barContainer.GetWindowById("GameEditorVisualizations");
        oldContainer?.Close();

        var container = new UIBaseWindow
        {
            Id = "GameEditorVisualizations",
            Margins = new Primitives.Rectangle(10, 15, 0, 0),
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5)
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
        foreach (EditorVisualization visualization in _editorVisualization)
        {
            // Assumes items with the same owning object follow each other.
            object owningObject = visualization.OwningObject;
            if (owningObject != lastOwningObject)
            {
                var objSeparator = EditorLabel.GetLabel(LabelStyle.MapEditor, owningObject.ToString() + "\n-------------");
                objSeparator.Margins = new Primitives.Rectangle(0, lastOwningObject != null ? 30 : 0, 0, 0);
                container.AddChild(objSeparator);
                lastOwningObject = owningObject;
            }

            UIBaseWindow visualizationCheckbox = TypeEditor.CreateCustomWithLabel(visualization.Name, visualization.Enabled, (val) => visualization.Enabled = val, LabelStyle.MapEditor);
            container.AddChild(visualizationCheckbox);
        }
    }

    #endregion

    #region Helpers

    public static void OpenToolWindowUnique(UIBaseWindow tool)
    {
        foreach (UIBaseWindow item in EditorRoot.WindowChildren())
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
        EditorRoot.AddChild(tool);
    }

    #endregion
}