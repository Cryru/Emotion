#region Using

using Emotion.Standard.XML.TypeHandlers;

#endregion

namespace Emotion.Standard.XML
{
    public class XMLFieldHandler
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
        public XMLReflectionHandler ReflectionInfo { get; private set; }

        /// <summary>
        /// Knows how to handle the type this field is of.
        /// </summary>
        public XMLTypeHandler TypeHandler { get; private set; }

        public XMLFieldHandler(XMLReflectionHandler field, XMLTypeHandler typeHandler)
        {
            ReflectionInfo = field;
            TypeHandler = typeHandler;
            Name = ReflectionInfo?.Name ?? XMLHelpers.GetTypeName(TypeHandler.Type);
        }
    }
}