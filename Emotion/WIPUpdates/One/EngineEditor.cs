#region Using

using Emotion.Common.Input;
using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.Tools.GameDataTool;

#endregion

namespace Emotion.WIPUpdates.One;

public static partial class EngineEditor
{
    public static bool IsOpen { get; private set; }

    public static UIBaseWindow EditorRoot;

    private static UIRichText _perfText;

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

    private static void SetupDebugCameraUI(UIBaseWindow barContainer)
    {
        var container = new ContainerVisibleInEditorMode
        {
            Margins = new Primitives.Rectangle(10, 5, 0, 0),
            VisibleIn = MapEditorMode.TwoDee | MapEditorMode.ThreeDee
        };
        barContainer.AddChild(container);

        var editorWithLabel = new ObjectPropertyEditor(
            "View Game Camera",
            new BooleanEditor(),
            _debugCameraOptionOn,
            (newVal) => SetDebugCameraOption((bool)newVal)
        );
        EditorLabel label = editorWithLabel.Label;
        label.OutlineColor = Color.Black;
        label.OutlineSize = 2;
        label.FontSize = 23;
        container.AddChild(editorWithLabel);
    }

    private static void RenderGameCameraBound(RenderComposer c)
    {
        if (!_debugCameraOptionOn) return;

        CameraBase gameCam = _cameraOutsideEditor;
        if (gameCam == null) return;

        c.SetDepthTest(false);
        if (MapEditorMode == MapEditorMode.TwoDee || _cameraOutsideEditor is Camera2D)
        {
            Rectangle twoDee = gameCam.GetCameraView2D();
            c.RenderSprite(twoDee.Position, twoDee.Size, Color.Magenta * 0.1f);
            c.RenderOutline(twoDee.Position, twoDee.Size, Color.Magenta, 3);
        }
        
        if (MapEditorMode == MapEditorMode.ThreeDee)
        {
            Span<Vector3> frustumCorners = stackalloc Vector3[8];
            gameCam.GetCameraView3D(frustumCorners);

            Span<Vector3> sideA = stackalloc Vector3[4];
            Span<Vector3> sideB = stackalloc Vector3[4];
            Span<Vector3> sideNear = stackalloc Vector3[4];
            Span<Vector3> sideFar = stackalloc Vector3[4];

            CameraBase.GetCameraFrustumSidePlanes(frustumCorners, sideA, sideB);
            CameraBase.GetCameraFrustumNearAndFarPlanes(frustumCorners, sideNear, sideFar);

            c.RenderQuad(sideA, Color.Magenta * 0.1f);
            c.RenderQuad(sideB, Color.Magenta * 0.1f);
            c.RenderQuad(sideNear, Color.Magenta * 0.1f);
            c.RenderQuad(sideFar, Color.Magenta * 0.1f);

            c.RenderFrustum(frustumCorners, Color.White);
        }
        c.SetDepthTest(true);
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