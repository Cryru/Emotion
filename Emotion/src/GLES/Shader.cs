// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.GLES
{
    public class Shader
    {
        /// <summary>
        /// The type of shader this is.
        /// </summary>
        public ShaderType Type;

        /// <summary>
        /// The shader's internal id.
        /// </summary>
        public int Pointer;

        /// <summary>
        /// Create, add source, and compile a new shader.
        /// </summary>
        /// <param name="type">The type of shader to create.</param>
        /// <param name="source">The shader string source.</param>
        public Shader(ShaderType type, string source)
        {
            Type = type;

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