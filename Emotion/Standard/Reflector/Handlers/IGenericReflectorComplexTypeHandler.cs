#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorComplexTypeHandler
{
    public object? CreateNew();

    public ComplexTypeHandlerMember[] GetMembers();

    public IEnumerable<ComplexTypeHandlerMember> GetMembersDeep();

    public ComplexTypeHandlerMember? GetMemberByName(string name);
}