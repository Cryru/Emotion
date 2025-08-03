#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Game.Systems.UI;

public class UIGradientColor : UIBaseWindow
{
    public float GradientOut;
    public float FillHeight;
    public float GradientSize;
    public float GradientOverlap;

    public Color ColorTo;

    public float BaseColorMix = 0.0f;
    public float ToColorMix = 0.0f;

    protected override bool RenderInternal(Renderer composer)
    {
        Span<VertexData> memory = composer.RenderStream.GetStreamMemory(4, BatchMode.Quad);
        VertexData.SpriteToVertexData(memory, Position, Size, WindowColor);

        Color baseColorBias = Color.Lerp(WindowColor, ColorTo, BaseColorMix);
        uint cFrom = baseColorBias.ToUint();
        memory[0].Color = cFrom;
        memory[1].Color = cFrom;

        Color secondColorBias = Color.Lerp(ColorTo, WindowColor, ToColorMix);
        uint cTo = secondColorBias.ToUint();
        memory[2].Color = cTo;
        memory[3].Color = cTo;

        return true;
    }
}