#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface ICustomReflectorMeta_ExtraMembers
{
    public abstract static ComplexTypeHandlerMemberBase[] GetExtraReflectorMembers();
}

public interface ICustomReflectorMeta_CustomCreateNew<T>
{
    public abstract static T CustomCreateNew();
}
