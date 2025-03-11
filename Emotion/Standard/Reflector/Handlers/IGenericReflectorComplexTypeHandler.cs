#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IGenericReflectorComplexTypeHandler
{
    public object? CreateNew();

    public ComplexTypeHandlerMember[] GetMembers();

    public IEnumerable<ComplexTypeHandlerMember> GetMembersDeep();

    public ComplexTypeHandlerMember? GetMemberByName(string name);

    public ComplexTypeHandlerMember? GetMemberByName(int nameHash);

    public ComplexTypeHandlerMember? GetMemberByNameCaseInsensitive(string name);

    public ComplexTypeHandlerMember? GetMemberByNameCaseInsensitive(int nameHash);
}