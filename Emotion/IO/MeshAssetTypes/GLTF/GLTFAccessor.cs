#region Using

using System.Text.Json.Serialization;
using OpenGL;
using System.Runtime.InteropServices;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFAccessor
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("bufferView")]
    public int BufferView { get; set; }

    [JsonPropertyName("componentType")]
    public int ComponentType { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("normalized")]
    public bool Normalized { get; set; }

    [JsonPropertyName("byteOffset")]
    public int ByteOffset { get; set; }

    public int GetComponentSize()
    {
        switch (ComponentType)
        {
            case Gl.FLOAT:
                return 4;
            case Gl.UNSIGNED_BYTE:
                return 1;
            case Gl.UNSIGNED_SHORT:
                return 2;
        }

        return 0;
    }

    public int GetComponentCount()
    {
        switch (Type)
        {
            case "VEC4":
                return 4;
            case "VEC3":
                return 3;
            case "VEC2":
                return 2;
            case "SCALAR":
                return 1;
            case "MAT4":
                return 4 * 4;
        }

        return 0;
    }

    public int GetDataSize()
    {
        return GetComponentCount() * GetComponentSize();
    }

    public Func<ReadOnlyMemory<byte>, bool, float> GetComponentReader()
    {
        switch (ComponentType)
        {
            case Gl.FLOAT:
                return GLTFAccessorReadComponent<float>;
            case Gl.UNSIGNED_BYTE:
                return GLTFAccessorReadComponent<byte>;
            case Gl.UNSIGNED_SHORT:
                return GLTFAccessorReadComponent<ushort>;
        }

        return null!;
    }


    public Vector4 GetDataAsVec4Float(ReadOnlyMemory<byte> readFrom)
    {
        Vector4 returnVal = new Vector4(0f);
        int returnValWrite = 0;

        var readerFunc = GetComponentReader();

        int components = GetComponentCount();
        int componentSize = GetComponentSize();
        for (int i = 0; i < components; i++)
        {
            ReadOnlyMemory<byte> component = readFrom.Slice(i * componentSize, componentSize);
            float output = readerFunc(component, Normalized);

            returnVal[returnValWrite] = output;
            returnValWrite++;
        }

        return returnVal;
    }

    private static unsafe float GLTFAccessorReadComponent<TActualType>(ReadOnlyMemory<byte> data, bool normalize)
        where TActualType : unmanaged, INumber<TActualType>, IMinMaxValue<TActualType>
    {
        var span = data.Span;
        var spanActualType = MemoryMarshal.Cast<byte, TActualType>(span);
        TActualType actualTypeValue = spanActualType[0];

        float valAsFloat = (float)Convert.ChangeType(actualTypeValue, typeof(float));

        // Cast the number to a float and divide it by its max value to normalize it.
        if (normalize)
        {
            float valMax = (float)Convert.ChangeType(TActualType.MaxValue, typeof(float));
            valAsFloat = valAsFloat / valMax;
        }

        return valAsFloat;
    }
}