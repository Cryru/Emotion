#nullable enable


namespace Emotion.Game.Systems.UI;

public class UIProgressBar : UIBaseWindow
{
    public float Progress = 0.5f;
    public Color ProgressColor = Color.White;

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);

        r.RenderSprite(
            CalculatedMetrics.Position.ToVec2(),
            new Vector2(CalculatedMetrics.Size.X * Progress, CalculatedMetrics.Size.Y),
            ProgressColor
        );
    }
}
