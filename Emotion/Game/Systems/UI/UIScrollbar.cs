#nullable enable

namespace Emotion.Game.Systems.UI;

public class UIScrollbar : UIBaseWindow
{
    /// <summary>
    /// Whether the scrollbar scrolls horizontally.
    /// </summary>
    public bool Horizontal;

    public Color DefaultSelectorColor = new Color(125, 0, 0);
    public Color SelectorMouseInColor = new Color(200, 0, 0);

    [DontSerialize]
    public UIBaseWindow? ScrollParent = null;

    [DontSerialize]
    public Action<float>? OnScroll;

    private Color _selectorColor;
    protected Rectangle _selectorRect;
    private Vector2 _dragging;

    public float TotalArea;
    public float PageArea;
    public float Current;

    public Color? OutlineColor;
    public float OutlineSize;

    public UIScrollbar()
    {
        HandleInput = true;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        _selectorColor = DefaultSelectorColor;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyState.Down)
            {
                if (_selectorRect.ContainsInclusive(mousePos))
                    _dragging = mousePos - _selectorRect.Position;
                else
                    _dragging = Vector2.One;

                OnMouseMove(mousePos);
            }
            else if (status == KeyState.Up)
            {
                _dragging = Vector2.Zero;
            }
        }

        if (ScrollParent != null) return ScrollParent.OnKey(key, status, mousePos);

        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        if (_dragging != Vector2.Zero || _selectorRect.ContainsInclusive(mousePos))
            _selectorColor = SelectorMouseInColor;
        else
            _selectorColor = DefaultSelectorColor;

        if (_dragging == Vector2.Zero) return;

        float progress;
        if (Horizontal)
        {
            progress = Maths.Map(mousePos.X - _dragging.X, X, X + Width, 0, TotalArea);
            progress = MathF.Round(progress);
            var list = (UICallbackListNavigator?)ScrollParent;
            list?.ScrollByAbsolutePos(progress);
        }
        else
        {
            progress = Maths.Map(mousePos.Y - _dragging.Y, Y, Y + Height, 0, TotalArea);
            progress = MathF.Round(progress);
            var list = (UICallbackListNavigator?)ScrollParent;
            list?.ScrollByAbsolutePos(progress);
        }

        OnScroll?.Invoke(progress);

        base.OnMouseMove(mousePos);
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        _selectorColor = DefaultSelectorColor;

        base.OnMouseLeft(mousePos);
    }

    public void UpdateScrollbar()
    {
        if (ScrollParent != null)
        {
            // todo: what is this tomfoolery
            float spaceTaken = TotalArea > PageArea ? PageArea : TotalArea;
            _measuredSize.Y = spaceTaken;
            Height = spaceTaken;
        }

        if (Horizontal)
        {
            float progress = Maths.Map(Current, 0, TotalArea, 0, Width);
            progress = MathF.Round(progress);
            float size = Maths.Map(PageArea, 0, TotalArea, 0, Width);
            size = MathF.Round(size);
            _selectorRect = new Rectangle(X + progress, Y, size, Height);
        }
        else
        {
            float progress = Maths.Map(Current, 0, TotalArea, 0, Height);
            progress = MathF.Round(progress);
            float size = Maths.Map(PageArea, 0, TotalArea, 0, Height);
            size = MathF.Round(size);
            _selectorRect = new Rectangle(X, Y + progress, Width, size);
        }
    }

    protected override void AfterLayout()
    {
        base.AfterLayout();
        UpdateScrollbar();
    }

    protected override bool RenderInternal(Renderer c)
    {
        c.RenderSprite(Bounds, _calculatedColor);
        c.RenderSprite(_selectorRect, _selectorColor);

        return true;
    }
}