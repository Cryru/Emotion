#nullable enable

using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI2;
using Emotion.Game.Systems.UI2.Editor;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Serialization.XML;
using System.Linq;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public class ComplexTypeHandler<T> : ReflectorTypeHandlerBase<T>, IGenericReflectorComplexTypeHandler
{
    public override string TypeName => _typeName;

    public override Type Type => typeof(T);

    private ComplexTypeHandlerMemberBase[] _membersArr;
    private Dictionary<string, ComplexTypeHandlerMemberBase> _members;
    private byte[][] _membersCaseInsensitive;
    private Func<T>? _createNew;
    private string _typeName;

    private ComplexTypeHandlerMemberBase[]? _membersArrDeep;
    private byte[][] _membersCaseInsensitiveDeep;

    public ComplexTypeHandler(Func<T>? createNew, string typeName, ComplexTypeHandlerMemberBase[] members)
    {
        _createNew = createNew;
        _typeName = typeName;

        _membersArr = members;
        _members = new ();
        _membersCaseInsensitive = new byte[members.Length][];
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMemberBase member = members[i];
            string memberName = member.Name;
            _members.Add(memberName, member);
        }

        _membersCaseInsensitiveDeep = _membersCaseInsensitive;
    }

    public override TypeEditor? GetEditor()
    {
        // todo: some way of specifying these, maybe static members on an interface that T could have
        if (typeof(T) == typeof(Vector2))
            return new VectorEditor(2);
        if (typeof(T) == typeof(Vector3))
            return new VectorEditor(3);
        if (typeof(T) == typeof(Vector4))
            return new VectorEditor(4);
        if (typeof(T) == typeof(Rectangle))
            return new VectorEditor(4, ["X", "Y", "Width", "Height"]);
        if (ReflectorEngine.IsTypeDescendedFrom(typeof(T), typeof(O_UITemplate)))
            return new UITemplateEditor();

        return new ComplexObjectEditor<T>();
    }

    public override void PostInit()
    {
        for (int i = 0; i < _membersArr.Length; i++)
        {
            ComplexTypeHandlerMemberBase member = _membersArr[i];
            member.PostInit();
        }
        DetermineCaseInsensitive(_membersArr, _membersCaseInsensitive);

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
                for (int i = 0; i < _membersCaseInsensitiveDeep.Length; i++)
                {
                    byte[] memberNameUtf8 = _membersCaseInsensitiveDeep[i];
                    if (keySpan.SequenceEqual(memberNameUtf8))
                    {
                        AssertNotNull(_membersArrDeep);
                        ComplexTypeHandlerMemberBase member = _membersArrDeep[i];
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
        //if (!writer.WriteIndent()) return;
        if (!writer.WriteString("{\n")) return;

        bool first = true;
        foreach (ComplexTypeHandlerMemberBase member in GetMembersDeep())
        {
            if (member.HasAttribute<DontSerializeButShowInEditorAttribute>() != null) continue;

            if (!first) if (!writer.WriteString(",\n")) return;
            first = false;

            writer.PushIndent();
            if (!writer.WriteIndent()) return;

            if (!writer.WriteString(member.Name)) return;
            if (!writer.WriteString(" = ")) return;
            member.WriteAsCode(value, ref writer);

            writer.PopIndent();
        }

        if (!writer.WriteString("}")) return;
    }

    public override void WriteAsXML(T value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        if (addTypeTags)
        {
            if (!writer.WriteChar('<')) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }

        foreach (ComplexTypeHandlerMemberBase member in GetMembersDeep())
        {
            member.WriteAsXML(value, ref writer, true, config, indent + config.Indentation);
        }

        if (addTypeTags)
        {
            if (config.Pretty)
            {
                if (!writer.WriteChar('\n')) return;
            }

            if (!writer.WriteString("</")) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }
    }

    #endregion

    public override bool CanCreateNew()
    {
        return _createNew != null;
    }

    public override object? CreateNew()
    {
        if (_createNew == null) return null;
        return _createNew();
    }

    public ComplexTypeHandlerMemberBase[] GetMembers()
    {
        return _membersArr;
    }

    public ComplexTypeHandlerMemberBase[] GetMembersDeep()
    {
        if (_membersArrDeep == null) // Lazy load, but post-init will load it for everyone.
        {
            ComplexTypeHandlerMemberBase[] baseMembers = Array.Empty<ComplexTypeHandlerMemberBase>();

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
                List<ComplexTypeHandlerMemberBase> filteredBaseMembers = new List<ComplexTypeHandlerMemberBase>();

                // Sometimes the base class has duplicate members (such as when hiding/overloading is done)
                for (int i = 0; i < baseMembers.Length; i++)
                {
                    ComplexTypeHandlerMemberBase member = baseMembers[i];
                    string memberName = member.Name;
                    if (_members.TryAdd(memberName, member))
                        filteredBaseMembers.Add(member);
                }

                for (int i = 0; i < _membersArr.Length; i++)
                {
                    ComplexTypeHandlerMemberBase myMember = _membersArr[i];
                    filteredBaseMembers.Add(myMember);
                }

                _membersArrDeep = [.. filteredBaseMembers];
                _membersCaseInsensitiveDeep = new byte[_membersArrDeep.Length][];
                DetermineCaseInsensitive(_membersArrDeep, _membersCaseInsensitiveDeep);
            }
        }

        return _membersArrDeep;
    }

    public ComplexTypeHandlerMemberBase? GetMemberByName(string name)
    {
        if (_members.TryGetValue(name, out ComplexTypeHandlerMemberBase? member))
            return member;
        return null;
    }

    private void DetermineCaseInsensitive(ComplexTypeHandlerMemberBase[] members, byte[][] utf8CaseInsensitive)
    {
        for (int i = 0; i < members.Length; i++)
        {
            ComplexTypeHandlerMemberBase member = members[i];

            string memberName = member.Name;

            // Cache name as utf8 and with the first name uncapitalized.
            byte[] utf8Name = Helpers.UTF8Encoder.GetBytes(memberName);
            byte firstChar = utf8Name[0];
            byte lower = firstChar >= 65 && firstChar <= 90 ? (byte)(firstChar + 32) : firstChar;
            utf8Name[0] = lower;
            utf8CaseInsensitive[i] = utf8Name;

            member.PostInit();
        }
    }
}
