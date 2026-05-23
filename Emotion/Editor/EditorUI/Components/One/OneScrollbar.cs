#nullable enable

using Emotion;

namespace Emotion.Editor.EditorUI.Components.One;

public class OneScrollbar : UIScrollbar
{
    private Vector2 _scaledOffset;
    private Vector2 _scaledSizeDiff;

    public OneScrollbar(bool horizontal) : base(horizontal)
    {
        DefaultSelectorColor = OnePalette.PRIMARY_4;
        SelectorMouseInColor = OnePalette.PRIMARY_7;

        if (horizontal)
            Layout.SizingY = UISizing.Fixed(14);
        else
            Layout.SizingX = UISizing.Fixed(14);
        Visuals.BackgroundColor = OnePalette.PRIMARY_6;
    }

    protected override void InternalOnLayoutComplete()
    {
        base.InternalOnLayoutComplete();

        _scaledOffset = new Vector2(2f) * CalculatedMetrics.ScaleF;
        _scaledSizeDiff = new Vector2(4f) * CalculatedMetrics.ScaleF;
    }

    protected override void RenderScrollbarSelector(Renderer r)
    {
        Rectangle rect = _selectorRect;
        r.RenderRoundedRectSdf((rect.Position + _scaledOffset).ToVec3(), rect.Size - _scaledSizeDiff, _selectorColor, 5 * CalculatedMetrics.ScaleF);
    }
}