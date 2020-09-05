#region Using

using System.Collections.Generic;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// Reads XML data. Used by the serialization/deserialization functionality but provides
    /// an API for conventional XML reading too.
    /// </summary>
    public sealed partial class XMLReader
    {
        /// <summary>
        /// The tag depth at the current position of the reader.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Whether the whole file has been read.
        /// </summary>
        public bool Finished { get; private set; }

        private readonly string _source;
        private int _offset;
        private int _length;

        private static readonly Regex AttributeRegex = new Regex("([\\S]+)=\\\"([\\s\\S]*?)\\\"", RegexOptions.Compiled);

        /// <summary>
        /// Create a new XML reader, used to read XML documents.
        /// </summary>
        /// <param name="s">String data for the xml document.</param>
        public XMLReader(string s)
        {
            _source = s;
            _offset = 0;
            _length = s.Length;
        }

        /// <summary>
        /// Get the contents of the XML string from the reader head up to the end.
        /// </summary>
        public string CurrentContents()
        {
            return _source.Substring(_offset, _length - (_offset - _startOffset));
        }

        /// <summary>
        /// Go to the next tag in the xml. Returns everything between the current position and the next tag.
        /// This is also used for getting tag values.
        /// The cursor is left one position after &lt;
        /// </summary>
        /// <returns>The data between the tags.</returns>
        public string GoToNextTag()
        {
            if (Finished) return null;

            int valueStart = _offset + 1;

            for (; _offset - _startOffset < _length; _offset++)
            {
                char c = _source[_offset];
                if (c != '<') continue; // <tag>
                char oneAhead = PeekOneAhead();
                switch (oneAhead)
                {
                    case '?': // <?xml
                        continue;
                    case '/': // </tag
                        Depth--;
                        break;
                }

                _offset++;
                break;
            }

            if (_offset - _startOffset >= _length - 1) Finished = true;
            return valueStart == _offset || valueStart == _offset + 1 ? null : _source.Substring(valueStart, _offset - valueStart - 1);
        }

        /// <summary>
        /// Used by serialization. Does not ensure you are at a tag.
        /// Read the current tag and return its value.
        /// The cursor is left at the &gt;
        /// Immediately closing tags are returned as tag/ regardless of attributes.
        /// </summary>
        /// <param name="typeAttribute">An attribute with the name "type" if any.</param>
        /// <returns>What was read.</returns>
        public string SerializationReadTagAndTypeAttribute(out string typeAttribute)
        {
            int tagStart = _offset;
            var tagEnd = 0;
            var immediatelyClosing = false; // Tags such as <tag/> are considered immediately closing.
            typeAttribute = null;

            if (Finished) return null;

            if (_source[_offset] != '/') Depth++; // NextTag stops after the <, the next character could be / or any letter.
            _offset++;

            for (; _offset - _startOffset < _length; _offset++)
            {
                char c = _source[_offset];

                switch (c)
                {
                    case ' ' when tagEnd == 0: // <tag attribute="value"
                        tagEnd = _offset;
                        continue;
                    case '/' when PeekOneAhead() == '>': // tag/>
                    case '?' when PeekOneAhead() == '>': // tag?>
                        immediatelyClosing = true;
                        Depth--; // tag/>
                        break;
                }

                if (c == '>') break; // tag>
            }

            // If the tag didn't explicitly end, the end is now.
            if (tagEnd == 0)
            {
                tagEnd = _offset;
                immediatelyClosing = false; // The slash will be a part of the name anyway.
            }
            else
            {
                // Otherwise there are some attributes
                Match match = AttributeRegex.Match(_source, tagEnd, _offset - tagEnd);
                if (match.Success && match.Groups.Count == 3)
                {
                    string name = match.Groups[1].Value;
                    string value = match.Groups[2].Value;

                    if (name == "type") typeAttribute = value;
                }
            }

            if (_offset - _startOffset == _length - 1) Finished = true;
            return _source[tagStart..tagEnd] + (immediatelyClosing ? "/" : "");
        }

        /// <summary>
        /// Ensures you are at a tag.
        /// Read the current tag and return its value.
        /// The cursor is left at the &gt;
        /// </summary>
        public string ReadTagWithoutAttribute()
        {
            if (PeekOneBehind() != '<') GoToNextTag();

            int tagStart = _offset;
            var tagEnd = 0;

            if (Finished) return null;

            if (_source[_offset] != '/') Depth++; // NextTag stops after the <, the next character could be / or any letter.
            _offset++;

            for (; _offset - _startOffset < _length; _offset++)
            {
                char c = _source[_offset];
                if (c == ' ' && tagEnd == 0) tagEnd = _offset;
                if ((c == '/' || c == '?') && PeekOneAhead() == '>') // tag/> and tag?>
                {
                    Depth--;
                    continue;
                }

                if (c == '>') break; // tag>
            }

            // If the tag didn't explicitly end, the end is now.
            if (tagEnd == 0) tagEnd = _offset;

            if (_offset - _startOffset == _length - 1) Finished = true;
            return _source[tagStart..tagEnd];
        }

        /// <summary>
        /// Ensures you are at a tag.
        /// Read the current tag and return its value and attributes.
        /// The cursor is left at the &gt;
        /// </summary>
        public string ReadTagWithAllAttributes(out Dictionary<string, string> attributes)
        {
            if (PeekOneBehind() != '<') GoToNextTag();

            int tagStart = _offset;
            var tagEnd = 0;
            var immediatelyClosing = false; // Tags such as <tag/> are considered immediately closing.
            attributes = null;

            if (Finished) return null;

            if (_source[_offset] != '/') Depth++; // NextTag stops after the <, the next character could be / or any letter.
            _offset++;

            for (; _offset - _startOffset < _length; _offset++)
            {
                char c = _source[_offset];

                switch (c)
                {
                    case ' ' when tagEnd == 0: // <tag attribute="value"
                        tagEnd = _offset;
                        continue;
                    case '/' when PeekOneAhead() == '>': // tag/>
                    case '?' when PeekOneAhead() == '>': // tag?>
                        immediatelyClosing = true;
                        Depth--;
                        break;
                }

                if (c == '>') break; // tag>
            }

            // If the tag didn't explicitly end, the end is now.
            if (tagEnd == 0)
            {
                tagEnd = _offset;
                immediatelyClosing = false; // The slash will be a part of the name anyway.
            }
            else
            {
                // Otherwise there are some attributes
                attributes = new Dictionary<string, string>();
                Match match = AttributeRegex.Match(_source, tagEnd, _offset - tagEnd);
                while (match.Success && match.Groups.Count == 3)
                {
                    attributes.Add(match.Groups[1].Value, match.Groups[2].Value);
                    match = match.NextMatch();
                }
            }

            if (_offset - _startOffset == _length - 1) Finished = true;
            return _source[tagStart..tagEnd] + (immediatelyClosing ? "/" : "");
        }

        /// <summary>
        /// Get the next character in the XML if any.
        /// </summary>
        /// <returns></returns>
        private char PeekOneAhead()
        {
            if (Finished) return '\0';
            return _offset + 1 - _startOffset >= _length - 1 ? '\0' : _source[_offset + 1];
        }

        /// <summary>
        /// Get the previous character in the XML if any.
        /// </summary>
        /// <returns></returns>
        private char PeekOneBehind()
        {
            if (Finished) return '\0';
            return _offset - 1 < _startOffset ? '\0' : _source[_offset - 1];
        }
    }
}