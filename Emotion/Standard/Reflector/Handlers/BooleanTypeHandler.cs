﻿#nullable enable

using System.Text.Json;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;

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
}
