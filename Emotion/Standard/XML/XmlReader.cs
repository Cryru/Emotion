#region Using

using System.Diagnostics;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Standard.XML
{
    public class XMLReader
    {
        public int Depth { get; private set; }
        public bool Finished { get; private set; }

        private readonly string _source;
        private int _offset;

        private static readonly Regex AttributeRegex = new Regex("([\\S]+)=\\\"([\\s\\S]*?)\\\"", RegexOptions.Compiled);

        public XMLReader(string s)
        {
            _source = s;
            _offset = 0;
        }

#if DEBUG
        public string GetDebugSource()
        {
            return _source.Substring(_offset);
        }
#endif

        /// <summary>
        /// Go to the next tag in the xml. Returns everything between the current position and the next tag.
        /// This is also used for getting tag values.
        /// The cursor is left one position after &lt;
        /// </summary>
        /// <returns>The data between the tags.</returns>
        public string GoToNextTag()
        {
            int valueStart = _offset + 1;

            for (; _offset < _source.Length; _offset++)
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

            if (_offset >= _source.Length - 1) Finished = true;
            return valueStart == _offset ? null : _source.Substring(valueStart, _offset - valueStart - 1);
        }

        /// <summary>
        /// Read the current tag and return its value.
        /// The cursor is left at the &gt;
        /// </summary>
        /// <param name="typeAttribute">An attribute with the name "type" if any.</param>
        /// <returns>What was read.</returns>
        public string ReadTag(out string typeAttribute)
        {
            int tagStart = _offset;
            var tagEnd = 0;
            typeAttribute = null;

            if (_source[_offset] != '/') Depth++;

            for (; _offset < _source.Length; _offset++)
            {
                char c = _source[_offset];

                // <tag attribute="value"
                if (c == ' ' && tagEnd == 0)
                {
                    tagEnd = _offset;
                    continue;
                }

                if (c != '>') continue; // tag>
                break;
            }

            // If the tag didn't explicitly end, the end is now.
            if (tagEnd == 0)
            {
                tagEnd = _offset;
            }
            else
            {
                // Otherwise there are some attributes
                Match match = AttributeRegex.Match(_source, tagEnd, _offset);
                if (match.Success && match.Groups.Count == 3)
                {
                    string name = match.Groups[1].Value;
                    string value = match.Groups[2].Value;

                    if (name == "type") typeAttribute = value;
                }
            }

            if (_offset == _source.Length - 1) Finished = true;
            return _source.Substring(tagStart, tagEnd - tagStart);
        }

        private char PeekOneAhead()
        {
            if (Finished) return '\0';
            return _offset + 1 >= _source.Length - 1 ? '\0' : _source[_offset + 1];
        }
    }
}