#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.Parsers.XML;

#endregion

namespace Emotion.Game.Systems.UI;

[DontSerializeMembers("Position", "Size")]
public partial class UIBaseWindow : IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
{
    public static float DefaultMaxSizeF = 9999;

    /// <summary>
    /// By default windows greedily take up all the size they can.
    /// </summary>
    public static Vector2 DefaultMaxSize = new(DefaultMaxSizeF, DefaultMaxSizeF);

    #region Loading, Update, Render

    private Task _loadingTask = Task.CompletedTask;
    private bool _needsLoad = true;

    //public bool IsLoading()
    //{
    //    return !_loadingTask.IsCompleted || _needsLoad;
    //}

    public void CheckLoadContent(UILoadingContext ctx)
    {
        // Add to loading only if not currently loading.
        lock (this)
        {
            if (_loadingTask.IsCompleted)
            {
                _loadingTask = LoadContent();
                // if (!_loadingTask.IsCompleted) Engine.Log.Trace(ToString(), "UI Loading");
                ctx.AddLoadingTask(_loadingTask);
                _needsLoad = false;
            }
        }

        if (Children == null) return;
        for (var i = 0; i < Children.Count; i++)
        {
            UIBaseWindow child = Children[i];
            child.CheckLoadContent(ctx);
        }
    }

    protected virtual void InvalidateLoaded()
    {
        //_needsLoad = true;
        //Controller?.InvalidatePreload();
    }

    protected virtual Task LoadContent()
    {
        return Task.CompletedTask;
    }

   

   



    #endregion

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
    /// How to layout the children of this window.
    /// </summary>
    public LayoutMode LayoutMode { get; set; } = LayoutMode.Free;

    /// <summary>
    /// Spacing if the LayoutMode is a list.
    /// </summary>
    public Vector2 ListSpacing
    {
        get => _listSpacing;
        set
        {
            if (value == _listSpacing) return;
            _listSpacing = value;
            InvalidateLayout();
        }
    }

    private Vector2 _listSpacing;






    /// <summary>
    /// Margins push the window in one of the four directions, only if it is against another window.
    /// This is applied after alignment, but before the anchor.
    /// </summary>
    public Rectangle Margins
    {
        get => _margins;
        set
        {
            if (_margins == value) return;
            _margins = value;
            InvalidateLayout();
        }
    }

    private Rectangle _margins;

    /// <summary>
    /// Paddings push children windows if they are inside the parent.
    /// </summary>
    public Rectangle Paddings
    {
        get => _paddings;
        set
        {
            if (_paddings == value) return;
            _paddings = value;
            InvalidateLayout();
        }
    }

    private Rectangle _paddings;

    /// <summary>
    /// The minimum size the window can be.
    /// </summary>
    public Vector2 MinSize
    {
        get => new(MinSizeX, MinSizeY);
        set
        {
            MinSizeX = value.X;
            MinSizeY = value.Y;
        }
    }

    [DontSerialize] // will be saved via the Vector2 prop
    public float MinSizeX;

    [DontSerialize] //
    public float MinSizeY;

    [DontSerialize] //
    public float MaxSizeY = DefaultMaxSizeF;

    /// <summary>
    /// Always added to the position after all other checks.
    /// Mostly used to offset from an anchor position.
    /// </summary>
    public Vector2 Offset
    {
        get => _offsetBacking;
        set
        {
            if (value == _offsetBacking) return;
            _offsetBacking = value;
            InvalidateLayout();
        }
    }

    private Vector2 _offsetBacking;

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

    protected virtual void AfterMeasure(Vector2 contentSize)
    {
    }

    protected virtual void AfterLayout()
    {
        // nop - to be overriden
    }

    #endregion

    #region Color

    /// <summary>
    /// This color should be mixed in with the rendering of the window somehow.
    /// Used to control opacity as well.
    /// </summary>
    public Color WindowColor
    {
        get => _windowColor;
        set
        {
            if (_windowColor == value) return;
            _windowColor = value;
            InvalidateColor();
        }
    }

    private Color _windowColor = Color.White;

    /// <summary>
    /// If set to true then this window will not mix its color with its parent's.
    /// </summary>
    public bool IgnoreParentColor = false;

    protected Color _calculatedColor;
    protected bool _updateColor = true;

    public void InvalidateColor()
    {
        _updateColor = true;
        if (Children == null) return;
        for (var i = 0; i < Children.Count; i++)
        {
            UIBaseWindow child = Children[i];
            child.InvalidateColor();
        }
    }

    protected Coroutine? _alphaTweenRoutine;
    private ITimer? _alphaTweenTimer;

    private IEnumerator AlphaTweenRoutine(ITimer alphaTween, byte startingAlpha, byte targetAlpha, bool? setVisible)
    {
        while (true)
        {
            alphaTween.Update(Engine.DeltaTime);
            var current = (byte) Maths.Lerp(startingAlpha, targetAlpha, alphaTween.Progress);
            WindowColor = WindowColor.SetAlpha(current);

            // If fading in we need to set visible from the get go.
            if (setVisible != null && setVisible.Value && !Visible) Visible = true;

            if (alphaTween.Finished)
            {
                Assert(WindowColor.A == targetAlpha);
                if (setVisible != null) Visible = setVisible.Value;
                yield break;
            }

            InvalidateColor();
            yield return null;
        }
    }

    public void SetAlpha(byte value, ITimer? tween = null)
    {
        Engine.CoroutineManager.StopCoroutine(_alphaTweenRoutine);
        _alphaTweenTimer?.End();
        _alphaTweenTimer = null;

        if (tween == null)
        {
            WindowColor = WindowColor.SetAlpha(value);
            return;
        }

        if (WindowColor.A == value)
        {
            tween.End();
            _alphaTweenRoutine = null;
            return;
        }

        _alphaTweenTimer = tween;
        _alphaTweenRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(tween, WindowColor.A, value, null));
    }

    public void SetVisible(bool val, ITimer? tween = null)
    {
        SetVisibleFade(val, tween);
    }

    public IRoutineWaiter SetVisibleFade(bool val, ITimer? tween = null)
    {
        Engine.CoroutineManager.StopCoroutine(_alphaTweenRoutine);
        _alphaTweenTimer?.End();
        _alphaTweenTimer = null;

        var targetAlpha = (byte) (val ? 255 : 0);
        if (tween == null)
        {
            WindowColor = WindowColor.SetAlpha(targetAlpha);
            Visible = val;
            return Coroutine.CompletedRoutine;
        }

        if (Visible == val && WindowColor.A == targetAlpha)
        {
            tween.End();
            _alphaTweenRoutine = null;
            return Coroutine.CompletedRoutine;
        }

        _alphaTweenTimer = tween;
        _alphaTweenRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(tween, WindowColor.A, targetAlpha, Visible == val ? null : val));
        return _alphaTweenRoutine;
    }

    protected virtual void CalculateColor()
    {
        if (Parent == null || IgnoreParentColor)
            _calculatedColor = WindowColor;
        else
            _calculatedColor = WindowColor * Parent._calculatedColor.A;

        if (Children == null) return;
        for (var i = 0; i < Children.Count; i++)
        {
            UIBaseWindow child = Children[i];
            child.CalculateColor();
        }
    }

    #endregion

    #region Input

    /// <summary>
    /// Find a window that handles input in this parent.
    /// Could be either a child window or the parent itself.
    /// </summary>
    public UIBaseWindow? FindInputFocusable()
    {
        if (!Visible) return null;

        if (Children != null && ChildrenHandleInput)
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIBaseWindow win = Children[i];
                if (win.Visible)
                {
                    UIBaseWindow? found = win.FindInputFocusable();
                    if (found != null) return found;
                }
            }

        return HandleInput ? this : null;
    }

    public virtual void InputFocusChanged(bool haveFocus)
    {
    }

    #endregion

    #region Animations

    /// <summary>
    /// Ignore the displacements on the parent, otherwise the displacements of this window are multiplied by the parent's.
    /// </summary>
    public bool IgnoreParentDisplacement { get; set; }

    /// <summary>
    /// List of affine transformations to apply to this window and its children.
    /// </summary>
    [DontSerialize]
    public NamedTransformationStack TransformationStack
    {
        get => _transformationStackBacking ??= new NamedTransformationStack();
    }

    protected NamedTransformationStack? _transformationStackBacking;

    protected void MarkChildrenMatricesAsDirty()
    {
        if (Children == null) return;
        for (int i = 0; i < Children.Count; i++)
        {
            UIBaseWindow child = Children[i];
            if (child.IgnoreParentDisplacement) continue;
            if (child._transformationStackBacking != null)
                child._transformationStackBacking.MatrixDirty = true;
            child.MarkChildrenMatricesAsDirty();
        }
    }

    /// <summary>
    /// Displace the position of a UI window over time.
    /// At the end of the tween the window will remain displaced.
    /// </summary>
    public IEnumerator TranslationDisplacement(Vector3 position, ITimer tween, string id = "translation")
    {
        while (true)
        {
            tween.Update(Engine.DeltaTime);
            Vector3 current = Vector3.Lerp(Vector3.Zero, position, tween.Progress);
            TransformationStack.AddOrUpdate(id, Matrix4x4.CreateTranslation(current.X, current.Y, 0));
            if (tween.Finished) yield break;

            yield return null;
        }
    }

    public IEnumerator PositionDisplacement(Vector3 position, ITimer tween, string id = "translation")
    {
        while (true)
        {
            tween.Update(Engine.DeltaTime);
            Vector3 current = Vector3.Lerp(Position / GetScale(), position, tween.Progress);
            TransformationStack.AddOrUpdate(id, Matrix4x4.CreateTranslation(current.X, current.Y, 0), true, MatrixSpecialFlag.TranslationPositionReplace);
            if (tween.Finished) break;

            yield return null;
        }
    }

    public IEnumerator ScaleDisplacement(float scaleStart, float scaleTarget, ITimer tween, string id = "scale")
    {
        while (true)
        {
            tween.Update(Engine.DeltaTime);
            float current = Maths.Lerp(scaleStart, scaleTarget, tween.Progress);
            TransformationStack.AddOrUpdate(id,
                Matrix4x4.CreateScale(current, current, 1f), true, MatrixSpecialFlag.ScaleBoundsCenter);
            if (tween.Finished) break;

            yield return null;
        }
    }


    /// <summary>
    /// The window's rotation around its center, in degrees.
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation == value) return;
            SetRotation(value);
        }
    }

    private float _rotation;

    private Coroutine? _rotationRoutineCurrent;
    private ITimer? _rotationTweenCurrent;

    /// <summary>
    /// Set a rotation (in degrees) for this window around its center.
    /// </summary>
    public void SetRotation(float degrees, ITimer? tween = null)
    {
        if (_rotationRoutineCurrent != null && !_rotationRoutineCurrent.Finished)
            Engine.CoroutineManager.StopCoroutine(_rotationRoutineCurrent);
        _rotationRoutineCurrent = Engine.CoroutineManager.StartCoroutine(RotationDisplacement(_rotation, degrees, tween));
    }

    /// <summary>
    /// Rotate a UI window around its center. Optionally over time.
    /// </summary>
    private IEnumerator RotationDisplacement(float fromDegrees, float toDegrees, ITimer? tween = null, string id = "rotation")
    {
        if (tween == null)
        {
            _rotation = toDegrees;
            TransformationStack.AddOrUpdate(id, Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(toDegrees)), true, MatrixSpecialFlag.RotateBoundsCenter);
            yield break;
        }

        while (true)
        {
            tween.Update(Engine.DeltaTime);
            float current = Maths.LerpAngle(fromDegrees, toDegrees, tween.Progress);
            _rotation = current;
            TransformationStack.AddOrUpdate(id, Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(current)), true, MatrixSpecialFlag.RotateBoundsCenter);
            if (tween.Finished) yield break;

            yield return null;
        }
    }

    #endregion

    #region Enum

    public IEnumerator<UIBaseWindow> GetEnumerator()
    {
        for (var i = 0; i < Children?.Count; i++)
        {
            UIBaseWindow cur = Children[i];

            // Get children.
            var childrenLists = new Queue<List<UIBaseWindow>>();
            if (cur.Children != null) childrenLists.Enqueue(cur.Children);
            yield return cur;

            while (childrenLists.Count > 0)
            {
                List<UIBaseWindow> list = childrenLists.Dequeue();
                for (var j = 0; j < list.Count; j++)
                {
                    UIBaseWindow child = list[j];
                    // Get grandchildren if any.
                    if (child.Children?.Count > 0) childrenLists.Enqueue(child.Children);
                    yield return child;
                }
            }
        }
    }

    #endregion

    #region Rollover Functionality

    public virtual UIRollover? GetRollover()
    {
        return null;
    }

    #endregion
}