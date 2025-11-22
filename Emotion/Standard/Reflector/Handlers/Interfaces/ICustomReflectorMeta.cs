#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface ICustomReflectorMeta<T>
{
    public abstract static ComplexTypeHandlerMemberBase[] GetExtraSerializationMembers();

    public abstract static T CustomCreateNew();
}
