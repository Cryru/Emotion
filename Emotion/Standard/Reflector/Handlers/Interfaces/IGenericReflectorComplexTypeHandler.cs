#nullable enable

using Emotion;


#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IGenericReflectorComplexTypeHandler : IGenericReflectorTypeHandler
{
    public bool CanCreateNew();

    public object? CreateNew();

    public ComplexTypeHandlerMember[] GetMembers();

    public ComplexTypeHandlerMember[] GetMembersDeep();

    public ComplexTypeHandlerMember? GetMemberByName(string name);
}