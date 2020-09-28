#region Using

using System;
using System.Globalization;

#endregion

#nullable enable

namespace Emotion.Standard.XML
{
    public abstract class XMLReaderAttributeHandler
    {
        /// <summary>
        /// Get the attribute of this name as a string (its pure format) or null if it doesn't exist.
        /// </summary>
        /// <param name="attributeName">The attribute of this tag of this name.</param>
        public abstract string? Attribute(string attributeName);

        /// <summary>
        /// Get the attribute of this name as a nullable int.
        /// </summary>
        public int? AttributeIntN(string attributeName)
        {
            string? value = Attribute(attributeName);
            if (value == null) return null;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)) return null;
            return result;
        }

        /// <summary>
        /// Get the attribute of this name as an int.
        /// </summary>
        public int AttributeInt(string attributeName)
        {
            return AttributeIntN(attributeName) ?? default;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable uint.
        /// </summary>
        public uint? AttributeUIntN(string attributeName)
        {
            string? value = Attribute(attributeName);
            if (value == null) return null;
            if (!uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result)) return null;
            return result;
        }

        /// <summary>
        /// Get the attribute of this name as a uint.
        /// </summary>
        public uint AttributeUInt(string attributeName)
        {
            return AttributeUIntN(attributeName) ?? default;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable double.
        /// </summary>
        public double? AttributeDoubleN(string attributeName)
        {
            string? value = Attribute(attributeName);
            if (value == null) return null;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)) return null;
            return result;
        }

        /// <summary>
        /// Get the attribute of this name as a double.
        /// </summary>
        public double AttributeDouble(string attributeName)
        {
            return AttributeDoubleN(attributeName) ?? default;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable float.
        /// </summary>
        public float? AttributeFloatN(string attributeName)
        {
            string? value = Attribute(attributeName);
            if (value == null) return null;
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)) return null;
            return result;
        }

        /// <summary>
        /// Get the attribute of this name as a float.
        /// </summary>
        public float AttributeFloat(string attributeName)
        {
            return AttributeFloatN(attributeName) ?? default;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable bool.
        /// </summary>
        public bool? AttributeBoolN(string attributeName)
        {
            string? value = Attribute(attributeName);
            if (value == null) return null;
            // 0 and 1 aren't handled by bool.Parse but are valid.
            switch (value)
            {
                case "0":
                    return false;
                case "1":
                    return true;
            }

            if (!bool.TryParse(value, out bool result)) return null;
            return result;
        }

        /// <summary>
        /// Get the attribute of this name as a bool.
        /// </summary>
        public bool AttributeBool(string attributeName)
        {
            return AttributeBoolN(attributeName) ?? default;
        }

        /// <summary>
        /// Get the attribute of this name as an enum value.
        /// </summary>
        public T AttributeEnum<T>(string attributeName) where T : Enum
        {
            string? value = Attribute(attributeName);
            if (value == null) return default!;
            if (!Enum.TryParse(typeof(T), value, true, out object? enumValue)) return default!;
            return (T) enumValue!;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable enum value.
        /// </summary>
        public T? AttributeEnumN<T>(string attributeName) where T : struct
        {
            string? value = Attribute(attributeName);
            if (value == null) return default;
            if (!Enum.TryParse(typeof(T), value, true, out object? enumValue)) return default;
            return (T?) enumValue!;
        }
    }
}