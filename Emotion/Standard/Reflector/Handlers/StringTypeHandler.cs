﻿#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class StringTypeHandler : ReflectorTypeHandlerBase<string>
{
    public override string TypeName => typeof(string).Name;

    public override Type Type => typeof(string);

    public override bool CanGetOrParseValueAsString => true;

    public override string? ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.String);

        return reader.GetString();
    }

    public override TypeEditor? GetEditor()
    {
        return new StringEditor();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, string? instance)
    {
        if (instance == null) return false;
        return stringWriter.WriteString(instance);
    }

    public object? ParseValueFromString(string val)
    {
        return val;
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out string result)
    {
        result = new string(data);
        return true;
    }
}
