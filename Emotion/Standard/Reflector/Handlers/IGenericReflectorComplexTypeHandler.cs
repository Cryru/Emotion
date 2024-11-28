#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorComplexTypeHandler
{
    public ComplexTypeHandlerMember[] GetMembers();

    public ComplexTypeHandlerMember? GetMemberHandler(string name);
}