#region Using

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace Emotion.Standard.TMX
{
    public interface ITmxElement
    {
        string Name { get; }
    }

    public class TmxList<T> : KeyedCollection<string, T> where T : ITmxElement
    {
        private Dictionary<string, int> _nameCount = new Dictionary<string, int>();

        public new void Add(T t)
        {
            string tName = t.Name;

            // Rename duplicate entries by appending a number
            if (Contains(tName))
                _nameCount[tName] += 1;
            else
                _nameCount.Add(tName, 0);
            base.Add(t);
        }

        protected override string GetKeyForItem(T item)
        {
            string name = item.Name;
            int count = _nameCount[name];

            var dupes = 0;

            // For duplicate keys, append a counter
            // For pathological cases, insert underscores to ensure uniqueness
            while (Contains(name))
            {
                name = name + string.Concat(Enumerable.Repeat("_", dupes))
                            + count;
                dupes++;
            }

            return name;
        }
    }
}