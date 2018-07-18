// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
{
    /// <summary>
    /// A Program Object represents fully processed executable code, in the OpenGL Shading Language, for one or more Shader
    /// stages.
    /// </summary>
    public class ShaderProgram
    {
        #region Global

        /// <summary>
        /// The current shader program.
        /// </summary>
        public static ShaderProgram Current;

        /// <summary>
        /// The location of vertices within the shader.
        /// </summary>
        public static readonly int VertexLocation = 0;

        /// <summary>
        /// The location of the colors within the shader.
        /// </summary>
        public static readonly int ColorLocation = 1;

        #endregion

        #region Properties

        /// <summary>
        /// The program's unique id.
        /// </summary>
        public int Pointer { get; private set; }

        #endregion

        /// <summary>
        /// Create a shader program.
        /// </summary>
        /// <param name="fragmentShader">The program's fragment shader.</param>
        /// <param name="vertexShader">The program's vertex shader.</param>
        public ShaderProgram(Shader vertexShader, Shader fragmentShader)
        {
            ThreadManager.ExecuteGLThread(() =>
            {
                // Save current.
                ShaderProgram current = Current;

                // Check if vert provided.
                Shader vert = vertexShader ?? Shader.DefaultVertex;
                Shader frag = fragmentShader ?? Shader.DefaultFragment;

                // Create the program and attach shaders.
                Pointer = GL.CreateProgram();
                GL.AttachShader(Pointer, vert.Pointer);
                GL.AttachShader(Pointer, frag.Pointer);
                Link();
                Use();
                GL.DetachShader(Pointer, vert.Pointer);
                GL.DetachShader(Pointer, frag.Pointer);
                GL.ValidateProgram(Pointer);

                Helpers.CheckError("making program");

                // Set default uniforms.
                SetUniformColor("color", Color.White);

                SetUniformMatrix4("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, 960, 0, 540, -1, 1)); //todo
                SetUniformMatrix4("viewMatrix", Matrix4.Identity);
                SetUniformMatrix4("modelMatrix", Matrix4.Identity);
                SetUniformMatrix4("textureMatrix", Matrix4.Identity);

                SetUniformInt("drawTexture", 0);
                SetUniformInt("additionalTexture", 1);

                SetUniformFloat("time", 0);

                Helpers.CheckError("setting program uniforms");

                // Restore current.
                current?.Use();
            });
        }

        /// <summary>
        /// Links the program to the attached shaders.
        /// </summary>
        private void Link()
        {
            GL.LinkProgram(Pointer);

            // Check link status.
            string programStatus = GL.GetProgramInfoLog(Pointer);
            if (programStatus != "") throw new Exception("Failed to link program " + Pointer + " : " + programStatus);
        }

        /// <summary>
        /// Set this program as the current program.
        /// </summary>
        internal void Use()
        {
            GL.UseProgram(Pointer);
            Current = this;
        }

        /// <summary>
        /// Destroy this program.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteProgram(Pointer);
        }

        /// <summary>
        /// Returns the id of a uniform with a specified name within the shader program.
        /// </summary>
        /// <param name="name">The name of the uniform whose id to return.</param>
        /// <returns>The id of the uniform the name belongs to.</returns>
        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Pointer, name);
        }

        #region Uniform Upload

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public void SetUniformMatrix4(string name, Matrix4 matrix4)
        {
            SetUniformMatrix4(GetUniformLocation(name), matrix4);
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public void SetUniformInt(string name, int data)
        {
            SetUniformInt(GetUniformLocation(name), data);
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public void SetUniformFloat(string name, float data)
        {
            SetUniformFloat(GetUniformLocation(name), data);
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public void SetUniformColor(string name, Color data)
        {
            SetUniformColor(GetUniformLocation(name), data);
        }

        #endregion

        #region Uniform Upload Global

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public static void SetUniformMatrix4(int id, Matrix4 matrix4)
        {
            unsafe
            {
                float* matrixPtr = &matrix4.Row0.X;
                ThreadManager.ExecuteGLThread(() => { GL.UniformMatrix4(id, 1, false, matrixPtr); });
            }
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public static void SetUniformInt(int id, int data)
        {
            ThreadManager.ExecuteGLThread(() => { GL.Uniform1(id, data); });
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public static void SetUniformFloat(int id, float data)
        {
            ThreadManager.ExecuteGLThread(() => { GL.Uniform1(id, data); });
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public static void SetUniformColor(int id, Color data)
        {
            ThreadManager.ExecuteGLThread(() => { GL.Uniform4(id, data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f); });
        }

        #endregion
    }
}