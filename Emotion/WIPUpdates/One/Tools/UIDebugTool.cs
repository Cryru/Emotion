using Emotion.Editor.EditorHelpers;
using Emotion.Platform.Input;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.Tools;

public class UIWindowDebugInfo
{
    public string Path;
    public string Id;
    public Rectangle Bounds;
    public Rectangle ScaledPadding;
}

public class UIDebugTool : EditorWindow
{
    private UIWindowDebugInfo _debugInfo;
    private UIBaseWindow? _debuggingWindow;

    public UIDebugTool() : base("UI Debug Tool")
    {
        _debugInfo = new UIWindowDebugInfo();
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        var container = new UIBaseWindow
        {
            LayoutMode = LayoutMode.VerticalList
        };
        contentParent.AddChild(container);

        var buttonContainer = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0)
        };
        container.AddChild(buttonContainer);

        var inspectButton = new EditorButton("Select Window");
        inspectButton.OnClickedProxy = (_) => Engine.UI.EnterInspectMode();
        buttonContainer.AddChild(inspectButton);

        var selectParent = new EditorButton("Select Parent");
        selectParent.OnClickedProxy = (_) =>
        {
            var window = _debuggingWindow;
            if (window == null) return;

            if (window.Parent == null) return;
            DebugWindow(window.Parent);
        };
        buttonContainer.AddChild(selectParent);

        var bpMeasure = new EditorButton("Breakpoint Window Measure");
        bpMeasure.OnClickedProxy = (_) =>
        {
            var window = _debuggingWindow;
            if (window == null) return;

            UIController.SetWindowBreakpointOnMeasure(window);
            window.InvalidateLayout();
        };
        buttonContainer.AddChild(bpMeasure);

        var bpLayout = new EditorButton("Breakpoint Window Layout");
        bpLayout.OnClickedProxy = (_) =>
        {
            var window = _debuggingWindow;
            if (window == null) return;

            UIController.SetWindowBreakpointOnLayout(window);
            window.InvalidateLayout();
        };
        buttonContainer.AddChild(bpLayout);

        //var bpVisualize = new EditorButton("Visualize Window Layout");
        //bpVisualize.OnClickedProxy = (_) =>
        //{
        //    var window = _windowsUnderMouse[^1];
        //    UIController.Debug_RenderLayoutEngine = window;
        //    window.InvalidateLayout();
        //};
        //buttonContainer.AddChild(bpVisualize);

        var properties = new GenericPropertiesEditorPanel(_debugInfo)
        {
            PanelMode = PanelMode.Embedded,
            OnPropertyEdited = (propertyName, oldValue) =>
            {

            },
        };
        container.AddChild(properties);
    }

    protected override bool UpdateInternal()
    {
        var underMouse = Engine.UI.GetInspectModeSelectedWindow();
        if (underMouse != null)
        {
            for (int i = underMouse.Count - 1; i >= 0; i--)
            {
                var win = underMouse[i];
                if (win.IsWithin(this))
                {
                    underMouse.RemoveAt(i);
                }
            }

            if (underMouse.Count > 0)
            {
                UIBaseWindow windowUnderMouse = underMouse[^1];
                DebugWindow(windowUnderMouse);
            }
        }

        return base.UpdateInternal();
    }

    private void DebugWindow(UIBaseWindow dbgWindow)
    {
        _debuggingWindow = dbgWindow;
        _debugInfo.Path = GetWindowPath(dbgWindow);
        _debugInfo.Id = dbgWindow.Id;
        _debugInfo.Bounds = dbgWindow.Bounds;
        _debugInfo.ScaledPadding = dbgWindow.Paddings * dbgWindow.GetScale();
    }

    private string GetWindowPath(UIBaseWindow win)
    {
        UIBaseWindow cur = win;
        StringBuilder pathBuild = new StringBuilder();
        while (cur != null)
        {
            if (cur.Id != null)
            {
                pathBuild.Insert(0, cur.Id);
            }
            else
            {
                var parent = cur.Parent;
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(cur);
                    pathBuild.Insert(0, index);
                }
                else
                {
                    pathBuild.Insert(0, cur.GetType().Name);
                }
            }

            pathBuild.Insert(0, "\\");

            cur = cur.Parent;
        }

        return pathBuild.ToString();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        return base.RenderInternal(c);
    }
}
