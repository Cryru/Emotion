#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

namespace Emotion.Standard.Reflector.Handlers;

public class StaticComplexTypeHandler : IGenericReflectorComplexTypeHandler, IGenericReflectorTypeHandler
{
    public string TypeName { get; init; }

    public Type Type { get; init; }

    public bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMember[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMember> _members;
    private byte[][] _membersCaseInsensitive;

    public StaticComplexTypeHandler(Type typ, string typeName, ComplexTypeHandlerMember[] members)
    {
        Type = typ;
        TypeName = typeName;

        _membersArr = members;
        _members = new();
        _membersCaseInsensitive = new byte[members.Length][];
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember member = members[i];
            string memberName = member.Name;

            _members.Add(memberName, member);

            // Cache name as utf8 and with the first name uncapitalized.
            byte[] utf8Name = Helpers.UTF8Encoder.GetBytes(memberName);
            byte firstChar = utf8Name[0];
            byte lower = (firstChar >= 65 && firstChar <= 90) ? (byte)(firstChar + 32) : firstChar;
            utf8Name[0] = lower;
            _membersCaseInsensitive[i] = utf8Name;
        }
    }

    public bool CanCreateNew()
    {
        return false;
    }

    public object? CreateNew()
    {
        return null;
    }

    public TypeEditor? GetEditor()
    {
        return null; // Although this is technically possible :P
    }

    public void PostInit()
    {
        for (int i = 0; i < _membersArr.Length; i++)
        {
            ComplexTypeHandlerMember member = _membersArr[i];
            member.PostInit();
        }
    }

    public ComplexTypeHandlerMember[] GetMembers()
    {
        return _membersArr;
    }

    public ComplexTypeHandlerMember[] GetMembersDeep()
    {
        // Static types cant inherit so this is the same as members.
        return _membersArr;
    }

    public ComplexTypeHandlerMember? GetMemberByName(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }

    #region All Serialization (Disabled)

    public void WriteAsCode<OwnerT>(OwnerT? value, ref ValueStringWriter writer)
    {
        throw new NotImplementedException();
    }

    #endregion
}
