#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFImage
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}