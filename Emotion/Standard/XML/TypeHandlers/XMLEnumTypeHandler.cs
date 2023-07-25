#region Using

using System.Reflection;
using System.Text;
using Emotion.Common.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.XML.TypeHandlers
{
	public class XMLEnumTypeHandler : XMLPrimitiveTypeHandler
	{
		private DontSerializeFlagValueAttribute? _dontSerializeFlag;

		public XMLEnumTypeHandler(Type type, bool opaque) : base(type, opaque)
		{
			var isFlagEnum = type.GetCustomAttribute<FlagsAttribute>();
			if (isFlagEnum != null && Type.GetEnumUnderlyingType() == typeof(uint))
				_dontSerializeFlag = type.GetCustomAttribute<DontSerializeFlagValueAttribute>();
		}

        public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null)
        {
            if (_dontSerializeFlag != null) obj = Enum.ToObject(Type, _dontSerializeFlag.ClearDontSerialize((uint)obj));

            base.SerializeValue(obj, output, indentation, recursionChecker);
        }

        public override object? Deserialize(XMLReader input)
		{
			string readValue = input.GoToNextTag();
			if (readValue == "") return _defaultValue;

			if (Enum.TryParse(Type, readValue, out object? parsed)) return parsed;

			Engine.Log.Warning($"Couldn't find value {readValue} in enum {Type}.", MessageSource.XML, true);
			return null;
		}
	}
}