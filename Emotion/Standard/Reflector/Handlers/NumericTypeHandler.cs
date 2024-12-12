#nullable enable

using System.Globalization;

namespace Emotion.Standard.Reflector.Handlers;

public class NumericTypeHandler<T> : IReflectorTypeHandler where T : INumber<T>
{
    public Type Type => typeof(T);

    public bool CanGetOrParseValueAsString => true;

    public string GetValueAsString(object instance)
    {
        return instance.ToString() ?? string.Empty;
    }

    public object? ParseValueFromString(string val)
    {
        bool success = T.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out T? result);
        if (success) return result;
        return T.Zero;
    }
}
