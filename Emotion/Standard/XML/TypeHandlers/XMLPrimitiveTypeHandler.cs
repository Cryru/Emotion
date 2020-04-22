#region Using

using System;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XMLPrimitiveTypeHandler : XMLTypeHandler
    {
        protected object _defaultValue;

        public XMLPrimitiveTypeHandler(Type type, bool opaque) : base(type)
        {
            _defaultValue = opaque ? Activator.CreateInstance(Type, true) : null;
        }

        public override bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (_defaultValue != null && obj.Equals(_defaultValue)) return false;
            return base.Serialize(obj, output, indentation, recursionChecker, fieldName);
        }

        public override object Deserialize(XMLReader input)
        {
            string readValue = input.GoToNextTag();
            return string.IsNullOrEmpty(readValue) ? _defaultValue : Convert.ChangeType(readValue, Type);
        }
    }
}