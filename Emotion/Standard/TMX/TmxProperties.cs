#region Using

using System.Collections.Generic;
using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.Standard.TMX
{
    public class TmxProperties : XMLReaderAttributeHandler
    {
        private Dictionary<string, string>? _dict;

        public TmxProperties(Dictionary<string, string>? backingDict)
        {
            _dict = backingDict;
        }

        public override string? Attribute(string attributeName)
        {
            if (_dict == null) return null;
            return _dict.ContainsKey(attributeName) ? _dict[attributeName] : null;
        }

        public bool TryGetValue(string key, out string? value)
        {
            if (_dict != null) return _dict.TryGetValue(key, out value);

            value = null;
            return false;
        }

        public string? GetValueOrDefault(string key)
        {
            return _dict?.GetValueOrDefault(key);
        }

        public bool ContainsKey(string key)
        {
            return _dict?.ContainsKey(key) ?? false;
        }

        public string? this[string key]
        {
            get => _dict?[key] ?? null;
        }
    }
}