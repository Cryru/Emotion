#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Globalization;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class PrimitiveNumericTypeHandler<T> : ReflectorTypeHandlerBase<T> where T : unmanaged, INumber<T>
{
    public override string TypeName => typeof(T).Name;

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => true;

    public override TypeEditor? GetEditor()
    {
        return new NumberEditor<T>();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, T instance)
    {
        return stringWriter.WriteNumber(instance);
    }

    public object? ParseValueFromString(string val)
    {
        bool success = T.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out T result);
        if (success) return result;
        return T.Zero;
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out T result)
    {
        bool success = T.TryParse(data, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        if (!success) result = T.Zero;
        return success;
    }
}
