#region Using

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Standard.XML
{
    public class XmlReflectionHandler
    {
        public bool NoValue { get; }
        public Type Type { get; }
        public string TypeName { get; private set; }
        public bool Opaque { get; private set; }

        private PropertyInfo _prop;
        private FieldInfo _field;

        public XmlReflectionHandler(PropertyInfo prop)
        {
            _prop = prop;
            Type = XmlHelpers.GetOpaqueType(prop.PropertyType, out bool opaque);
            Opaque = opaque;
            TypeName = XmlHelpers.GetTypeName(Type);
        }

        public XmlReflectionHandler(FieldInfo field)
        {
            _field = field;
            Type = XmlHelpers.GetOpaqueType(field.FieldType, out bool opaque);
            Opaque = opaque;
            TypeName = XmlHelpers.GetTypeName(Type);
        }

        public XmlReflectionHandler(Type type)
        {
            NoValue = true;
            Type = XmlHelpers.GetOpaqueType(type, out bool opaque);
            Opaque = opaque;
            TypeName = XmlHelpers.GetTypeName(Type);
        }

        public string GetName()
        {
            if (_prop != null) return _prop.Name;
            return _field != null ? _field.Name : TypeName;
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