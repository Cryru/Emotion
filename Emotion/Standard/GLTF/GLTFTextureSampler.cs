#nullable enable

using Emotion.Serialization.JSON;
using OpenGL;

namespace Emotion.Standard.GLTF;

public class GLTFTextureSampler : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public TextureWrapMode WrapS { get; set; }

    public TextureWrapMode WrapT { get; set; }

    public TextureMagFilter MagFilter { get; set; }

    public TextureMagFilter MinFilter { get; set; }
}
