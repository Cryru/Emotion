#region Using

using Emotion.Primitives;
using Emotion.Standard.TMX.Object;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Layer
{
    public class TmxObjectLayer : TmxLayer
    {
        public Color Color { get; private set; }
        public DrawOrder DrawOrder { get; private set; }

        public TmxList<TmxObject> Objects { get; private set; }

        public TmxObjectLayer(XMLReader xObjectGroup) : base(xObjectGroup, 0, 0)
        {
            Color = TmxHelpers.ParseTmxColor(xObjectGroup.Attribute("color"));
            DrawOrder = xObjectGroup.AttributeEnum<DrawOrder>("draworder");
            Objects = new TmxList<TmxObject>();
            foreach (XMLReader e in xObjectGroup.Elements("object"))
            {
                Objects.Add(new TmxObject(e));
            }
        }
    }
}