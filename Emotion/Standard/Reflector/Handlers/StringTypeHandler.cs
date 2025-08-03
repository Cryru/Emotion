#nullable enable

using System.Text.Json;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class StringTypeHandler : ReflectorTypeHandlerBase<string>
{
    public override string TypeName => typeof(string).Name;

    public override Type Type => typeof(string);

    #region Serialization Read

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

    #endregion

    #region Serialization Write

    public override void WriteAsCode(string value, ref ValueStringWriter writer)
    {
        writer.WriteString($"\"{value}\"");
    }

    #endregion

    public override TypeEditor? GetEditor()
    {
        return new StringEditor();
    }
}
