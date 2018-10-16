// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Soul;

#endregion

namespace Emotion.Graphics.GLES
{
    /// <summary>
    /// A Shader is a user-defined program designed to run on some stage of a graphics processor.
    /// </summary>
    public sealed class Shader
    {
        #region Properties

        /// <summary>
        /// The type of shader this is.
        /// </summary>
        public ShaderType Type;

        /// <summary>
        /// The shader's internal id.
        /// </summary>
        public int Pointer { get; private set; }

        #endregion

        /// <summary>
        /// Create, add source, and compile a new shader.
        /// </summary>
        /// <param name="type">The type of shader to create.</param>
        /// <param name="source">The shader string source.</param>
        public Shader(ShaderType type, string source)
        {
            Type = type;

            // Fix for MacOS.
            if (CurrentPlatform.OS == PlatformName.Mac)
            {
                source = source.Replace("#version 300 es", "#version 330");
                Debugger.Log(MessageType.Warning, MessageSource.GL, "Shader version changed from '300 es' to '330' because Mac platform was detected.");
            }

            // Fix for missing GL_ARB_gpu_shader5.
            if ((CurrentPlatform.OS == PlatformName.Windows || CurrentPlatform.OS == PlatformName.Linux) && Renderer.Shader5ExtensionMissing)
            {
                source = source.Replace("#version 300 es", "#version 400");
                Debugger.Log(MessageType.Warning, MessageSource.GL, "Shader version changed from '300 es` to 400' because the Shader5 extension is missing.");
            }

            // Create and compile the shader.
            Pointer = GL.CreateShader(type);
            GL.ShaderSource(Pointer, source);
            GL.CompileShader(Pointer);

            // Check compilation status.
            string compileStatus = GL.GetShaderInfoLog(Pointer);
            if (compileStatus != "") throw new Exception("Failed to compile shader " + Pointer + " : " + compileStatus);
        }

        /// <summary>
        /// Deletes the shader.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteShader(Pointer);
        }
    }
}