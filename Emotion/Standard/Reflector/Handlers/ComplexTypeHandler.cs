#nullable enable

using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class ComplexTypeHandler<T> : ReflectorTypeHandlerBase<T>, IGenericReflectorComplexTypeHandler
{
    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMember[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMember> _members;

    public ComplexTypeHandler(ComplexTypeHandlerMember[] members)
    {
        _membersArr = members;
        _members = new Dictionary<string, ComplexTypeHandlerMember>();
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember member = members[i];
            _members.Add(member.Name, member);
        }
    }
    public ComplexTypeHandlerMember[] GetMembers()
    {
        return _membersArr;
    }

    public ComplexTypeHandlerMember? GetMemberHandler(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember? member)) return member;
        return null;
    }

    public override bool WriteValueAsString(StringBuilder builder, T? instance)
    {
        return false;
    }

    public override bool ParseValueAsString<TReader>(TReader reader, out T? result)
    {
        result = default;
        return false;
    }
}
