#region Using

using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public class TmxAnimationFrame
    {
        public int Id { get; private set; }
        public int Duration { get; private set; }

        public TmxAnimationFrame(XMLReader xFrame)
        {
            Id = xFrame.AttributeInt("tileid");
            Duration = xFrame.AttributeInt("duration");
        }
    }
}