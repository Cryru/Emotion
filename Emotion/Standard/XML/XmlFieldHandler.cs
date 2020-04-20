#region Using

using System;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML.TypeHandlers;

#endregion

namespace Emotion.Standard.XML
{
    public class XmlFieldHandler
    {
        /// <summary>
        /// The name of the field. If within a complex class this is the field name, if
        /// within an array this is the type name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Contains information on how to set/get the value of this field, and its type.
        /// Within arrays this only contains the type.
        /// </summary>
        public XmlReflectionHandler ReflectionInfo { get; private set; }

        /// <summary>
        /// Knows how to handle the type this field is of.
        /// </summary>
        public XMLTypeHandler TypeHandler { get; private set; }

        /// <summary>
        /// Whether the field is of an opaque type.
        /// </summary>
        public bool OpaqueField { get; }

        public XmlFieldHandler(XmlReflectionHandler field, XMLTypeHandler typeHandler, bool opaqueType)
        {
            ReflectionInfo = field;
            TypeHandler = typeHandler;
            OpaqueField = opaqueType;
            Name = ReflectionInfo?.Name ?? XmlHelpers.GetTypeName(TypeHandler.Type);
        }

        public void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;

            XMLTypeHandler handler = GetDerivedTypeHandler(obj, out string derivedType);
            if (OpaqueField && !handler.ShouldSerialize(obj)) return;
            if (TypeHandler.RecursiveType)
            {
                if (recursionChecker == null) recursionChecker = new XmlRecursionChecker();
                if (recursionChecker.PushReference(obj)) return;
            }

            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append(derivedType != null ? $"<{Name} type=\"{derivedType}\">" : $"<{Name}>");
            handler.Serialize(obj, output, indentation + 1, recursionChecker);
            output.Append($"</{Name}>\n");

            if (TypeHandler.RecursiveType) recursionChecker.PopReference(obj);
        }

        public XMLTypeHandler GetDerivedTypeHandler(object obj, out string derivedType)
        {
            XMLTypeHandler handler = TypeHandler;
            derivedType = null;
            if (obj == null || !handler.CanBeInherited) return handler;

            Type objType = obj.GetType();
            if (objType == handler.Type) return handler;

            // Encountering a type which inherits from this type.
            if (handler.Type.IsAssignableFrom(objType))
            {
                handler = XmlHelpers.GetTypeHandler(objType);
                derivedType = XmlHelpers.GetTypeName(objType, true);
            }
            else
            {
                // wtf?
                Engine.Log.Warning($"Unknown object of type {objType.Name} was passed to handler of type {Name}", MessageSource.XML);
                return handler;
            }

            return handler;
        }

        public object Deserialize(XmlReader input)
        {
            return TypeHandler.Deserialize(input);
        }
    }
}