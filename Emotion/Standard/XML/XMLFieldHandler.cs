#region Using

using Emotion.Common.Serialization;
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
        public ReflectedMemberHandler ReflectionInfo { get; private set; }

        /// <summary>
        /// Knows how to handle the type this field is of.
        /// </summary>
        public XMLTypeHandler TypeHandler { get; private set; }

        /// <summary>
        /// The default value for this field.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Whether this field is excluded and should be skipped during deserialization/serialization.
        /// </summary>
        public bool Skip { get; set; }

        public XMLFieldHandler(ReflectedMemberHandler field, XMLTypeHandler typeHandler)
        {
            ReflectionInfo = field;
            TypeHandler = typeHandler;
            Name = ReflectionInfo?.Name ?? XMLHelpers.GetTypeName(TypeHandler.Type);
        }

        public void SetDefaultValue(object defaultConstruct)
        {
            DefaultValue = ReflectionInfo.GetValue(defaultConstruct);
        }
    }
}