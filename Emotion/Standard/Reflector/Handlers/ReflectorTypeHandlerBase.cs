﻿#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public abstract class ReflectorTypeHandlerBase<T> : IGenericReflectorTypeHandler
{
    public abstract string TypeName { get; }

    public abstract Type Type { get; }

    public abstract bool CanGetOrParseValueAsString { get; }

    public virtual TypeEditor? GetEditor()
    {
        return null;
    }

    public virtual void PostInit()
    {
        // nop
    }

    public virtual T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        return default;
    }

    public bool WriteValueAsStringGeneric<TParam>(ref ValueStringWriter stringWriter, TParam? instance)
    {
        if (instance is not T instanceAsT) return false;
        return WriteValueAsString(ref stringWriter, instanceAsT);
    }

    public bool WriteValueAsStringGeneric<TParam>(StringBuilder builder, TParam? instance)
    {
        if (instance is not T instanceAsT) return false;
        return WriteValueAsString(builder, instanceAsT);
    }

    public bool WriteValueAsString(StringBuilder builder, T? instance)
    {
        ValueStringWriter writer = new ValueStringWriter(builder);
        return WriteValueAsString(ref writer, instance);
    }

    public virtual bool WriteValueAsString(ref ValueStringWriter stringWriter, T? instance)
    {
        return false;
    }

    public bool ParseValueFromStringGeneric<TParam>(ReadOnlySpan<char> data, out TParam? result)
    {
        result = default;

        var success = ParseValueAsString(data, out T? resultAsType);
        if (!success) return false;

        if (resultAsType is TParam tAsParam)
        {
            result = tAsParam;
            return true;
        }

        return false;
    }

    public virtual bool ParseValueAsString(ReadOnlySpan<char> data, out T? result)
    {
        result = default;
        return false;
    }
}