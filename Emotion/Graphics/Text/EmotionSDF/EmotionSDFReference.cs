#region Using

using System.Numerics;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Standard.OpenType;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionSDF
{
    public class EmotionSDFReference
    {
        public DrawableFont ReferenceFont;
        public FrameBuffer? AtlasFramebuffer;
        public Binning.BinningResumableState BinningState = new Binning.BinningResumableState(Vector2.Zero);

        public EmotionSDFReference(Font font, int referenceSize, bool pixelFont)
        {
            ReferenceFont = new DrawableFont(font, referenceSize, pixelFont);
        }
    }
}