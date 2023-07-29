#region Using

using System.Globalization;
using System.Text;

#endregion

#nullable enable

namespace Emotion.Standard.XML.TypeHandlers
{
	/// <inheritdoc />
	public class XMLPrimitiveTypeHandler : XMLTypeHandler
	{
		/// <inheritdoc />
		public XMLPrimitiveTypeHandler(Type type, bool nonNullable) : base(type)
		{
			_defaultValue = nonNullable ? Activator.CreateInstance(Type, true) : null;
		}

		/// <inheritdoc />
		public override object? Deserialize(XMLReader input)
		{
			string readValue = input.GoToNextTag();
			return string.IsNullOrEmpty(readValue) ? _defaultValue : Convert.ChangeType(readValue, Type, CultureInfo.InvariantCulture);
		}
	}
}