#region Using

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.XML
{
    public class XmlComplexFieldHandler : XmlFieldHandler
    {
        /// <summary>
        /// Whether this complex type contains a reference to itself, or a base class of self.
        /// </summary>
        public override bool RecursionCheck
        {
            get => TypeHandler.PossibleRecursion;
        }

        /// <summary>
        /// The complex type analyzer.
        /// </summary>
        public XMLTypeHandler TypeHandler { get; }

        public XmlComplexFieldHandler(XmlReflectionHandler field, XMLTypeHandler typeHandler) : base(field)
        {
            TypeHandler = typeHandler;
        }

        public override void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker)
        {
            if (obj == null) return;

            if (RecursionCheck)
            {
                if (recursionChecker == null) recursionChecker = new XmlRecursionChecker();
                if (recursionChecker.PushReference(obj)) return;
            }

            XMLTypeHandler handler = TypeHandler;
            Type objType = obj.GetType();
            string derivedType = null;
            if (objType != TypeHandler.Type)
            {
                // Encountering a type which inherits from this type.
                if (TypeHandler.Type.IsAssignableFrom(objType))
                {
                    handler = XmlHelpers.GetTypeHandler(objType);
                    derivedType = XmlHelpers.GetTypeName(objType, true);
                }
                else
                {
                    // wtf?
                    Engine.Log.Warning($"Unknown object of type {objType.Name} was passed to handler of type {Name}", MessageSource.XML);
                    return;
                }
            }

            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append(derivedType != null ? $"<{Name} type=\"{derivedType}\">\n" : $"<{Name}>\n");
            handler.Serialize(obj, output, indentation + 1, recursionChecker);
            output.AppendJoin(XmlFormat.IndentChar, new string[indentation + 1]);
            output.Append($"</{Name}>\n");

            if (RecursionCheck) recursionChecker.PopReference(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Deserialize(XmlReader input)
        {
            return TypeHandler.Deserialize(input);
        }
    }
}