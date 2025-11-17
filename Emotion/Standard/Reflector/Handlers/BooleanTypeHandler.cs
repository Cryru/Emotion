#nullable enable

using System.Text.Json;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Serialization.XML;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class BooleanTypeHandler : ReflectorTypeHandlerBase<bool>
{
    public override string TypeName => typeof(bool).Name;

    public override Type Type => typeof(bool);

    #region Serialization Read

    public override bool ParseFromJSON(ref Utf8JsonReader reader)
    {
        JsonTokenType token = reader.TokenType;
        if (token != JsonTokenType.True && token != JsonTokenType.False)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False);
        return reader.TokenType == JsonTokenType.True;
    }

    public override unsafe bool ParseFromXML(ref ValueStringReader reader)
    {
        char* readMemory = stackalloc char[12];
        Span<char> readMemorySpan = new Span<char>(readMemory, 12 * sizeof(char));

        int readChars = reader.ReadToNextOccuranceofChar('<', readMemorySpan);
        if (readChars == 0) return default;
        return readMemorySpan[0] == 't';
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(bool value, ref ValueStringWriter writer)
    {
        writer.WriteString(value ? "true" : "false");
    }

    public override void WriteAsXML(bool value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config)
    {
        if (addTypeTags)
            writer.WriteXMLTag(Type.Name, ValueStringWriter.XMLTagType.Normal);

        writer.WriteString(value ? "true" : "false");

        if (addTypeTags)
            writer.WriteXMLTag(Type.Name, ValueStringWriter.XMLTagType.Closing);
    }

    #endregion

    public override TypeEditor? GetEditor()
    {
        return new BooleanEditor();
    }
}
