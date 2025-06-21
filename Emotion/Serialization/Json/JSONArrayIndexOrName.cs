using Emotion.Game.Data;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using System;
using System.Text.Json;

#nullable enable

namespace Emotion.Serialization.JSON;

/// <summary>
/// Allows handling of cases in which a JSON value can be either an object with keys or an array.
/// </summary>
public interface IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }
}

/// <summary>
/// Allows handling of cases in which the the JSON value is reference to an array index or key name.
/// </summary>
public struct JSONArrayIndexOrName
{
    public bool Valid;
    public bool IsIndex;
    public int ReferenceAsIndex;
    public string? ReferenceAsName;

    public JSONArrayIndexOrName(int idx)
    {
        ReferenceAsIndex = idx;
        IsIndex = true;
        Valid = true;
    }

    public JSONArrayIndexOrName(string name)
    {
        ReferenceAsName = name;
        IsIndex = false;
        Valid = true;
    }

    public readonly T? GetReferenced<T>(T[] array) where T : IJSONIndexOrNameReferencable
    {
        if (array.Length == 0 || !Valid)
            return default;

        if (IsIndex)
            return array[ReferenceAsIndex];

        foreach (T item in array)
        {
            if (item.JSON_NAMED_ARRAY_ID == ReferenceAsName)
                return item;
        }

        return default;
    }

    public static bool operator ==(JSONArrayIndexOrName a, JSONArrayIndexOrName b)
    {
        if (a.IsIndex)
        {
            if (!b.IsIndex) return false;
            return a.ReferenceAsIndex == b.ReferenceAsIndex;
        }

        if (b.IsIndex) return false;
        return a.ReferenceAsName == b.ReferenceAsName;
    }

    public static bool operator !=(JSONArrayIndexOrName a, JSONArrayIndexOrName b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is JSONArrayIndexOrName asThisType)
            return this == asThisType;
        return false;
    }

    public override readonly int GetHashCode()
    {
        return IsIndex ? ReferenceAsIndex : (ReferenceAsName?.GetHashCode() ?? 0);
    }

    public static implicit operator JSONArrayIndexOrName(string? id)
    {
        if (id == null) return default;
        return new JSONArrayIndexOrName(id);
    }

    public static implicit operator JSONArrayIndexOrName(int? idx)
    {
        if (idx == null) return default;
        return new JSONArrayIndexOrName(idx.Value);
    }
}

public class JSONIndexOrNameHandler : ReflectorTypeHandlerBase<JSONArrayIndexOrName>
{
    public override string TypeName => nameof(JSONArrayIndexOrName);

    public override Type Type => typeof(JSONArrayIndexOrName);

    public override JSONArrayIndexOrName ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.Number)
        {
            if (!reader.Read())
                return default;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            ReflectorTypeHandlerBase<string>? handler = ReflectorEngine.GetTypeHandler<string>();
            return handler?.ParseFromJSON(ref reader);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            ReflectorTypeHandlerBase<int>? handler = ReflectorEngine.GetTypeHandler<int>();
            return handler?.ParseFromJSON(ref reader);
        }

        return default;
    }
}

/// <summary>
/// Used for a specific case in GLTFTexture where PBR components can be
/// a texture reference (via index/name) or an array of floats representing the color
/// </summary>
public struct JSONArrayIndexOrNameOrArrayOfFloats
{
    public JSONArrayIndexOrName ReferenceAsNameOrArray;
    public float[]? ReferenceAsArrayOfFloats;
    public bool Valid;
    public bool IsArray;

    public JSONArrayIndexOrNameOrArrayOfFloats(JSONArrayIndexOrName indexOrName)
    {
        ReferenceAsNameOrArray = indexOrName;
        IsArray = false;
        Valid = true;
    }

    public JSONArrayIndexOrNameOrArrayOfFloats(float[] array)
    {
        ReferenceAsArrayOfFloats = array;
        IsArray = true;
        Valid = true;
    }

    public static implicit operator JSONArrayIndexOrNameOrArrayOfFloats(string? id)
    {
        if (id == null) return default;
        return new JSONArrayIndexOrName(id);
    }

    public static implicit operator JSONArrayIndexOrNameOrArrayOfFloats(JSONArrayIndexOrName? indexOrName)
    {
        if (indexOrName == null) return default;
        return new JSONArrayIndexOrNameOrArrayOfFloats(indexOrName.Value);
    }

    public static implicit operator JSONArrayIndexOrNameOrArrayOfFloats(float[]? array)
    {
        if (array == null) return default;
        return new JSONArrayIndexOrNameOrArrayOfFloats(array);
    }
}

public class JSONArrayIndexOrNameOrArrayOfFloatsHandler : ReflectorTypeHandlerBase<JSONArrayIndexOrNameOrArrayOfFloats>
{
    public override string TypeName => nameof(JSONArrayIndexOrNameOrArrayOfFloats);

    public override Type Type => typeof(JSONArrayIndexOrNameOrArrayOfFloats);

    public override JSONArrayIndexOrNameOrArrayOfFloats ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.Number && reader.TokenType != JsonTokenType.StartArray) 
        {
            if (!reader.Read())
                return default;
        }

        if (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.Number)
        {
            ReflectorTypeHandlerBase<JSONArrayIndexOrName>? handler = ReflectorEngine.GetTypeHandler<JSONArrayIndexOrName>();
            return handler?.ParseFromJSON(ref reader);
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            ReflectorTypeHandlerBase<float[]>? handler = ReflectorEngine.GetTypeHandler<float[]>();
            return handler?.ParseFromJSON(ref reader);
        }

        return default;
    }
}