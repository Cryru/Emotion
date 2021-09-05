#region Using

using System;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

#nullable enable

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLEnumTypeHandler : XMLPrimitiveTypeHandler
    {
        public XMLEnumTypeHandler(Type type, bool opaque) : base(type, opaque)
        {
        }

        public override object? Deserialize(XMLReader input)
        {
            string readValue = input.GoToNextTag();
            if (Enum.TryParse(Type, readValue, out object? parsed))
            {
                return parsed;
            }

            Engine.Log.Warning($"Couldn't find value {readValue} in enum {Type}.", MessageSource.XML);
            return null;
        }
    }
}