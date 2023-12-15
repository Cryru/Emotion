#region Using

using Emotion.Common.Serialization;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// A member of a complex type that has been excluded from serialization.
    /// </summary>
    public class XMLSkippedMember : XMLFieldHandler
    {
        public XMLSkippedMember(ReflectedMemberHandler field) : base(field, null)
        {
            Skip = true;
        }
    }
}