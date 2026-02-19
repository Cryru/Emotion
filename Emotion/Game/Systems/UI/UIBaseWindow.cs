#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Time;
using Emotion.Standard.Parsers.XML;

#endregion

namespace Emotion.Game.Systems.UI;

[DontSerializeMembers("Position", "Size")]
public partial class UIBaseWindow : IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
{
    #region Layout

    /// <summary>
    /// Whether this window is the background of its parent.
    /// </summary>
    public bool BackgroundWindow
    {
        get => _backgroundWindow;
        set
        {
            if (value == _backgroundWindow) return;
            _backgroundWindow = value;
            InvalidateLayout();
        }
    }

    protected bool _backgroundWindow;

    /// <summary>
    /// Whether this window should be rendered on top of all other windows.
    /// </summary>
    public bool OverlayWindow
    {
        get => _overlayWindow;
        set
        {
            if (value == _overlayWindow) return;
            _overlayWindow = value;
            InvalidateLayout();
        }
    }

    protected bool _overlayWindow;

    /// <summary>
    /// Position relative to another window in the same controller.
    /// </summary>
    public string? RelativeTo
    {
        get => _relativeTo;
        set
        {
            if (value == _relativeTo) return;
            _relativeTo = value;
            InvalidateLayout();
        }
    }

    private string? _relativeTo;

    [Obsolete("delete")]
    protected virtual void DELETEME_AfterLayout()
    {
        // nop - to be overriden
    }

    #endregion

    #region Color

    /// <summary>
    /// This color should be mixed in with the rendering of the window somehow.
    /// Used to control opacity as well.
    /// </summary>
    [Obsolete("Visuals.BackgroundColor")]
    public Color WindowColor;

    /// <summary>
    /// If set to true then this window will not mix its color with its parent's.
    /// </summary>
    public bool IgnoreParentColor = false;

    protected Color _calculatedColor;

    protected Coroutine? _alphaRoutine;
    private ValueTimer _alphaTimer;

    private IEnumerator AlphaTweenRoutine(byte startingAlpha, byte targetAlpha, bool? setVisible)
    {
        while (true)
        {
            _alphaTimer.Update(Engine.DeltaTime);
            var current = (byte) Maths.Lerp(startingAlpha, targetAlpha, _alphaTimer.GetFactor());
            Visuals.BackgroundColor = Visuals.BackgroundColor.SetAlpha(current);

            // If fading in we need to set visible from the get go.
            if (setVisible != null && setVisible.Value && !Visible) Visible = true;

            if (_alphaTimer.Finished)
            {
                Assert(Visuals.BackgroundColor.A == targetAlpha);
                if (setVisible != null) Visible = setVisible.Value;
                yield break;
            }

            yield return null;
        }
    }

    public void SetAlpha(byte value, float ms = 0)
    {
        Engine.CoroutineManager.StopCoroutine(_alphaRoutine);
        _alphaTimer.Reset();

        if (ms == 0)
        {
            Visuals.BackgroundColor = Visuals.BackgroundColor.SetAlpha(value);
            return;
        }

        if (Visuals.BackgroundColor.A == value)
        {
            _alphaRoutine = null;
            return;
        }

        _alphaTimer = new ValueTimer(ms);
        _alphaRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(Visuals.BackgroundColor.A, value, null));
    }

    public IRoutineWaiter SetVisible(bool val, float ms = 0)
    {
        Engine.CoroutineManager.StopCoroutine(_alphaRoutine);

        var targetAlpha = (byte) (val ? 255 : 0);
        if (ms == 0)
        {
            Visuals.BackgroundColor = Visuals.BackgroundColor.SetAlpha(targetAlpha);
            Visuals.Visible = val;
            return Coroutine.CompletedRoutine;
        }

        if (Visible == val && WindowColor.A == targetAlpha)
        {
            _alphaRoutine = null;
            return Coroutine.CompletedRoutine;
        }

        _alphaTimer = new ValueTimer(ms);
        _alphaRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(Visuals.BackgroundColor.A, targetAlpha, Visible == val ? null : val));
        return _alphaRoutine;
    }

    #endregion

    public virtual void InputFocusChanged(bool haveFocus)
    {
    }

    #region Rollover Functionality

    public virtual UIRollover? GetRollover()
    {
        return null;
    }

    #endregion
}