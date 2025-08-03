#nullable enable

using OpenGL;
using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFTextureSampler : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public TextureWrapMode WrapS { get; set; }

    public TextureWrapMode WrapT { get; set; }

    public TextureMagFilter MagFilter { get; set; }

    public TextureMagFilter MinFilter { get; set; }
}
