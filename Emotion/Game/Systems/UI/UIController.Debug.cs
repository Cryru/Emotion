#nullable enable

using Emotion.Editor.Tools;
using System.Linq;

namespace Emotion.Game.Systems.UI;

public partial class UIController
{
    

    private static UIBaseWindow? _debugBPMeasure;
    private static bool _bpMeasureOnce = false;

    private static UIBaseWindow? _debugBPLayout;
    private static bool _bpLayoutOnce = false;

    private static UIBaseWindow? _debugBPMeasureAfterChildren;
    private static UIBaseWindow? _debugBPLayoutAfterChildren;

    private static UIDebugTool? _debugTool;

    public static UIBaseWindow? Debug_RenderLayoutEngine;

    public static void SetUIDebugTool(UIDebugTool? debugTool)
    {
        _debugTool = debugTool;
    }

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

    public static void SetWindowBreakpointOnMeasureAfterChildren(UIBaseWindow? win)
    {
        _debugBPMeasureAfterChildren = win;
    }

    public static void SetWindowBreakpointOnLayoutAfterChildren(UIBaseWindow? win, bool once = true)
    {
        _debugBPLayoutAfterChildren = win;
    }

    public static void SetWindowVisualizeLayout(UIBaseWindow? win)
    {
        Debug_RenderLayoutEngine = win;
    }

    [Conditional("DEBUG")]
    [DebuggerHidden]
    public static void DebugShouldBreakpointMeasure(UIBaseWindow me)
    {
        if (me == _debugBPMeasure)
        {
            if (_bpMeasureOnce) _debugBPMeasure = null;
            Debugger.Break();
        }
    }

    [Conditional("DEBUG")]
    [DebuggerHidden]
    public static void DebugShouldBreakpointLayout(UIBaseWindow me)
    {
        if (me == _debugBPLayout)
        {
            if (_bpLayoutOnce) _debugBPLayout = null;
            Debugger.Break();
        }
    }

    [Conditional("DEBUG")]
    [DebuggerHidden]
    public static void DebugShouldBreakpointMeasureAfterChildren(UIBaseWindow me)
    {
        if (me == _debugBPMeasureAfterChildren)
        {
            _debugBPMeasureAfterChildren = null;
            Debugger.Break();
        }
    }

    [Conditional("DEBUG")]
    [DebuggerHidden]
    public static void DebugShouldBreakpointLayoutAfterChildren(UIBaseWindow me)
    {
        if (me == _debugBPLayoutAfterChildren)
        {
            _debugBPLayoutAfterChildren = null;
            Debugger.Break();
        }
    }
}
