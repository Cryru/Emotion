#region Using

using Emotion.Game;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Text
{
    public class GlyphRendererState
    {
        public FrameBuffer AtlasBuffer;
        public Packing.PackingResumableState PackingState;
    }
}