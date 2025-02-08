#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class StringTypeHandler : ReflectorTypeHandlerBase<string>
{
    public override Type Type => typeof(string);

    public override bool CanGetOrParseValueAsString => true;

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

    public override bool ParseValueAsString<TReader>(TReader reader, out string result)
    {
        ReadOnlySpan<byte> data = reader.GetDataFromCurrentPosition();
        result = Encoding.UTF8.GetString(data);
        return true;
    }
}
