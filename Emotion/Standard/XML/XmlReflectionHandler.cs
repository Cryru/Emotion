#region Using

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Standard.XML
{
    public class XMLReflectionHandler
    {
        public string Name { get; }

        private PropertyInfo _prop;
        private FieldInfo _field;

        public XMLReflectionHandler(PropertyInfo prop)
        {
            _prop = prop;
            Name = _prop.Name;
        }

        public XMLReflectionHandler(FieldInfo field)
        {
            _field = field;
            Name = _field.Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(object obj)
        {
            if (_prop != null) return _prop.GetValue(obj);
            return _field != null ? _field.GetValue(obj) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(object obj, object val)
        {
            if (_prop != null) _prop.SetValue(obj, val);
            if (_field != null) _field.SetValue(obj, val);
        }
    }
}