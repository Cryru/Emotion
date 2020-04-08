#region Using

using System.IO;
using System.Text;

#endregion

namespace Emotion.Standard.XML
{
    public abstract class XmlFieldHandler
    {
        /// <summary>
        /// The name of the field. If within a complex class this is the field name, if
        /// within an array this is the type name.
        /// </summary>
        public string Name
        {
            get => ReflectionInfo.GetName();
        }

        /// <summary>
        /// Whether this field could contain a reference to itself.
        /// </summary>
        public virtual bool RecursionCheck { get; set; }

        /// <summary>
        /// Contains information on how to set/get the value of this field, and its type.
        /// Within arrays this only contains the type.
        /// </summary>
        public XmlReflectionHandler ReflectionInfo { get; private set; }

        protected XmlFieldHandler(XmlReflectionHandler field)
        {
            ReflectionInfo = field;
        }

        public abstract void Serialize(object obj, StringBuilder output, int indentation, XmlRecursionChecker recursionChecker);
        public abstract object Deserialize(XmlReader input);
    }
}