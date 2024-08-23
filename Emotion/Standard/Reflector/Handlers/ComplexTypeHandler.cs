#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public abstract class ComplexTypeHandler : IReflectorTypeHandler
{
    public Type Type { get; protected set; }

    public bool CanGetOrParseValueAsString => false;

    protected ComplexTypeHandler(Type t)
    {
        Type = t;
    }

    public abstract ComplexTypeHandlerMember[] GetMembers();
}

public class ComplexTypeHandler<T> : ComplexTypeHandler
{
    private ComplexTypeHandlerMember[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMember> _members;

    public ComplexTypeHandler(ComplexTypeHandlerMember[] members) : base(typeof(T))
    {
        _membersArr = members;
        _members = new Dictionary<string, ComplexTypeHandlerMember>();
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember member = members[i];
            _members.Add(member.Name, member);
        }
    }
    public override ComplexTypeHandlerMember[] GetMembers()
    {
        return _membersArr;
    }

    public ComplexTypeHandlerMember? GetMemberHandler(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember? member)) return member;
        return null;
    }
}
