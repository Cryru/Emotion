using Emotion.Common.Input;
using Emotion.Editor.EditorHelpers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Editor.EditorWindows;

public class UIWindowDebugInfo
{
    public string Path;
    public string Id;
}

public class InterfaceEditor : EditorPanel
{
    private UIWindowDebugInfo _debugInfo;

    private bool _inspecting;
    private List<UIBaseWindow> _windowsUnderMouse;

    public InterfaceEditor() : base("UI Studio")
    {
        _debugInfo = new UIWindowDebugInfo();
        _windowsUnderMouse = new List<UIBaseWindow>();
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var container = new UIBaseWindow();
        container.LayoutMode = LayoutMode.VerticalList;
        _contentParent.AddChild(container);

        var buttonContainer = new UIBaseWindow();
        buttonContainer.LayoutMode = LayoutMode.HorizontalList;
        buttonContainer.StretchX = true;
        buttonContainer.StretchY = true;
        buttonContainer.ListSpacing = new Vector2(2, 0);
        container.AddChild(buttonContainer);

        var inspectButton = new EditorButton("Select Window");
        inspectButton.OnClickedProxy = (_) =>
        {
            _inspecting = true;
            HandleInput = true;
            OrderInParent = 999;
        };
        buttonContainer.AddChild(inspectButton);

        var bpMeasure = new EditorButton("Breakpoint Window Measure");
        bpMeasure.OnClickedProxy = (_) =>
        {
            var window = _windowsUnderMouse[^1];
            UIController.SetWindowBreakpointOnMeasure(window);
            window.InvalidateLayout();
        };
        buttonContainer.AddChild(bpMeasure);

        var bpLayout = new EditorButton("Breakpoint Window Layout");
        bpLayout.OnClickedProxy = (_) =>
        {
            var window = _windowsUnderMouse[^1];
            UIController.SetWindowBreakpointOnLayout(window);
            window.InvalidateLayout();
        };
        buttonContainer.AddChild(bpLayout);

        var bpVisualize = new EditorButton("Visualize Window Layout");
        bpVisualize.OnClickedProxy = (_) =>
        {
            var window = _windowsUnderMouse[^1];
            UIController.Debug_RenderLayoutEngine = window;
            window.InvalidateLayout();
        };
        buttonContainer.AddChild(bpVisualize);

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
        if (_inspecting)
        {
            UIController.Debug_GetWindowsUnderMouse(_windowsUnderMouse);
            for (int i = _windowsUnderMouse.Count - 1; i >= 0; i--)
            {
                var win = _windowsUnderMouse[i];
                if (win.IsWithin(this))
                {
                    _windowsUnderMouse.RemoveAt(i);
                }
            }

            if (_windowsUnderMouse.Count > 0)
            {
                var windowUnderMouse = _windowsUnderMouse[^1];

                _debugInfo.Path = GetWindowPath(windowUnderMouse);
                _debugInfo.Id = windowUnderMouse.Id;
            }
        }

        return base.UpdateInternal();
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

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        bool returnVal = base.OnKey(key, status, mousePos);

        if (key == Key.MouseKeyLeft && status == KeyState.Down)
        {
            if (_inspecting) _inspecting = false;
        }

        return returnVal;
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        if (_inspecting && _windowsUnderMouse.Count > 0)
        {
            var windowUnderMouse = _windowsUnderMouse[^1];
            c.RenderOutline(windowUnderMouse.Position, windowUnderMouse.Size, Color.Red);
        }

        if (_inspecting) return false;

        return base.RenderInternal(c);
    }
}
