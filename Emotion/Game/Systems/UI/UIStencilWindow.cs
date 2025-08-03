#nullable enable

namespace Emotion.Game.Systems.UI;

/// <summary>
/// Used to clip UIs using stencil logic.
/// This could work with a clip rect as well, but then the area would always have to be square
/// while this was the various transformations and displacements will affect the clip.
/// </summary>
public class UIStencilWindow : UIBaseWindow
{
    protected override bool RenderInternal(Renderer c)
    {
        c.SetStencilTest(true);
        c.ToggleRenderColor(false);
        c.StencilStartDraw();
        c.RenderSprite(Position, Size, Color.White);
        c.ToggleRenderColor(true);
        c.StencilFillIn();

        return base.RenderInternal(c);
    }

    protected override void AfterRenderChildren(Renderer c)
    {
        c.SetStencilTest(false);
    }
}