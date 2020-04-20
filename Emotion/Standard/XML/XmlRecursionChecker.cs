#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML
{
    public class XMLRecursionChecker
    {
        private HashSet<object> _references = new HashSet<object>();

        public bool PushReference(object obj)
        {
            bool duplicate = !_references.Add(obj);
            if (!duplicate) return false;
            Engine.Log.Warning("Tried to serialize a recursive reference.", MessageSource.XML);
            return true;
        }

        public void PopReference(object obj)
        {
            _references.Remove(obj);
        }
    }
}