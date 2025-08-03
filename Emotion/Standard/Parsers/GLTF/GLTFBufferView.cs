#nullable enable




#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFBufferView : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public JSONArrayIndexOrName Buffer { get; set; }

    public int ByteLength { get; set; }

    public int ByteOffset { get; set; }

    public int ByteStride { get; set; }

    public int Target { get; set; }
}
