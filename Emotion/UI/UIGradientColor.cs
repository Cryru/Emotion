#region Using

using System;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UIGradientColor : UIBaseWindow
    {
        public float GradientOut;
        public float FillHeight;
        public float GradientSize;
        public float GradientOverlap;

        public Color ColorTo;

        public float BaseColorMix = 0.0f;
        public float ToColorMix = 0.0f;

        protected override bool RenderInternal(RenderComposer composer)
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
}