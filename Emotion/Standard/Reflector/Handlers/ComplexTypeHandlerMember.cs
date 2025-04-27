#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
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

    public Attribute[] Attributes { get; set; } = Array.Empty<Attribute>();

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

    #region Serialization Read

    public abstract bool ParseFromJSON<OwnerT>(ref Utf8JsonReader reader, OwnerT intoObject);

    #endregion

    #region Serialization Write

    public abstract void WriteAsCode<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer);

    #endregion
}

// todo: nullable types
// todo: static member type
public class ComplexTypeHandlerMember<ParentT, MyT> : ComplexTypeHandlerMember
{
    protected Action<ParentT, MyT?> _setValue;
    protected Func<ParentT, MyT?> _getValue;

    private ReflectorTypeHandlerBase<MyT>? _typeHandler;

    public ComplexTypeHandlerMember(string name, Action<ParentT, MyT?> setValue, Func<ParentT, MyT?> getValue) : base(typeof(MyT), name)
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

    #region Serialization Read

    public override bool ParseFromJSON<OwnerT>(ref Utf8JsonReader reader, OwnerT intoObject)
    {
        if (intoObject is not ParentT parentT)
            return false;

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return false;

        MyT? val = handler.ParseFromJSON(ref reader);
        _setValue(parentT, val);
        return true;
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer)
    {
        if (ownerObject is not ParentT parentT)
            return;

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return;

        MyT? val = _getValue(parentT);
        handler.WriteAsCode(val, ref writer);
    }

    #endregion

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
