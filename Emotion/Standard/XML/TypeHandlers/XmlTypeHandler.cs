#region Using

using System;
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
        /// The serialization name of the type.
        /// </summary>
        public string TypeName { get; }

        protected XMLTypeHandler(Type type)
        {
            Type = type;
            TypeName = XMLHelpers.GetTypeName(Type);
        }

        public virtual bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>{obj}</{fieldName}>\n");
            return true;
        }

        public abstract object Deserialize(XMLReader input);

        /// <summary>
        /// Whether this type could potentially contain a reference of the specified type.
        /// Applies to reference types only.
        /// </summary>
        public virtual bool IsRecursiveWith(Type type)
        {
            return false;
        }
    }
}