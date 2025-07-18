﻿#region Using

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

        UIBaseWindow viewGameCamera = TypeEditor.CreateCustomWithLabel("View Game Camera", _debugCameraOptionOn, SetDebugCameraOption, LabelStyle.MapEditor);

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

        container.AddChild(viewGameCamera);
        container.AddChild(cameraSpeed);
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