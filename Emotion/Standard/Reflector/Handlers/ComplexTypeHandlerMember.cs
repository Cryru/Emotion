#nullable enable

using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Serialization.XML;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

// todo: nullable types
// todo: static member type
public class ComplexTypeHandlerMember<ParentT, MyT> : ComplexTypeHandlerMemberBase
{
    public delegate void SetValueFunc(ref ParentT parent, MyT? value);

    protected SetValueFunc _setValue;
    protected Func<ParentT, MyT?> _getValue;

    private ReflectorTypeHandlerBase<MyT>? _typeHandler;
    private bool _isComplex;

    public ComplexTypeHandlerMember(string name, SetValueFunc setValue, Func<ParentT, MyT?> getValue) : base(typeof(ParentT), typeof(MyT), name)
    {
        Name = name;

        _setValue = setValue;
        _getValue = getValue;
    }

    public override bool IsValueDefault<TOwner>(TOwner owner)
    {
        if (owner is not ParentT parentT)
            return false;

        MyT? val = _getValue(parentT);
        MyT? defaultVal = default;

        if (_typeHandler is ComplexTypeHandler<ParentT> complex && complex.DefaultInstance != null)
        {
            defaultVal = _getValue(complex.DefaultInstance);
        }

        return EqualityComparer<MyT>.Default.Equals(val, defaultVal);
    }

    public override void PostInit()
    {
        _typeHandler = ReflectorEngine.GetTypeHandler<MyT>();
        _isComplex = _typeHandler is IGenericReflectorComplexTypeHandler;
    }

    public override ReflectorTypeHandlerBase<MyT>? GetTypeHandler()
    {
        return _typeHandler;
    }

    #region Member

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
            _setValue(ref parentType, valType);
            return true;
        }
        return false;
    }

    public override object? SetValueInComplexObjectAndReturnParent(object obj, object? val)
    {
        if (obj is ParentT parentType && val is MyT valType)
        {
            _setValue(ref parentType, valType);
            return parentType;
        }
        return default;
    }

    #endregion

    #region Serialization Read

    public override bool ParseFromJSON<ParseIntoT>(ref Utf8JsonReader reader, ref ParseIntoT intoObject)
    {
        ref ParentT parentT = ref Unsafe.As<ParseIntoT, ParentT>(ref intoObject);

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return false;

        MyT? val = handler.ParseFromJSON(ref reader);
        _setValue(ref parentT, val);
        return true;
    }

    public override bool ParseFromXML<ParentT1>(ref ValueStringReader reader, ref ParentT1 intoObject)
    {
        ref ParentT parentT = ref Unsafe.As<ParentT1, ParentT>(ref intoObject);

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return false;

        MyT? val = handler.ParseFromXML(ref reader);
        _setValue(ref parentT, val);
        return true;
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer)
    {
        ref ParentT parentT = ref Unsafe.As<OwnerT, ParentT>(ref ownerObject);

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return;

        MyT? val = _getValue(parentT);
        handler.WriteAsCode<MyT>(val, ref writer);
    }

    public override void WriteAsXML<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        ref ParentT parentT = ref Unsafe.As<OwnerT, ParentT>(ref ownerObject);

        ReflectorTypeHandlerBase<MyT>? handler = _typeHandler;
        if (handler == null) return;

        if (addTypeTags)
        {
            if (config.Pretty)
            {
                writer.WriteChar('\n');
                writer.WriteIndent();
            }
        }

        MyT? val = _getValue(parentT);

        // If null write a closing tag
        if (val == null)
        {
            if (addTypeTags)
                writer.WriteXMLTag(Name, ValueStringWriter.XMLTagType.SelfClosing);

            return;
        }

        if (addTypeTags)
            writer.WriteXMLTag(Name, ValueStringWriter.XMLTagType.Normal);

        handler.WriteAsXML<MyT>(val, ref writer, false, config);

        if (addTypeTags)
        {
            if (_isComplex && config.Pretty)
            {
                writer.WriteChar('\n');
                writer.WriteIndent();
            }

            writer.WriteXMLTag(Name, ValueStringWriter.XMLTagType.Closing);
        }
    }

    #endregion
}
