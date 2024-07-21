#region Using

using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;

#endregion

namespace Emotion.WIPUpdates.One;

public static partial class EngineEditor
{
    public static bool IsOpen { get; private set; }

    public static UIBaseWindow EditorRoot;

    private static UIText _perfText;

    public static void Attach()
    {
        if (!Engine.Configuration.DebugMode) return;
        Engine.Host.OnKey.AddListener(EditorButtonHandler, KeyListenerType.Editor);
        EditorRoot = new UIBaseWindow()
        {
            Id = "EditorRoot"
        };
    }

    private static bool EditorButtonHandler(Key key, KeyStatus status)
    {
        if (key == Key.GraveAccent && status == KeyStatus.Down)
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
        Engine.UI.AddChild(EditorRoot);

        UIBaseWindow barContainer = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        EditorRoot.AddChild(barContainer);
        barContainer.AddChild(new EditorTopBar());
        barContainer.AddChild(new MapEditorViewMode());

        _perfText = new UIText();
        _perfText.FontSize = 25;
        _perfText.AnchorAndParentAnchor = UIAnchor.TopRight;
        _perfText.OutlineColor = Color.Black;
        _perfText.OutlineSize = 2;
        _perfText.Margins = new Primitives.Rectangle(0, 50, 5, 0);
        EditorRoot.AddChild(_perfText);

        IsOpen = true;
        Engine.Log.Info($"Editor opened", "Editor");
    }

    public static void CloseEditor()
    {
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

        string perfReadoutStr = $"FPS: {PerformanceMetrics.FpsLastSecond}\nDraw Calls: {PerformanceMetrics.DrawCallsLastFrame}\nUpdate Ahead:{Engine.CurrentUpdateFrame - Engine.CurrentRenderFrame}";
        c.RenderString(Vector3.Zero, Color.White, perfReadoutStr, FontAsset.GetDefaultBuiltIn().GetAtlas(30), null, Graphics.Text.FontEffect.Outline, 2f, Color.Black);
        //_perfText.Text = ;
    }

    #region Helpers

    public static void OpenToolWindowUnique(UIBaseWindow tool)
    {
        foreach (UIBaseWindow item in EditorRoot.WindowChildren())
        {
            if (item.GetType() == tool.GetType())
            {
                Engine.UI.SetInputFocus(item);
                return;
            }
        }
        EditorRoot.AddChild(tool);
    }

    #endregion
}