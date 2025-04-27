#nullable enable

using Emotion.Serialization.XML;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public class StaticComplexTypeHandler : IGenericReflectorComplexTypeHandler, IGenericReflectorTypeHandler
{
    public string TypeName { get; init; }

    public Type Type { get; init; }

    public bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMemberBase[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMemberBase> _members;
    private byte[][] _membersCaseInsensitive;

    public StaticComplexTypeHandler(Type typ, string typeName, ComplexTypeHandlerMemberBase[] members)
    {
        Type = typ;
        TypeName = typeName;

        _membersArr = members;
        _members = new();
        _membersCaseInsensitive = new byte[members.Length][];
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMemberBase member = members[i];
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
            ComplexTypeHandlerMemberBase member = _membersArr[i];
            member.PostInit();
        }
    }

    public ComplexTypeHandlerMemberBase[] GetMembers()
    {
        return _membersArr;
    }

    public ComplexTypeHandlerMemberBase[] GetMembersDeep()
    {
        // Static types cant inherit so this is the same as members.
        return _membersArr;
    }

    public ComplexTypeHandlerMemberBase? GetMemberByName(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMemberBase? member))
            return member;
        return null;
    }

    #region All Serialization (Disabled)

    public void WriteAsCode<OwnerT>(OwnerT? value, ref ValueStringWriter writer)
    {
        throw new NotImplementedException();
    }

    public void WriteAsXML<OwnerT>(OwnerT? value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        throw new NotImplementedException();
    }

    public T? ParseFromJSON<T>(ref Utf8JsonReader reader)
    {
        throw new NotImplementedException();
    }

    public T? ParseFromXML<T>(ref ValueStringReader reader)
    {
        throw new NotImplementedException();
    }

    #endregion
}
