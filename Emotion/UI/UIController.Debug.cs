using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.UI;

public partial class UIController
{
    public static void Debug_GetWindowsUnderMouse(List<UIBaseWindow> output)
    {
        output.Clear();
        var mousePos = Engine.Host.MousePosition;

        for (var c = 0; c < _allControllers.Count; c++) // Controllers are presorted in priority order.
        {
            UIController controller = _allControllers[c];

            // Controller inactive via update. (We check the older variable because this function is called in the update of the interface editor)
            if (!controller._calledUpdateTickBeforeLast) continue;
            if (controller.Children == null) continue;

            for (int i = controller.Children.Count - 1; i >= 0; i--) // Top to bottom
            {
                UIBaseWindow win = controller.Children[i];
                if (win.Visible && win.IsPointInside(mousePos))
                {
                    Debug_GetWindowsUnderMouseInner(win, mousePos, output);
                }
            }
        }
    }

    private static bool Debug_GetWindowsUnderMouseInner(UIBaseWindow win, Vector2 mousePos, List<UIBaseWindow> output)
    {
        if (win.Children == null)
        {
            output.Add(win);
            return true;
        }

        bool anyHandled = false;
        for (int i = win.Children.Count - 1; i >= 0; i--)
        {
            var child = win.Children[i];
            if (child.Visible && child.IsPointInside(mousePos))
            {
                anyHandled = true;
                Debug_GetWindowsUnderMouseInner(child, mousePos, output);
            }
        }

        if (!anyHandled)
        {
            output.Add(win);
            return true;
        }
        return false;
    }

    private static UIBaseWindow? _debugBPMeasure;
    private static bool _bpMeasureOnce = false;

    private static UIBaseWindow? _debugBPLayout;
    private static bool _bpLayoutOnce = false;

    public static UIBaseWindow? Debug_RenderLayoutEngine;

    public static void SetWindowBreakpointOnMeasure(UIBaseWindow? win, bool once = true)
    {
        _debugBPMeasure = win;
        _bpMeasureOnce = once;
    }

    public static void SetWindowBreakpointOnLayout(UIBaseWindow? win, bool once = true)
    {
        _debugBPLayout = win;
        _bpLayoutOnce = once;
    }

    public static void SetWindowVisualizeLayout(UIBaseWindow? win)
    {
        Debug_RenderLayoutEngine = win;
    }

    [Conditional("DEBUG")]
    public static void DebugShouldBreakpointMeasure(UIBaseWindow me)
    {
        if (me == _debugBPMeasure)
        {
            if (_bpMeasureOnce) _debugBPMeasure = null;
            Debugger.Break();
        }
    }

    [Conditional("DEBUG")]
    public static void DebugShouldBreakpointLayout(UIBaseWindow me)
    {
        if (me == _debugBPLayout)
        {
            if (_bpLayoutOnce) _debugBPLayout = null;
            Debugger.Break();
        }
    }
}
