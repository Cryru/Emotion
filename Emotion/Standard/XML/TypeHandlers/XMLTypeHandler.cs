#region Using

using System;
using System.Globalization;
using System.Text;

#endregion

namespace Emotion.Standard.XML.TypeHandlers
{
    /// <summary>
    /// Object which knows how to handle the serialization and deserialization of objects.
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

        /// <summary>
        /// Create a handler for a type.
        /// </summary>
        /// <param name="type">The type to handle.</param>
        protected XMLTypeHandler(Type type)
        {
            Type = type;
            TypeName = XMLHelpers.GetTypeName(Type);
        }

        /// <summary>
        /// Serialize the object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="output"></param>
        /// <param name="indentation"></param>
        /// <param name="recursionChecker"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public virtual bool Serialize(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker recursionChecker = null, string fieldName = null)
        {
            if (obj == null) return false;

            fieldName ??= TypeName;
            output.AppendJoin(XMLFormat.IndentChar, new string[indentation]);
            output.Append($"<{fieldName}>{Convert.ToString(obj, CultureInfo.InvariantCulture)}</{fieldName}>\n");
            return true;
        }

        /// <summary>
        /// Deserialize the object.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public abstract object Deserialize(XMLReader input);
    }
}