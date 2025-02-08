#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorComplexTypeHandler
{
    public ComplexTypeHandlerMember[] GetMembers();

    public IEnumerable<ComplexTypeHandlerMember> GetMembersDeep();

    public ComplexTypeHandlerMember? GetMemberByName(string name);
}