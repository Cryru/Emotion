#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;

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
        Engine.UI.AddChild(EditorRoot);

        UIBaseWindow barContainer = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        EditorRoot.AddChild(barContainer);
        barContainer.AddChild(new EditorTopBar());
        barContainer.AddChild(new MapEditorViewMode());

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

        string perfReadoutStr = $"<right>FPS: {PerformanceMetrics.FpsLastSecond}\nDCF: {PerformanceMetrics.DrawCallsLastFrame:00}";
        _perfText.Text = perfReadoutStr;
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