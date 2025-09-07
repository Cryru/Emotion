#nullable enable

namespace Emotion.Standard.Reflector.Handlers.Interfaces;

public interface IReflectorEnumHandler
{
    public Type UnderlyingType { get; }

    public string[] GetNames();

    public Array GetValuesGeneric();

    public string GetValueName(object value);

    public bool TryParse(string str, out object result);

    public bool TryParse<TUnderlyingNumeric>(TUnderlyingNumeric numeric, out object result) where TUnderlyingNumeric : INumber<TUnderlyingNumeric>;
}

public interface IReflectorEnumHandler<T> : IReflectorEnumHandler
    where T : Enum
{
    public T[] GetValues();
}