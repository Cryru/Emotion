#nullable enable

namespace Emotion.Game.Systems.UI;

public class UISlider : UIBaseWindow
{
    public int MinValue
    {
        get => _minVal;
        set
        {
            if (_minVal == value) return;
            _minVal = value;
            InvalidateLayout();
        }
    }

    private int _minVal;

    public int MaxValue
    {
        get => _maxVal;
        set
        {
            if (_maxVal == value) return;
            _maxVal = value;
            InvalidateLayout();
        }
    }

    private int _maxVal = 100;

    /// <summary>
    /// Whether the scrollbar scrolls horizontally.
    /// </summary>
    public bool Horizontal;

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = Maths.Clamp(value, MinValue, MaxValue);
            OnValueChanged?.Invoke(_value);
            InvalidateLayout();
        }
    }

    private int _value;

    /// <summary>
    /// Whether to keep the selector inside the bar or to keep its center inside. By default the center is kept inside only.
    /// </summary>
    public bool KeepSelectorInside = true;

    /// <summary>
    /// How wide should the selector be compared to the total bar.
    /// </summary>
    public int SelectorRatio = 1;

    public Color DefaultSelectorColor = Color.Red;
    public Color RolloverColor = Color.PrettyRed;

    [DontSerialize] public Action<int>? OnValueChanged;

    [DontSerialize] public UIBaseWindow? ScrollParent = null;

    private UIBaseWindow _selector = null!;
    private bool _dragging;

    public UISlider()
    {
        HandleInput = true;
        GrowX = false;
        GrowY = false;
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        UIBaseWindow? scroll = GetWindowById("Selector");
        if (scroll == null)
        {
            scroll = new UISolidColor {
                WindowColor = DefaultSelectorColor,
                Name = "Selector",
            };
            AddChild(scroll);
        }

        _selector = scroll;
    }

    protected override bool UpdateInternal()
    {
        Vector2 mouse = MouseInside ? Engine.Host.MousePosition : Vector2.Zero;
        _selector.WindowColor = _dragging || _selector.RenderBounds.Contains(mouse) ? RolloverColor : DefaultSelectorColor;
        return base.UpdateInternal();
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyState.Down)
            {
                SetValueFromPos(mousePos);
                _dragging = true;
            }
            else if (status == KeyState.Up)
            {
                _dragging = false;
            }
        }

        if (ScrollParent != null) return ScrollParent.OnKey(key, status, mousePos);

        if (key == Key.MouseWheel) Value += status == KeyState.Up ? -1 : 1;

        return base.OnKey(key, status, mousePos);
    }

    public void SetValueFromPos(Vector2 pos)
    {
        Vector2 relativePos = pos - _renderBoundsWithChildren.Position;
        int range = MaxValue - MinValue;
        Vector2 size = Size;

        if (Horizontal)
            Value = MinValue + (int) MathF.Round(relativePos.X / size.X * range);
        else
            Value = MinValue + (int) MathF.Round(relativePos.Y / size.Y * range);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        if (!_dragging) return;
        SetValueFromPos(mousePos);
        base.OnMouseMove(mousePos);
    }

    protected override void AfterMeasure(Vector2 mySize)
    {
        mySize /= GetScale();
        int range = 1 + (MaxValue - MinValue);
        Vector2 selectorSize;
        if (Horizontal)
            selectorSize = new Vector2(mySize.X / range * SelectorRatio, DefaultMaxSize.Y);
        else
            selectorSize = new Vector2(DefaultMaxSize.X, mySize.Y / range * SelectorRatio);

        _selector.MaxSize = selectorSize;
    }

    // Glue new UI and old UI
    //protected override Vector2 Measure(Vector2 space)
    //{
    //    var size = base.Measure(space);
    //    AfterMeasure(size);
    //    Logic();
    //    return size;
    //}

    protected void Logic()
    {
        Vector2 size = Size / GetScale();
        int range = MaxValue - MinValue;

        if (Horizontal)
        {
            float selectorSize = _selector.MaxSize.X;
            float offset;
            if (!KeepSelectorInside)
            {
                offset = size.X / range * (Value - MinValue);
                offset -= selectorSize / 2;
            }
            else
            {
                offset = (size.X - selectorSize) / range * (Value - MinValue);
            }

            _selector.Layout.Offset = new IntVector2((int) Math.Floor(offset), 0);
        }
        else
        {
            float selectorSize = _selector.MaxSize.Y;
            float offset;
            if (!KeepSelectorInside)
            {
                offset = size.Y / range * (Value - MinValue);
                offset -= selectorSize / 2;
            }
            else
            {
                offset = (size.Y - selectorSize) / range * (Value - MinValue);
            }

            _selector.Layout.Offset = new IntVector2(0, (int) Math.Floor(float.IsNaN(offset) ? 0 : offset));
        }
    }

    protected override bool RenderInternal(Renderer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        // c.RenderSprite(Parent.Position, Parent.Size, Color.Red);
        return true;
    }
}