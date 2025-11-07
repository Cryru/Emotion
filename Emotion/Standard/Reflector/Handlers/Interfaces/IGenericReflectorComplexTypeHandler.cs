#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IGenericReflectorComplexTypeHandler : IGenericReflectorTypeHandler
{
    public Type[] Interfaces { get; }

    public ComplexTypeHandlerMemberBase[] GetMembers();

    public ComplexTypeHandlerMemberBase[] GetMembersDeep();

    public ComplexTypeHandlerMemberBase? GetMemberByName(string name);
}