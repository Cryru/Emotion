namespace Emotion.Editor.EditorUI.Components.One;

public class OneHorizontalPanelSeparator : HorizontalPanelSeparator
{
    public OneHorizontalPanelSeparator()
    {
        Layout.SizingX = UISizing.Fixed(6);
        MouseInsideChanged(false, Vector2.Zero);
    }

    protected override void MouseInsideChanged(bool inside, Vector2 mousePos)
    {
        Visuals.BackgroundColor = inside ? OnePalette.PRIMARY_5 : OnePalette.PRIMARY_6;
        base.MouseInsideChanged(inside, mousePos);
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);

        float radius = 1 * CalculatedMetrics.ScaleF;
        float space = MathF.Round(6 * CalculatedMetrics.ScaleF);

        Vector2 center = CalculatedMetrics.Position.ToVec2() + (CalculatedMetrics.Size.ToVec2() / 2f);
        center = center.Round();
        r.RenderCircle(center.ToVec3(), radius, OnePalette.DETAILS_1, true);
        r.RenderCircle(center.ToVec3() - new Vector3(0, space, 0), radius, OnePalette.DETAILS_1, true);
        r.RenderCircle(center.ToVec3() + new Vector3(0, space, 0), radius, OnePalette.DETAILS_1, true);
    }
}