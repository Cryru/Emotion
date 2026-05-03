#nullable enable

namespace Emotion.Game.Systems.UI;

[DontSerialize]
public class UIScrollbar : UIBaseWindow
{
    /// <summary>
    /// Whether the scrollbar scrolls horizontally.
    /// </summary>
    public bool Horizontal { get; protected set; }

    public Color DefaultSelectorColor = new Color(125, 0, 0);
    public Color SelectorMouseInColor = new Color(200, 0, 0);

    protected Color _selectorColor;
    protected Rectangle _selectorRect;
    protected Vector2 _dragging;

    public float TotalArea;
    public float PageArea;
    public float Current;

    public UIScrollbar(bool horizontal)
    {
        HandleInput = true;
        Horizontal = horizontal;

        if (horizontal)
        {
            Layout.SizingY = UISizing.Fixed(15);
            Layout.SizingX = UISizing.Grow();
            Layout.Margins = new UISpacing(0, 0, 15, 0);
            Layout.AnchorAndParentAnchor = UIAnchor.BottomLeft;
        }
        else
        {
            Layout.SizingX = UISizing.Fixed(15);
            Layout.SizingY = UISizing.Grow();
            Layout.AnchorAndParentAnchor = UIAnchor.TopRight;
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();
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
            return false;
        }

        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        if (_dragging != Vector2.Zero || _selectorRect.ContainsInclusive(mousePos))
            _selectorColor = SelectorMouseInColor;
        else
            _selectorColor = DefaultSelectorColor;

        if (_dragging == Vector2.Zero) return;

        float progress = 0;
        if (Horizontal)
        {
            Rectangle track = CalculatedMetrics.Bounds.ToRect();
            float thumb = _selectorRect.Width;
            float trackLength = MathF.Max(0, track.Width - thumb);
            float pos = mousePos.X - _dragging.X - track.X;
            float norm = trackLength <= 0 ? 0 : pos / trackLength;
            norm = Math.Clamp(norm, 0, 1);
            progress = norm * MathF.Max(0, TotalArea - PageArea);
            Parent.ScrollTo(new Vector2(progress, Parent.ScrollOffset.Y));
        }
        else
        {
            Rectangle track = CalculatedMetrics.Bounds.ToRect();
            float thumb = _selectorRect.Height;
            float trackLength = MathF.Max(0, track.Height - thumb);
            float pos = mousePos.Y - _dragging.Y - track.Y;
            float norm = trackLength <= 0 ? 0 : pos / trackLength;
            norm = Math.Clamp(norm, 0, 1);
            progress = norm * MathF.Max(0, TotalArea - PageArea);
            Parent.ScrollTo(new Vector2(Parent.ScrollOffset.X, progress));
        }

        base.OnMouseMove(mousePos);
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        _selectorColor = DefaultSelectorColor;

        base.OnMouseLeft(mousePos);
    }

    public void UpdateScrollbar()
    {
        Rectangle track = CalculatedMetrics.Bounds.ToRect();
        if (Horizontal)
        {
            float trackW = track.Width;
            float thumbW = TotalArea <= 0 ? trackW : (PageArea / MathF.Max(1, TotalArea)) * trackW;
            thumbW = Math.Clamp(thumbW, 0, trackW);

            float maxScroll = MathF.Max(0, TotalArea - PageArea);
            float norm = maxScroll <= 0 ? 0 : Current / maxScroll;
            norm = Math.Clamp(norm, 0, 1);

            float x = track.X + norm * MathF.Max(0, trackW - thumbW);
            _selectorRect = new Rectangle(x, track.Y, thumbW, track.Height);
        }
        else
        {
            float trackH = track.Height;
            float thumbH = TotalArea <= 0 ? trackH : (PageArea / MathF.Max(1, TotalArea)) * trackH;
            thumbH = Math.Clamp(thumbH, 0, trackH);

            float maxScroll = MathF.Max(0, TotalArea - PageArea);
            float norm = maxScroll <= 0 ? 0 : Current / maxScroll;
            norm = Math.Clamp(norm, 0, 1);

            float y = track.Y + norm * MathF.Max(0, trackH - thumbH);
            _selectorRect = new Rectangle(track.X, y, track.Width, thumbH);
        }
    }

    protected override void InternalRender(Renderer r)
    {
        r.RenderSprite(_selectorRect, _selectorColor);
        base.InternalRender(r);
    }
}