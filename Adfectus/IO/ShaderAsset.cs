#region Using

using System;
using System.IO;
using System.Xml.Serialization;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Logging;

#endregion

namespace Adfectus.IO
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
        /// The name of the fallback shader used - if any.
        /// </summary>
        public string FallbackName { get; set; }

        /// <summary>
        /// The actual shader compiled from this asset.
        /// </summary>
        public ShaderProgram Shader;

        /// <summary>
        /// The serializer used to deserialize shader files into ShaderDescription files.
        /// </summary>
        public static XmlSerializer Serializer = new XmlSerializer(typeof(ShaderDescription));

#if DEBUG

        /// <summary>
        /// The vertex shader source this shader uses. Accessible only in debug mode.
        /// </summary>
        public string VertShader { get; private set; }

        /// <summary>
        /// The fragment shader source this shader uses. Accessible only in debug mode.
        /// </summary>
        public string FragShader { get; private set; }

#endif

        protected override void CreateInternal(byte[] data)
        {
            // Deserialize the shader description.
            using (MemoryStream stream = new MemoryStream(data))
            {
                Description = (ShaderDescription) Serializer.Deserialize(stream);
            }

            // Get the text contents of the shader files referenced.
            TextFile vertShader = !string.IsNullOrEmpty(Description.Vert) ? Engine.AssetLoader.Get<TextFile>(Description.Vert) : null;
            TextFile fragShader = !string.IsNullOrEmpty(Description.Frag) ? Engine.AssetLoader.Get<TextFile>(Description.Frag) : null;

#if DEBUG
            VertShader = vertShader?.Content;
            FragShader = fragShader?.Content;
#endif

            // Create the shader. If the shader fails to be created and it isn't the final one in the chain, use a fallback.
            Shader = Engine.GraphicsManager.CreateShaderProgram(vertShader?.Content, fragShader?.Content);
            if (Shader == null && Description.Fallback != "final")
            {
                Engine.Log.Warning($"Shader {Name} creation failed, will use fallback - {Description.Fallback}.", MessageSource.AssetLoader);

                // Use the fallback. Is set to the default compatibility shader, if no fallback is specified.
                IsFallback = true;
                ShaderAsset fallbackShader = Engine.AssetLoader.Get<ShaderAsset>($"{Description.Fallback}");
                Shader = fallbackShader.Shader;
                FallbackName = fallbackShader.Name;

#if DEBUG
                VertShader = fallbackShader.VertShader;
                FragShader = fallbackShader.FragShader;
#endif
            }
            else if (Shader == null && Description.Fallback == "final")
            {
                ErrorHandler.SubmitError(new Exception($"Tried to compile shader {Name} and the fallback chain, but ended up with an error. Check logs for more info."));
            }

            // Shader text files are no longer needed.
            vertShader?.Dispose();
            fragShader?.Dispose();
        }

        protected override void DisposeInternal()
        {
            if (!IsFallback) Shader?.Delete();
        }
    }
}