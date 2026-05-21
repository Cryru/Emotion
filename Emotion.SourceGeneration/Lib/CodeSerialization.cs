using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Emotion.CodeSerializationLib
{
    public class CodeSerialization
    {
        public static void Serialize(int indent, StringBuilder sb, Type typ, object obj)
        {
            string indentString = new string(' ', indent);
            foreach (PropertyInfo prop in typ.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
            {
                object value = prop.GetValue(obj);
                if (value == null) continue;

                string line = FormatAssignment(prop.Name, prop.PropertyType, value);
                if (line == null) continue;
                sb.AppendLine($"{indentString}{line}");
            }

            foreach (FieldInfo prop in typ.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = prop.GetValue(obj);
                if (value == null) continue;

                string line = FormatAssignment(prop.Name, prop.FieldType, value);
                if (line == null) continue;
                sb.AppendLine($"{indentString}{line}");
            }
        }

        private static string FormatAssignment(string name, Type type, object value)
        {
            if (type == typeof(string)) return $"{name} = \"{value}\";";
            if (type.IsPrimitive || type == typeof(decimal)) return $"{name} = {FormatPrimitive(value)};";

            if (value is IList list)
            {
                Type elemType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
                string items = string.Join(", ", list.Cast<object>().Select(e => FormatInline(elemType, e)));
                return $"{name} = new {FormatTypeName(type)} {{ {items} }};";
            }

            return string.Empty;
        }

        private static string FormatInline(Type type, object value)
        {
            if (value == null) return "null";
            if (type == typeof(string)) return $"\"{value}\"";
            if (type.IsPrimitive || type == typeof(decimal)) return FormatPrimitive(value);
            if (type.IsEnum) return $"{type.FullName}.{value}";

            // Inline object initializer for nested types
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
            var members = string.Join(", ", props
                .Select(p => (name: p.Name, val: p.GetValue(value), type: p.PropertyType))
                .Where(x => x.val != null)
                .Select(x => $"{x.name} = {FormatInline(x.type, x.val)}"));

            return $"new {FormatTypeName(type)} {{ {members} }}";
        }

        private static string FormatPrimitive(object value)
        {
            switch (value)
            {
                case float f:
                    return $"{f}f";
                case double d:
                    return $"{d}";
                case bool b:
                    return b ? "true" : "false";
                case char c:
                    return $"'{c}'";
                case long l:
                    return $"{l}L";
                case uint u:
                    return $"{u}u";
                case ulong ul:
                    return $"{ul}UL";
                default:
                    return value.ToString() ?? string.Empty;
            }
        }

        private static string FormatTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.FullName ?? string.Empty;

            string args = string.Join(", ", t.GetGenericArguments().Select(FormatTypeName));
            return $"{t.Namespace}.{t.Name.Split('`')[0]}<{args}>";
        }
    }
}
