#region Using

using System;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLEnumTypeHandler : XMLPrimitiveTypeHandler
    {
        public XMLEnumTypeHandler(Type type, bool opaque) : base(type, opaque)
        {
        }

        public override object Deserialize(XMLReader input)
        {
            string readValue = input.GoToNextTag();
            return Enum.Parse(Type, readValue);
        }
    }
}