#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

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

    public override TypeEditor? GetEditor()
    {
        if (typeof(T) == typeof(Vector2))
            return new VectorEditor(2);
        if (typeof(T) == typeof(Vector3))
            return new VectorEditor(3);
        if (typeof(T) == typeof(Vector4))
            return new VectorEditor(4);
        if (typeof(T) == typeof(Rectangle))
            return new VectorEditor(4, ["X", "Y", "Width", "Height"]);

        return base.GetEditor();
    }

    public ComplexTypeHandlerMember[] GetMembers()
    {
        return _membersArr;
    }

    public IEnumerable<ComplexTypeHandlerMember> GetMembersDeep()
    {
        for (int i = 0; i < _membersArr.Length; i++)
        {
            ComplexTypeHandlerMember member = _membersArr[i];
            yield return member;
        }

        Type? baseType = Type.BaseType;
        if (baseType == null) yield break;

        IGenericReflectorTypeHandler? handler = ReflectorEngine.GetTypeHandler(baseType);
        if (handler == null) yield break; // Yikes?

        Assert(handler is IGenericReflectorComplexTypeHandler);
        if (handler is IGenericReflectorComplexTypeHandler complexHandler)
        {
            foreach (ComplexTypeHandlerMember member in complexHandler.GetMembersDeep())
            {
                yield return member;
            }
        }
    }

    public ComplexTypeHandlerMember? GetMemberByName(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember? member)) return member;
        return null;
    }

    public override bool ParseValueAsString<TReader>(TReader reader, out T? result)
    {
        result = default;
        return false;
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, T? instance)
    {
        return false;
    }
}
