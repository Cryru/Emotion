#region Using

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    public class XmlStringTypeHandler : XMLTypeHandler
    {
        public override bool CanBeInherited { get => false; }

        public XmlStringTypeHandler(Type type) : base(type)
        {
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;
            obj = SanitizeString((string) obj);
            output.Append($"{obj}");
        }

        public override object Deserialize(XmlReader input)
        {
            string readValue = input.GoToNextTag();
            return RestoreString(readValue);
        }

        private static readonly Regex StringSanitizeRegex = new Regex("<", RegexOptions.Compiled);
        private static readonly Regex StringRestoreRegex = new Regex("&lt;", RegexOptions.Compiled);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string SanitizeString(string str)
        {
            return StringSanitizeRegex.Replace(str, "&lt;");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string RestoreString(string str)
        {
            return StringRestoreRegex.Replace(str, "<");
        }
    }
}