#region Using

using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// Handles the interop between the serializers and C# reflection.
    /// </summary>
    public class ReflectedMemberHandler
    {
        /// <summary>
        /// The name of the property this object handles.
        /// </summary>
        public string Name { get; }

        private PropertyInfo _prop;
        private FieldInfo _field;

        /// <summary>
        /// Create a new reflection handler for the specified property.
        /// </summary>
        /// <param name="prop"></param>
        public ReflectedMemberHandler(PropertyInfo prop)
        {
            _prop = prop;
            Name = _prop.Name;
        }

        /// <summary>
        /// Create a new reflection handler for the specified field.
        /// </summary>
        /// <param name="field"></param>
        public ReflectedMemberHandler(FieldInfo field)
        {
            _field = field;
            Name = _field.Name;
        }

        /// <summary>
        /// Get the value of the field or property this handler manages from the object instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(object obj)
        {
            if (_prop != null) return _prop.GetValue(obj);
            return _field != null ? _field.GetValue(obj) : null;
        }

        /// <summary>
        /// Set the value of the field or property this handler manages from the object instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(object obj, object val)
        {
            if (_prop != null) _prop.SetValue(obj, val);
            if (_field != null) _field.SetValue(obj, val);
        }
    }
}