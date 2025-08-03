#nullable enable

#region Using

using Emotion.Standard.Parsers.XML;

#endregion

namespace Emotion.Standard.Parsers.TMX;

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