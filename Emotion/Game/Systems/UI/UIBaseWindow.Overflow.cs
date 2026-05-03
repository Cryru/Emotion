#nullable enable

using Emotion.Editor.EditorUI;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    [DontSerialize]
    public Vector2 ScrollOffset { get; private set; }

    protected Matrix4x4 _scrollMatrix = Matrix4x4.Identity;
    protected UIScrollbar? _scrollbarV;
    protected UIScrollbar? _scrollbarH;

    protected virtual UIScrollbar? CreateScrollbarVertical()
    {
        return new EditorScrollBar();
    }

    protected virtual UIScrollbar? CreateScrollbarHorizontal()
    {
        return new EditorScrollBarHorizontal();
    }

    private void EnsureScrollbars()
    {
        if (Layout.OverflowY == UIOverflow.Scroll && _scrollbarV == null)
        {
            _scrollbarV = CreateScrollbarVertical();
            if (_scrollbarV != null)
                AddChild(_scrollbarV);
        }
        if (Layout.OverflowX == UIOverflow.Scroll && _scrollbarH == null)
        {
            _scrollbarH = CreateScrollbarHorizontal();
            if (_scrollbarH != null)
                AddChild(_scrollbarH);
        }
    }

    public bool ScrollTo(Vector2 pos, bool forceUpdate = false)
    {
        Vector2 clamped = Vector2.Clamp(pos, Vector2.Zero, CalculatedMetrics.MaxScroll);
        if (clamped == ScrollOffset && !forceUpdate) return false;

        ScrollOffset = clamped;
        _scrollMatrix = Matrix4x4.CreateTranslation(-ScrollOffset.X, -ScrollOffset.Y, 0);
        SyncScrollbars();
        return true;
    }

    private void SyncScrollbars()
    {
        if (_scrollbarV != null)
        {
            IntVector2 viewport = CalculatedMetrics.GetViewportSize();
            _scrollbarV.Current = ScrollOffset.Y;
            _scrollbarV.TotalArea = CalculatedMetrics.MaxScroll.Y + viewport.Y;
            _scrollbarV.PageArea = viewport.Y;
            _scrollbarV.UpdateScrollbar();
            _scrollbarV.SetVisible(CalculatedMetrics.MaxScroll.Y > 0);
        }
        if (_scrollbarH != null)
        {
            IntVector2 viewport = CalculatedMetrics.GetViewportSize();
            _scrollbarH.Current = ScrollOffset.X;
            _scrollbarH.TotalArea = CalculatedMetrics.MaxScroll.X + viewport.X;
            _scrollbarH.PageArea = viewport.X;
            _scrollbarH.UpdateScrollbar();
            _scrollbarH.SetVisible(CalculatedMetrics.MaxScroll.X > 0);
        }
    }

    // Called from InternalOnLayoutComplete
    private void ProcessOverflowLayout()
    {
        EnsureScrollbars();

        IntRectangle contentRect = CalculatedMetrics.GetViewportRect();
        IntVector2 viewportSize = contentRect.Size;
        IntVector2 viewportStart = contentRect.Position;
        Vector2 contentEnd = Vector2.Zero;

        List<UIBaseWindow> children = GetChildrenListForPass(0);
        foreach (UIBaseWindow child in children)
        {
            IntRectangle b = child.CalculatedMetrics.Bounds;
            contentEnd.X = Math.Max(contentEnd.X, b.Right - viewportStart.X);
            contentEnd.Y = Math.Max(contentEnd.Y, b.Bottom - viewportStart.Y);
        }

        Vector2 maxScroll = new Vector2(
            Layout.OverflowX == UIOverflow.Scroll ? Math.Max(0, contentEnd.X - viewportSize.X) : 0,
            Layout.OverflowY == UIOverflow.Scroll ? Math.Max(0, contentEnd.Y - viewportSize.Y) : 0
        );

        if (_scrollbarV != null)
            maxScroll.X += _scrollbarV.CalculatedMetrics.Size.X;
        if (_scrollbarH != null)
            maxScroll.Y += _scrollbarH.CalculatedMetrics.Size.Y;

        CalculatedMetrics.MaxScroll = maxScroll;
        ScrollTo(ScrollOffset, true);
    }
}
