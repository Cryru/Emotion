#region Using

using System.IO;
using System.Xml.Serialization;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Shading;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    public class ShaderDescription
    {
        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Frag { get; set; }

        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Vert { get; set; }

        /// <summary>
        /// The path to another shader description to fallback to if the creation of this shader fails.
        /// </summary>
        public string Fallback { get; set; } = "Shaders/DefaultShader.xml";
    }

    public class ShaderAsset : Asset
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
        public ShaderProgram Shader { get; protected set; }

        /// <summary>
        /// The serializer used to deserialize shader files into ShaderDescription files.
        /// </summary>
        public static XmlSerializer Serializer = new XmlSerializer(typeof(ShaderDescription));

        protected override void CreateInternal(byte[] data)
        {
            // Deserialize the shader description.
            using (var stream = new MemoryStream(data))
            {
                Description = (ShaderDescription) Serializer.Deserialize(stream);
            }

            // Get the text contents of the shader files referenced. If any of them are missing substitute with the default one.
            TextAsset vertShader = null;
            var ownVert = false;

            if (!string.IsNullOrEmpty(Description.Vert))
            {
                vertShader = Engine.AssetLoader.Get<TextAsset>(Description.Vert);
                ownVert = true;
            }
            
            if (vertShader == null)
            {
                vertShader = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultVert.vert");
                ownVert = false;
            }

            TextAsset fragShader = null;
            var ownFrag = false;

            if (!string.IsNullOrEmpty(Description.Frag))
            {
                fragShader = Engine.AssetLoader.Get<TextAsset>(Description.Frag);
                ownFrag = true;
            }

            if (fragShader == null)
            {
                fragShader = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultFrag.frag");
                ownFrag = false;
            }

            Engine.Log.Info($"Creating shader asset - v:{vertShader.Name}, f:{fragShader.Name}", MessageSource.AssetLoader);

            // Create the shader, or at least try to.
            GLThread.ExecuteGLThread(() =>
            {
                Shader = ShaderFactory.CreateShader(vertShader.Content, fragShader.Content);
            });
            
            // Free text assets as they are no longer needed.
            if (ownVert) Engine.AssetLoader.Destroy(vertShader.Name);
            if (ownFrag) Engine.AssetLoader.Destroy(fragShader.Name);

            // Check if compilation was successful.
            if (Shader != null && Shader.Valid) return;

            Engine.Log.Warning($"Shader {Name} creation failed. Falling back.", MessageSource.AssetLoader);
            IsFallback = true;

            // If there is no fallback, fallback to default.
            if (string.IsNullOrEmpty(Description.Fallback))
            {
                Engine.Log.Warning("No fallback specified, falling back to default.", MessageSource.AssetLoader);
                Shader = ShaderFactory.DefaultProgram;
                FallbackName = "Default";
                return;
            }

            var fallBackShader = Engine.AssetLoader.Get<ShaderAsset>(Description.Fallback);
            // If not found, fallback to default.
            if (fallBackShader == null)
            {
                Engine.Log.Warning($"Fallback {Description.Fallback} not found. Falling back to default.", MessageSource.AssetLoader);
                Shader = ShaderFactory.DefaultProgram;
                FallbackName = "Default";
                return;
            }

            Engine.Log.Warning($"Shader {Name} fell back to {Description.Fallback}.", MessageSource.AssetLoader);
            FallbackName = Description.Fallback;
            Shader = fallBackShader.Shader;
        }

        protected override void DisposeInternal()
        {
            if (!IsFallback)
            {
                Shader.Dispose();
            }
        }
    }
}