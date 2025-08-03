#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public struct UIRectangleSpacingMetric
{
    public Vector2 TopLeft;
    public Vector2 BottomRight;

    public UIRectangleSpacingMetric(float left, float top, float right, float bottom)
    {
        TopLeft = new Vector2(left, top);
        BottomRight = new Vector2(right, bottom);
    }
}

public class O_UIWindowCalculatedMetrics
{
    public Vector3 Position;
    public Vector2 Size;

    public override string ToString()
    {
        return "Caculation";
    }
}

public class O_UIWindowLayoutMetrics
{
    public Vector2 MinSize = new Vector2();
    public Vector2 MaxSize = new Vector2(99999, 99999);
    public UIRectangleSpacingMetric Padding;

    public LayoutMode LayoutMode = LayoutMode.Free;
    public Vector2 ListSpacing;

    // new
    public bool FitX = true;
    public bool FitY = true;

    public bool GrowX = false;
    public bool GrowY = false;

    public override string ToString()
    {
        return "Metrics";
    }
}

public class O_UIWindowProperties
{
    public Color Color = Color.White;

    public override string ToString()
    {
        return "Properties";
    }
}

public partial class O_UIBaseWindow
{
    [SerializeNonPublicGetSet]
    public O_UIWindowLayoutMetrics Layout { get; private set; } = new O_UIWindowLayoutMetrics();

    [SerializeNonPublicGetSet]
    public O_UIWindowProperties Properties { get; private set; } = new O_UIWindowProperties();

    [SerializeNonPublicGetSet]
    [DontSerializeButShowInEditor]
    public O_UIWindowCalculatedMetrics CalculatedMetrics { get; private set; } = new O_UIWindowCalculatedMetrics();

    public List<O_UIBaseWindow> Children { get => ChildrenSerialized; init => ChildrenSerialized = value; }

    [SerializeNonPublicGetSet]
    public List<O_UIBaseWindow> ChildrenSerialized { get; private set; } = new List<O_UIBaseWindow>();

    public class O_UIBaseWindowSystemAdapter // Friend class workaround.
    {
        private O_UIBaseWindow _this;

        public O_UIBaseWindowSystemAdapter(O_UIBaseWindow win)
        {
            _this = win;
        }
    }

    [DontSerialize]
    public O_UIBaseWindowSystemAdapter UISystemAdapter { get; private set; }

    public O_UIBaseWindow()
    {
        UISystemAdapter = new O_UIBaseWindowSystemAdapter(this);
    }

    public virtual void Update()
    {
        SystemDoLayout(UILayoutPass.Measure);
        SystemDoLayout(UILayoutPass.Grow);
        SystemDoLayout(UILayoutPass.Layout);
    }

    public void Render(Renderer c)
    {
        var calc = CalculatedMetrics;
        c.RenderSprite(calc.Position, calc.Size, Properties.Color);

        foreach (O_UIBaseWindow win in Children)
        {
            win.Render(c);
        }
    }

    #region Layout

    public enum UILayoutPass
    {
        None,

        Measure,
        Grow,
        Layout
    }

    protected void SystemDoLayout(UILayoutPass pass)
    {
        switch (pass)
        {
            case UILayoutPass.Measure:
                CalculatedMetrics.Size = MeasureWindow();
                break;
            case UILayoutPass.Grow:
                GrowWindow();
                break;
            case UILayoutPass.Layout:
                LayoutWindow(Vector2.Zero);
                break;
        }
    }

    protected void LayoutWindow(Vector2 pos)
    {
        CalculatedMetrics.Position = pos.ToVec3();

        Vector2 pen = new Vector2();
        pen += Layout.Padding.TopLeft;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                foreach (O_UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                    pen[listMask] += child.CalculatedMetrics.Size[listMask] + Layout.ListSpacing[listMask];
                }
                break;
        }
    }

    protected Vector2 MeasureWindow()
    {
        Vector2 mySize = InternalGetWindowMinSize();
        mySize += Layout.Padding.TopLeft;
        mySize += Layout.Padding.BottomRight;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                int inverseListMask = listMask == 1 ? 0 : 1;

                Vector2 pen = new Vector2();
                foreach (O_UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;

                    pen[listMask] += windowMinSize[listMask];
                    pen[inverseListMask] = MathF.Max(pen[inverseListMask], windowMinSize[inverseListMask]);
                }

                float totalSpacing = Layout.ListSpacing[listMask] * (Children.Count - 1);

                bool fitAlongList = listMask == 0 ? Layout.FitX : Layout.FitY;
                if (fitAlongList)
                    mySize[listMask] += pen[listMask] + totalSpacing;

                bool fitAcrossList = inverseListMask == 0 ? Layout.FitX : Layout.FitY;
                if (fitAcrossList)
                    mySize[inverseListMask] += pen[inverseListMask];

                break;
        }

        return Vector2.Clamp(mySize, Layout.MinSize, Layout.MaxSize);
    }

    protected void GrowWindow()
    {
        Vector2 myMeasuredSize = CalculatedMetrics.Size;
        myMeasuredSize -= Layout.Padding.TopLeft;
        myMeasuredSize -= Layout.Padding.BottomRight;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                int inverseListMask = listMask == 1 ? 0 : 1;

                float listRemainingSize = myMeasuredSize[listMask];
                foreach (O_UIBaseWindow child in Children)
                {
                    float listSize = child.CalculatedMetrics.Size[listMask];
                    listRemainingSize -= listSize;
                }
                listRemainingSize -= Layout.ListSpacing[listMask] * (Children.Count - 1);

                foreach (O_UIBaseWindow child in Children)
                {
                    bool growAlongList = listMask == 0 ? child.Layout.GrowX : child.Layout.GrowY;
                    if (growAlongList)
                        child.CalculatedMetrics.Size[listMask] += listRemainingSize;

                    bool growAcrossList = inverseListMask == 0 ? child.Layout.GrowX : child.Layout.GrowY;
                    if (growAcrossList)
                        child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask];

                    child.GrowWindow();
                }
                break;
        }
    }

    #endregion

    #region API

    protected virtual Vector2 InternalGetWindowMinSize()
    {
        return Vector2.Zero;
    }

    #endregion

    public override string ToString()
    {
        return $"Window: {Layout.LayoutMode}";
    }
}

public class O_UISystem : O_UIBaseWindow
{
    public O_UISystem()
    {
        Layout.FitX = false;
        Layout.FitY = false;
        Layout.GrowX = false;
        Layout.GrowY = false;
    }

    public void AddChild(O_UIBaseWindow window)
    {
        Children.Add(window);
        _needLayout = true;
    }

    public override void Update()
    {
        if (_needLayout)
            ApplyLayout();

        
    }

    #region System

    protected override Vector2 InternalGetWindowMinSize()
    {
        return new Vector2(1920, 1080);
    }

    #endregion

    #region Layout

    protected bool _needLayout = true;

    private void ApplyLayout()
    {
        _needLayout = false;

        SystemDoLayout(UILayoutPass.Measure);
        SystemDoLayout(UILayoutPass.Grow);
        SystemDoLayout(UILayoutPass.Layout);
    }

    #endregion
}