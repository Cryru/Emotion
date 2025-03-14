#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using System.Text;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public enum CanWriteMemberResult
{
    CanWrite,
    InputError,
    NonTrivialMember,
    ValueIsNull,
}

public abstract class ComplexTypeHandlerMember
{
    public Type Type { get; protected set; }

    public string Name { get; set; }

    public Attribute[] Attributes = Array.Empty<Attribute>();

    protected ComplexTypeHandlerMember(Type memberType, string name)
    {
        Type = memberType;
        Name = name;
    }

    public abstract void PostInit();

    public T? HasAttribute<T>() where T : Attribute
    {
        for (int i = 0; i < Attributes.Length; i++)
        {
            var attribute = Attributes[i];
            if (attribute is T attributeAsT) return attributeAsT;
        }
        return null;
    }

    public abstract IGenericReflectorTypeHandler? GetTypeHandler();

    // This is the same as getting the type handler and calling WriteValueAsString, but avoids boxing.
    public bool UnsafeWriteValueAsStringFromComplexObject(StringBuilder builder, object? obj)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return UnsafeWriteValueAsStringFromComplexObject(ref writer, obj);
    }

    public abstract CanWriteMemberResult CanWriteValueAsStringFromComplexObject(object? obj);

    public abstract bool UnsafeWriteValueAsStringFromComplexObject(ref ValueStringWriter writer, object? obj);

    public abstract bool GetValueFromComplexObject(object obj, out object? readValue);

    public abstract bool SetValueInComplexObject(object obj, object? val);


}

public abstract class ComplexTypeHandlerMember<ParentT> : ComplexTypeHandlerMember
{
    protected ComplexTypeHandlerMember(Type memberType, string name) : base(memberType, name)
    {
    }

    public abstract bool ParseFromJSON(ref Utf8JsonReader reader, ParentT intoObject);
}

public class ComplexTypeHandlerMember<ParentT, MyT> : ComplexTypeHandlerMember<ParentT>
{
    protected Action<ParentT, MyT> _setValue;
    protected Func<ParentT, MyT> _getValue;

    private ReflectorTypeHandlerBase<MyT>? _typeHandler;

    public ComplexTypeHandlerMember(string name, Action<ParentT, MyT> setValue, Func<ParentT, MyT> getValue) : base(typeof(MyT), name)
    {
        _setValue = setValue;
        _getValue = getValue;
    }

    public override void PostInit()
    {
        _typeHandler = ReflectorEngine.GetTypeHandler<MyT>();
    }

    public override ReflectorTypeHandlerBase<MyT>? GetTypeHandler()
    {
        return _typeHandler;
    }

    public override bool GetValueFromComplexObject(object obj, out object? readValue)
    {
        readValue = null;
        if (obj is ParentT parentType)
        {
            readValue = _getValue(parentType);
            return true;
        }
        return false;
    }

    public override bool SetValueInComplexObject(object obj, object? val)
    {
        if (obj is ParentT parentType && val is MyT valType)
        {
            _setValue(parentType, valType);
            return true;
        }
        return false;
    }

    public override bool ParseFromJSON(ref Utf8JsonReader reader, ParentT intoObject)
    {
        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return false;

        MyT? val = handler.ParseFromJSON(ref reader);
        _setValue(intoObject, val);
        return true;
    }

    public override CanWriteMemberResult CanWriteValueAsStringFromComplexObject(object? obj)
    {
        if (obj is not ParentT parentType) return CanWriteMemberResult.InputError;

        MyT? val = _getValue(parentType);
        if (val == null) return CanWriteMemberResult.ValueIsNull;

        ReflectorTypeHandlerBase<MyT>? typeHandler = GetTypeHandler();
        if (typeHandler == null) return CanWriteMemberResult.InputError;
        if (!typeHandler.CanGetOrParseValueAsString) return CanWriteMemberResult.NonTrivialMember;

        return CanWriteMemberResult.CanWrite;
    }

    public override bool UnsafeWriteValueAsStringFromComplexObject(ref ValueStringWriter writer, object? obj)
    {
        if (obj is not ParentT parentType) return false;

        ReflectorTypeHandlerBase<MyT>? typeHandler = GetTypeHandler();
        if (typeHandler == null) return false;

        MyT? val = _getValue(parentType);
        typeHandler.WriteValueAsString(ref writer, val);
        return true;
    }
}
