#region Using

using System.Collections.Generic;

#endregion

#nullable enable

namespace Emotion.Standard.XML
{
    /// <summary>
    /// This is the X-Document like API of the XMLReader used outside the serialization logic.
    /// </summary>
    public partial class XMLReader : XMLReaderAttributeHandler
    {
        /// <summary>
        /// Name of the tag containing this XML element, if derived from a document.
        /// </summary>
        public string? Name { get; private set; }

        private int _startOffset;
        private int _startDepth;
        private Dictionary<string, string>? _attributes;

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
        public XMLReader? Element(string tagName)
        {
            Reset();
            return NextElement(tagName);
        }

        /// <summary>
        /// Returns the next XML element from the current read position.
        /// </summary>
        /// <returns></returns>
        public XMLReader? Element()
        {
            return NextElement(null);
        }

        /// <summary>
        /// Returns the next element with the specified tag name.
        /// </summary>
        /// <param name="tagName">The tag name to search for. If null returns the current element.</param>
        /// <returns></returns>
        public XMLReader? NextElement(string? tagName)
        {
            int searchDepth = Depth;
            XMLReader? foundTag = null;
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
                XMLReader? element = NextElement(elementTag);
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
                XMLReader? element = Element();
                if (element != null) result.Add(element);
            }

            return result;
        }

        /// <inheritdoc />
        public override string? Attribute(string attributeName)
        {
            if (_attributes == null) return null;
            return _attributes.ContainsKey(attributeName) ? _attributes[attributeName] : null;
        }
    }
}