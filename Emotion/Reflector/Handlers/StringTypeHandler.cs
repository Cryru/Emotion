#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Utility.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

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
