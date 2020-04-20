#region Using

using System;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// Handles trivial types such as primitives, strings, and enums.
    /// </summary>
    public class XmlTrivialFieldHandler : XmlFieldHandler
    {
        private object _defaultValue;
        private bool _isEnum;
        private bool _isString;

        public XmlTrivialFieldHandler(XmlReflectionHandler field) : base(field)
        {
            _defaultValue = field.Type.IsValueType && field.Opaque ? Activator.CreateInstance(field.Type, true) : null;
            _isString = field.Type == StringType;
            _isEnum = field.Type.IsEnum;
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker _)
        {
            if (obj == null || obj.Equals(_defaultValue)) return;

            if (_isString) obj = SanitizeString((string) obj);

            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append($"<{Name}>");
            output.Append($"{obj}");
            output.Append($"</{Name}>\n");
        }

        public override object Deserialize(XmlReader input)
        {
            string readValue = input.GoToNextTag();

            if (!_isEnum) return _isString ? RestoreString(readValue) : Convert.ChangeType(readValue, ReflectionInfo.Type);

            return Enum.Parse(ReflectionInfo.Type, readValue);
        }

        private static readonly Type StringType = typeof(string);
        private static readonly Type NullableType = typeof(Nullable<>);

        public static bool TypeIsTrivial(Type type)
        {
            return type.IsPrimitive || type == StringType || type.IsEnum || type == NullableType;
        }

        #region StringSanitation

        private static readonly Regex StringSanitizeRegex = new Regex("<", RegexOptions.Compiled);
        private static readonly Regex StringRestoreRegex = new Regex("&lt;", RegexOptions.Compiled);

        private static string SanitizeString(string str)
        {
            return StringSanitizeRegex.Replace(str, "&lt;");
        }

        private static string RestoreString(string str)
        {
            return StringRestoreRegex.Replace(str, "<");
        }

        #endregion
    }
}