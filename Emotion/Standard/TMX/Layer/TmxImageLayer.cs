#region Using

using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Layer
{
    public class TmxImageLayer : TmxLayer
    {
        public string Image { get; private set; }

        public TmxImageLayer(XMLReader xImageLayer) : base(xImageLayer, xImageLayer.AttributeInt("width"), xImageLayer.AttributeInt("height"))
        {
            Image = xImageLayer.Element("image").CurrentContents();
        }
    }
}