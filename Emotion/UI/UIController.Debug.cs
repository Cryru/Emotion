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

        List<(UIBaseWindow, int)> outputWithDepth = new List<(UIBaseWindow, int)>();

        var children = Engine.UI.Children;
        for (int i = children.Count - 1; i >= 0; i--) // Top to bottom
        {
            UIBaseWindow win = children[i];
            if (win.Visible && win.IsPointInside(mousePos))
            {
                Debug_GetWindowsUnderMouseInner(win, mousePos, outputWithDepth, 0);
            }
        }

        outputWithDepth.Sort((x, y) => MathF.Sign(x.Item2 - y.Item2));
        for (int i = 0; i < outputWithDepth.Count; i++)
        {
            var window = outputWithDepth[i].Item1;
            output.Add(window);
        }
    }

    private static bool Debug_GetWindowsUnderMouseInner(UIBaseWindow win, Vector2 mousePos, List<(UIBaseWindow, int)> output, int depth)
    {
        if (win.Children == null)
        {
            output.Add((win, depth));
            return true;
        }

        bool anyHandled = false;
        for (int i = win.Children.Count - 1; i >= 0; i--)
        {
            var child = win.Children[i];
            if (child.Visible && child.IsPointInside(mousePos))
            {
                anyHandled = true;
                Debug_GetWindowsUnderMouseInner(child, mousePos, output, depth + 1);
            }
        }

        if (!anyHandled)
        {
            output.Add((win, depth));
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
