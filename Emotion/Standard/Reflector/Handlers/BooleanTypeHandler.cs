#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class BooleanTypeHandler : ReflectorTypeHandlerBase<bool>
{
    public override string TypeName => typeof(bool).Name;

    public override Type Type => typeof(bool);

    public override bool CanGetOrParseValueAsString => true;

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

    #endregion

    #region Serialization Write

    public override void WriteAsCode(bool value, ref ValueStringWriter writer)
    {
        writer.WriteString(value ? "true" : "false");
    }

    #endregion

    public override TypeEditor? GetEditor()
    {
        return new BooleanEditor();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, bool instance)
    {
        return stringWriter.WriteString(instance ? bool.TrueString : bool.FalseString);
    }

    public object? ParseValueFromString(string val)
    {
        return bool.TryParse(val, out bool result);
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out bool result)
    {
        return bool.TryParse(data, out result);
    }
}
