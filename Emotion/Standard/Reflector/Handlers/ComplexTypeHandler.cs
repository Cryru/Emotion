#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class ComplexTypeHandler<T> : ReflectorTypeHandlerBase<T>, IGenericReflectorComplexTypeHandler
{
    public override string TypeName => _typeName;

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMember[] _membersArr;
    private Dictionary<int, ComplexTypeHandlerMember> _members;
    private Dictionary<int, ComplexTypeHandlerMember> _membersCaseInsensitive;
    private Func<T>? _createNew;
    private string _typeName;

    public ComplexTypeHandler(Func<T>? createNew, string typeName, ComplexTypeHandlerMember[] members)
    {
        _createNew = createNew;
        _typeName = typeName;

        _membersArr = members;
        _members = new Dictionary<int, ComplexTypeHandlerMember>();
        _membersCaseInsensitive = new Dictionary<int, ComplexTypeHandlerMember>();
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember member = members[i];
            string name = member.Name;
            _members.Add(name.GetStableHashCode(), member);

            string nameLower = name.ToLowerInvariant();
            _membersCaseInsensitive.Add(nameLower.GetStableHashCode(), member);
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

        return new NestedComplexObjectEditor();
    }

    public object? CreateNew()
    {
        if (_createNew == null) return null;
        return _createNew();
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
        int hash = name.GetStableHashCode();
        if (_members.TryGetValue(hash, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }

    public ComplexTypeHandlerMember? GetMemberByName(int nameHash)
    {
        if (_members.TryGetValue(nameHash, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }

    public ComplexTypeHandlerMember? GetMemberByNameCaseInsensitive(string name)
    {
        int hash = name.GetStableHashCode();
        if (_membersCaseInsensitive.TryGetValue(hash, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }

    public ComplexTypeHandlerMember? GetMemberByNameCaseInsensitive(int nameHash)
    {
        if (_membersCaseInsensitive.TryGetValue(nameHash, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out T? result)
    {
        result = default;
        return false;
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, T? instance)
    {
        return false;
    }
}
