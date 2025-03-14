#nullable enable

using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class ComplexTypeHandler<T> : ReflectorTypeHandlerBase<T>, IGenericReflectorComplexTypeHandler
{
    public override string TypeName => _typeName;

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMember<T>[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMember<T>> _members;
    private byte[][] _membersCaseInsensitive;
    private Func<T>? _createNew;
    private string _typeName;

    public ComplexTypeHandler(Func<T>? createNew, string typeName, ComplexTypeHandlerMember<T>[] members)
    {
        _createNew = createNew;
        _typeName = typeName;

        _membersArr = members;
        _members = new ();
        _membersCaseInsensitive = new byte[members.Length][];
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember<T> member = members[i];
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

    public override T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.StartObject);

        T? val = _createNew != null ? _createNew() : default;
        if (val == null)
        {
            if (!reader.TrySkip())
            {
                Assert(false, $"Failed skip in JSON parsing an object {TypeName}");
            }
            return val;
        }

        while (reader.Read())
        {
            JsonTokenType token = reader.TokenType;
            if (token == JsonTokenType.PropertyName)
            {
                ReadOnlySpan<byte> keySpan = reader.ValueSpan;

                bool found = false;
                for (int i = 0; i < _membersCaseInsensitive.Length; i++)
                {
                    byte[] memberNameUtf8 = _membersCaseInsensitive[i];
                    if (keySpan.SequenceEqual(memberNameUtf8))
                    {
                        ComplexTypeHandlerMember<T> member = _membersArr[i];
                        found = member.ParseFromJSON(ref reader, val);
                        break;
                    }
                }

                if (!found)
                {
                    if (!reader.TrySkip())
                    {
                        Assert(false, $"Failed skip in JSON parsing an object {TypeName}");
                    }
                }
            }
            else if (token == JsonTokenType.EndObject)
            {
                break;
            }
            else
            {
                Assert(false, $"Unknown token {token} in JSON parsing an object {TypeName}");
            }
        }

        return val;
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

    public override void PostInit()
    {
        for (int i = 0; i < _membersArr.Length; i++)
        {
            ComplexTypeHandlerMember member = _membersArr[i];
            member.PostInit();
        }
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
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember<T>? member))
            return member;
        return null;
    }
}
