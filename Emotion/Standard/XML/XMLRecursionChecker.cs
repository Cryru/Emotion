﻿namespace Emotion.Standard.XML
{
    /// <summary>
    /// Handles checking for recursion when serializing.
    /// </summary>
    public class XMLRecursionChecker
    {
        private HashSet<object> _references = new HashSet<object>();

        /// <summary>
        /// Push an object reference.
        /// The return value is whether the object reference is unique. If false is returned
        /// you've encountered recursion.
        /// </summary>
        public bool PushReference(object obj, string fieldName)
        {
            bool duplicate = !_references.Add(obj);
            if (!duplicate) return false;
            Engine.Log.Warning($"Tried to serialize a recursive reference in field {fieldName}.", MessageSource.XML);
            return true;
        }

        /// <summary>
        /// Pop an object reference.
        /// </summary>
        /// <param name="obj"></param>
        public void PopReference(object obj)
        {
            _references.Remove(obj);
        }
    }
}