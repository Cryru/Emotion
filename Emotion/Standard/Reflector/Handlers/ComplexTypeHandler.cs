#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Linq;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class ComplexTypeHandler<T> : ReflectorTypeHandlerBase<T>, IGenericReflectorComplexTypeHandler
{
    public override string TypeName => _typeName;

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    private ComplexTypeHandlerMember[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMember> _members;
    private byte[][] _membersCaseInsensitive;
    private Func<T>? _createNew;
    private string _typeName;

    private ComplexTypeHandlerMember[] _membersArrDeep;
    private byte[][] _membersCaseInsensitiveDeep;

    private bool _postInitCalled;

    public ComplexTypeHandler(Func<T>? createNew, string typeName, ComplexTypeHandlerMember[] members)
    {
        _createNew = createNew;
        _typeName = typeName;

        _membersArr = members;
        _members = new ();
        _membersCaseInsensitive = new byte[members.Length][];
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMember member = members[i];
            string memberName = member.Name;
            _members.Add(memberName, member);
        }
    }

    public override void PostInit()
    {
        for (int i = 0; i < _membersArr.Length; i++)
        {
            ComplexTypeHandlerMember member = _membersArr[i];

            string memberName = member.Name;

            // Cache name as utf8 and with the first name uncapitalized.
            byte[] utf8Name = Helpers.UTF8Encoder.GetBytes(memberName);
            byte firstChar = utf8Name[0];
            byte lower = (firstChar >= 65 && firstChar <= 90) ? (byte)(firstChar + 32) : firstChar;
            utf8Name[0] = lower;
            _membersCaseInsensitive[i] = utf8Name;

            member.PostInit();
        }

        // Load deep members.
        GetMembersDeep();
    }

    #region Serialization Read

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
                        ComplexTypeHandlerMember member = _membersArr[i];
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

    #endregion

    #region Serialization Write

    public override void WriteAsCode(T value, ref ValueStringWriter writer)
    {
        writer.WriteString("{\n");

        bool first = true;
        foreach (ComplexTypeHandlerMember member in GetMembersDeep())
        {
            if (!first) writer.WriteString(",\n");
            first = false;

            writer.WriteString(member.Name);
            writer.WriteString(" = ");
            member.WriteAsCode(value, ref writer);
        }

        writer.WriteString("}");
    }

    #endregion

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

    public bool CanCreateNew()
    {
        return _createNew != null;
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

    public ComplexTypeHandlerMember[] GetMembersDeep()
    {
        if (_membersArrDeep == null) // Lazy load, but post-init will load it for everyone.
        {
            ComplexTypeHandlerMember[] baseMembers = Array.Empty<ComplexTypeHandlerMember>();

            // My deep members are my base's deep members + my members.
            Type? baseType = Type.BaseType;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(ValueType))
            {
                IGenericReflectorComplexTypeHandler? baseHandler = ReflectorEngine.GetComplexTypeHandler(baseType);
                //AssertNotNull(baseHandler); // No base handler but have base type?
                if (baseHandler != null)
                    baseMembers = baseHandler.GetMembersDeep();
            }

            if (baseMembers.Length == 0)
            {
                _membersArrDeep = _membersArr;
                _membersCaseInsensitiveDeep = _membersCaseInsensitive;
            }
            else
            {
                List<ComplexTypeHandlerMember> filteredBaseMembers = new List<ComplexTypeHandlerMember>();

                // Sometimes the base class has duplicate members (such as when hiding/overloading is done)
                for (int i = 0; i < baseMembers.Length; i++)
                {
                    ComplexTypeHandlerMember member = baseMembers[i];
                    string memberName = member.Name;
                    if (_members.TryAdd(memberName, member))
                        filteredBaseMembers.Add(member);
                }

                for (int i = 0; i < _membersArr.Length; i++)
                {
                    ComplexTypeHandlerMember myMember = _membersArr[i];
                    filteredBaseMembers.Add(myMember);
                }

                _membersArrDeep = [.. filteredBaseMembers];
                _membersCaseInsensitiveDeep = new byte[_membersArrDeep.Length][];
            }
        }

        return _membersArrDeep;
    }

    public ComplexTypeHandlerMember? GetMemberByName(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMember? member))
            return member;
        return null;
    }
}
