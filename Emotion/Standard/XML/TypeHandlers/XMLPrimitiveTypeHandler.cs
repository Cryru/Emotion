#region Using

using System;
using System.Globalization;
using System.Text;

#endregion

#nullable enable

namespace Emotion.Standard.XML.TypeHandlers
{
    /// <inheritdoc />
    public class XMLPrimitiveTypeHandler : XMLTypeHandler
    {
        /// <summary>
        /// The object's default value. Used to skip serializing unnecessary data.
        /// </summary>
        protected object? _defaultValue;

        /// <inheritdoc />
        public XMLPrimitiveTypeHandler(Type type, bool nonNullable) : base(type)
        {
            _defaultValue = nonNullable ? Activator.CreateInstance(Type, true) : null;
        }

        /// <inheritdoc />
        public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null)
        {
            if (_defaultValue != null && obj.Equals(_defaultValue)) return;
            base.SerializeValue(obj, output, indentation, recursionChecker);
        }

        /// <inheritdoc />
        public override object? Deserialize(XMLReader input)
        {
            string readValue = input.GoToNextTag();
            return string.IsNullOrEmpty(readValue) ? _defaultValue : Convert.ChangeType(readValue, Type, CultureInfo.InvariantCulture);
        }
    }
}