// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using System.Xml.Serialization;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.Base;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// An asset which describes a shader.
    /// </summary>
    public sealed class ShaderAsset : Asset
    {
        /// <summary>
        /// The description of the shader.
        /// </summary>
        public ShaderDescription Description { get; private set; }

        /// <summary>
        /// Whether using the fallback shader.
        /// </summary>
        public bool IsFallback { get; set; }

        /// <summary>
        /// The actual shader compiled from this asset.
        /// </summary>
        public ShaderProgram Shader;

        /// <summary>
        /// The serializer used to deserialize shader files into ShaderDescription files.
        /// </summary>
        public static XmlSerializer Serializer = new XmlSerializer(typeof(ShaderDescription));

        internal override void CreateAsset(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                Description = (ShaderDescription) Serializer.Deserialize(stream);
            }

            TextFile vertShader = !string.IsNullOrEmpty(Description.Frag) ? Context.AssetLoader.Get<TextFile>(Description.Vert) : null;
            TextFile fragShader = !string.IsNullOrEmpty(Description.Frag) ? Context.AssetLoader.Get<TextFile>(Description.Frag) : null;

            Shader = GraphicsManager.CreateShaderProgram(vertShader?.Content, fragShader?.Content);
            if (Shader == null)
            {
                Context.Log.Warning($"Shader {Name} creation failed.", MessageSource.AssetLoader);

                // Check if a fallback is specified.
                if (!string.IsNullOrEmpty(Description.Fallback))
                {
                    Context.Log.Warning($"Shader {Name} will use fallback - {Description.Fallback}.", MessageSource.AssetLoader);
                    IsFallback = true;
                    ShaderAsset fallbackShader = Context.AssetLoader.Get<ShaderAsset>($"{Description.Fallback}");
                    Shader = fallbackShader.Shader;
                }
                else
                {
                    Context.Log.Error($"No fallback shader found for {Name}.", MessageSource.AssetLoader);
                }
            }

            // Shader text files are no longer needed.
            Context.AssetLoader.Destroy(Description.Vert);
            Context.AssetLoader.Destroy(Description.Frag);
        }

        internal override void DestroyAsset()
        {
            if (!IsFallback) Shader.Delete();
        }
    }
}