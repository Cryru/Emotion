#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// This is the X-Document like API of the XMLReader used outside the serialization logic.
    /// </summary>
    public partial class XMLReader
    {
        /// <summary>
        /// Name of the tag containing this XML element, if derived from a document.
        /// </summary>
        public string Name { get; private set; }

        private int _startOffset;
        private int _startDepth;
        private Dictionary<string, string> _attributes;

        private XMLReader(string source, int startOffset, int startDepth, Dictionary<string, string> attributes) : this(source)
        {
            _startOffset = startOffset;
            _offset = startOffset;
            _startDepth = startDepth;
            _attributes = attributes;
        }

        /// <summary>
        /// Reset the reader to the start of the xml file.
        /// </summary>
        public void Reset()
        {
            _offset = _startOffset;
            Depth = _startDepth;
            Finished = false;
        }

        /// <summary>
        /// Returns the xml element with the specified tag, or null if not found.
        /// </summary>
        /// <param name="tagName">The tag name of the element to return.</param>
        /// <returns>The xml element with the specified name.</returns>
        public XMLReader Element(string tagName)
        {
            Reset();
            return NextElement(tagName);
        }

        /// <summary>
        /// Returns the next XML element from the current read position.
        /// </summary>
        /// <returns></returns>
        public XMLReader Element()
        {
            return NextElement(null);
        }

        /// <summary>
        /// Returns the next element with the specified tag name.
        /// </summary>
        /// <param name="tagName">The tag name to search for.</param>
        /// <returns></returns>
        public XMLReader NextElement(string tagName)
        {
            int searchDepth = Depth;
            XMLReader foundTag = null;
            while (!Finished)
            {
                // Only search at the same depth.
                if (Depth > searchDepth)
                {
                    ReadTagWithoutAttribute();
                    GoToNextTag();
                    continue;
                }

                if (foundTag != null)
                {
                    foundTag._length = _offset - foundTag._offset - 1;
                    return foundTag;
                }

                // If found, record where it was found, and continue searching until we return to the same depth. Then we've found the closing tag.
                string tag = ReadTagWithAllAttributes(out Dictionary<string, string> lastAttributes);
                if (tagName == null || tag == tagName || tag == tagName + "/") foundTag = new XMLReader(_source, _offset + 1, Depth, lastAttributes) {Name = tagName ?? tag};
                GoToNextTag();
            }

            // Last element.
            if (Depth != searchDepth || foundTag == null) return null;
            foundTag._length = _offset - foundTag._offset - 1;
            return foundTag;
        }

        /// <summary>
        /// Returns all XML elements with the specified tag.
        /// </summary>
        /// <param name="elementTag">The tag of the elements to return.</param>
        /// <returns>All elements with this tag.</returns>
        public List<XMLReader> Elements(string elementTag)
        {
            Reset();

            var result = new List<XMLReader>();
            while (!Finished)
            {
                XMLReader element = NextElement(elementTag);
                if (element != null) result.Add(element);
            }

            return result;
        }

        /// <summary>
        /// Returns all elements contained in the xml.
        /// </summary>
        /// <returns>All elements contained in this xml.</returns>
        public List<XMLReader> Elements()
        {
            Reset();

            var result = new List<XMLReader>();
            while (!Finished)
            {
                XMLReader element = Element();
                if (element != null) result.Add(element);
            }

            return result;
        }

        /// <summary>
        /// Get the attribute of this name as a string (its pure format) or null if it doesn't exist.
        /// </summary>
        /// <param name="attributeName">The attribute of this tag of this name.</param>
        public string Attribute(string attributeName)
        {
            return _attributes.ContainsKey(attributeName) ? _attributes[attributeName] : null;
        }

        /// <summary>
        /// Get the attribute of this name as a nullable int.
        /// </summary>
        public int? AttributeIntN(string attributeName)
        {
            if (!_attributes.ContainsKey(attributeName)) return null;
            string value = _attributes[attributeName];
            if (!int.TryParse(value, out int result)) return null;
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
            if (!_attributes.ContainsKey(attributeName)) return null;
            string value = _attributes[attributeName];
            if (!uint.TryParse(value, out uint result)) return null;
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
            if (!_attributes.ContainsKey(attributeName)) return null;
            string value = _attributes[attributeName];
            if (!double.TryParse(value, out double result)) return null;
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
            if (!_attributes.ContainsKey(attributeName)) return null;
            string value = _attributes[attributeName];
            if (!float.TryParse(value, out float result)) return null;
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
            if (!_attributes.ContainsKey(attributeName)) return null;
            string value = _attributes[attributeName];
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
            if (!_attributes.ContainsKey(attributeName)) return default;
            if (!Enum.TryParse(typeof(T), _attributes[attributeName], true, out object value)) return default;
            return (T) value;
        }
    }
}