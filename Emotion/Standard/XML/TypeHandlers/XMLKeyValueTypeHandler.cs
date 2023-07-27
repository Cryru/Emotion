#region Using

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Emotion.Common.Serialization;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
	/// <inheritdoc />
	public class XMLKeyValueTypeHandler : XMLTypeHandler
	{
		private readonly Lazy<XMLFieldHandler> _keyHandler;
		private readonly Lazy<XMLFieldHandler> _valueHandler;

		private readonly object _keyDefault;
		private readonly object _valueDefault;

		/// <inheritdoc />
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public XMLKeyValueTypeHandler(Type type) : base(type)
		{
			PropertyInfo keyProperty = Type.GetProperty("Key");
			_keyHandler = new Lazy<XMLFieldHandler>(() => XMLHelpers.ResolveFieldHandler(keyProperty.PropertyType, new ReflectedMemberHandler(keyProperty)));
			PropertyInfo valueProperty = Type.GetProperty("Value");
			_valueHandler = new Lazy<XMLFieldHandler>(() => XMLHelpers.ResolveFieldHandler(valueProperty.PropertyType, new ReflectedMemberHandler(valueProperty)));

			Type opaqueKeyType = XMLHelpers.GetOpaqueType(keyProperty.PropertyType, out bool opaque);
			_keyDefault = opaque && opaqueKeyType.IsValueType ? Activator.CreateInstance(opaqueKeyType) : null;
			Type opaqueValueType = XMLHelpers.GetOpaqueType(valueProperty.PropertyType, out opaque);
			_valueDefault = opaque && opaqueValueType.IsValueType ? Activator.CreateInstance(opaqueValueType) : null;
		}

		/// <inheritdoc />
		public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null)
		{
			output.Append("\n");

			// Check key and value for recursive reference.
			recursionChecker ??= new XMLRecursionChecker();

			{
				XMLFieldHandler keyHandler = _keyHandler.Value;
				XMLTypeHandler keyTypeHandler = keyHandler.TypeHandler;
				object keyValue = keyHandler.ReflectionInfo.GetValue(obj);
				bool isKeyDefaultOfType = keyTypeHandler.IsTypeDefault(keyValue) || recursionChecker.PushReference(keyValue, "Key");

				if (!isKeyDefaultOfType) keyTypeHandler.Serialize(keyValue, output, indentation + 1, recursionChecker, "Key");

				recursionChecker.PopReference(keyValue);
			}

			{
				XMLFieldHandler valueHandler = _valueHandler.Value;
				XMLTypeHandler valueTypeHandler = valueHandler.TypeHandler;
				object valueValue = valueHandler.ReflectionInfo.GetValue(obj);

				bool isValueDefaultOfType = valueTypeHandler.IsTypeDefault(valueValue) || recursionChecker.PushReference(valueValue, "Value");
				if (!isValueDefaultOfType) valueTypeHandler.Serialize(valueValue, output, indentation + 1, recursionChecker, "Value");

				recursionChecker.PopReference(valueValue);
			}

			output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
		}

		/// <inheritdoc />
		public override object Deserialize(XMLReader input)
		{
			// This type is expected to be encountered only in the dictionary, where it use DeserializeKeyValue
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserialize a key and value objects. These are like any other objects except the two properties are "Key" and "Value"
		/// and the reflection handlers are for the specific key/value generic tuple pair.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void DeserializeKeyValue(XMLReader input, out object key, out object value)
		{
			key = _keyDefault;
			value = _valueDefault;

			int depth = input.Depth;
			input.GoToNextTag();
			while (input.Depth >= depth && !input.Finished)
			{
				XMLTypeHandler handler = XMLHelpers.GetInheritedTypeHandlerFromXMLTag(input, out string currentTag);
				switch (currentTag)
				{
					case "Key":
						if (handler == null) handler = _keyHandler.Value.TypeHandler;
						key = handler.Deserialize(input);
						break;
					case "Value":
						if (handler == null) handler = _valueHandler.Value.TypeHandler;
						value = handler.Deserialize(input);
						break;
					default:
						Engine.Log.Warning($"Unknown deserialization tag in KVP - {currentTag}.", MessageSource.XML);
						return;
				}

				input.GoToNextTag();
			}
		}
	}
}