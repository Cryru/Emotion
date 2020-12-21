#region Using

using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public class TmxTerrain : ITmxElement
    {
        public string Name { get; private set; }
        public int Tile { get; private set; }

        public TmxProperties Properties { get; private set; }

        public TmxTerrain(XMLReader xTerrain)
        {
            Name = xTerrain.Attribute("name");
            Tile = xTerrain.AttributeInt("tile");
            Properties = TmxHelpers.GetPropertyDict(xTerrain.Element("properties"));
        }
    }
}