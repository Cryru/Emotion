#region Using

using System;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    /// <summary>
    /// Object which knows how to handle the serialization and deserialization of complex types (non primitives).
    /// </summary>
    public abstract class XMLTypeHandler
    {
        /// <summary>
        /// The type this handler relates to.
        /// </summary>
        public Type Type { get; protected set; }

        /// <summary>
        /// Whether this type can be inherited from.
        /// </summary>
        public abstract bool CanBeInherited { get; }

        /// <summary>
        /// Whether this type requires recursion checking when being serialized. For instance
        /// arrays of arrays which can contain themselves and complex types with fields of
        /// the same type or a derived one.
        /// </summary>
        public virtual bool RecursiveType { get; protected set; }

        protected XMLTypeHandler(Type type)
        {
            Type = type;
        }

        public abstract void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker);
        public abstract object Deserialize(XmlReader input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool ShouldSerialize(object obj)
        {
            return obj != null;
        }

        /// <summary>
        /// Whether this type can contain a field derived from this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsRecursiveWith(Type type)
        {
            return Type.IsAssignableFrom(type);
        }
    }
}