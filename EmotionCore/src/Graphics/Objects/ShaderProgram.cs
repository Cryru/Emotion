// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Linq;
using System.Numerics;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// A Program Object represents fully processed executable code, in the OpenGL Shading Language, for one or more Shader
    /// stages.
    /// </summary>
    public class ShaderProgram : IGLObject
    {
        #region Global

        /// <summary>
        /// The current shader program.
        /// </summary>
        public static ShaderProgram Current;

        /// <summary>
        /// The default shader program.
        /// </summary>
        public static ShaderProgram Default;

        /// <summary>
        /// The location of vertices within the shader.
        /// </summary>
        public static readonly int VertexLocation = 0;

        /// <summary>
        /// The location of the texture UV within the shader.
        /// </summary>
        public static readonly int UvLocation = 1;

        /// <summary>
        /// The location of the texture id within the shader.
        /// </summary>
        public static readonly int TidLocation = 2;

        /// <summary>
        /// The location of the colors within the shader.
        /// </summary>
        public static readonly int ColorLocation = 3;

        #endregion

        #region Defaults

        /// <summary>
        /// The default vertex shader.
        /// </summary>
        public static Shader DefaultVertShader;

        /// <summary>
        /// The default fragment shader.
        /// </summary>
        public static Shader DefaultFragShader;

        #endregion

        /// <summary>
        /// The program's unique id.
        /// </summary>
        internal int Pointer;

        #region Initialization

        /// <summary>
        /// Create a shader program.
        /// </summary>
        /// <param name="fragmentShader">The program's fragment shader or null to use default.</param>
        /// <param name="vertexShader">The program's vertex shader or null to use default.</param>
        internal ShaderProgram(Shader vertexShader, Shader fragmentShader)
        {
            GLThread.ExecuteGLThread(() =>
            {
                // Set the first ever as default.
                if (Default == null) Default = this;

                // Check if vert provided.
                Shader vert = vertexShader ?? DefaultVertShader;
                Shader frag = fragmentShader ?? DefaultFragShader;

                Init(vert, frag);
            });
        }

        /// <summary>
        /// Create a shader program from shader strings.
        /// </summary>
        /// <param name="fragmentShader">The program's fragment shader or null to use default.</param>
        /// <param name="vertexShader">The program's vertex shader or null to use default.</param>
        public ShaderProgram(string vertexShader, string fragmentShader)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Shader vert = string.IsNullOrEmpty(vertexShader) ? DefaultVertShader : new Shader(ShaderType.VertexShader, vertexShader);
                Shader frag = string.IsNullOrEmpty(fragmentShader) ? DefaultFragShader : new Shader(ShaderType.FragmentShader, fragmentShader);

                Init(vert, frag);
            });
        }

        private void Init(Shader vert, Shader frag)
        {
            // Save current.
            ShaderProgram current = Current;

            // Create the program and attach shaders.
            Pointer = GL.CreateProgram();
            GL.AttachShader(Pointer, vert.Pointer);
            GL.AttachShader(Pointer, frag.Pointer);
            Link();
            Bind();
            GL.DetachShader(Pointer, vert.Pointer);
            GL.DetachShader(Pointer, frag.Pointer);
            GL.ValidateProgram(Pointer);

            GLThread.CheckError("making program");

            // Set default uniforms.
            SetUniformIntArray("textures", Enumerable.Range(0, 15).ToArray());

            // Restore current.
            current?.Bind();
        }

        #endregion

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
        public void Bind()
        {
            GL.UseProgram(Pointer);
            Current = this;
        }

        /// <summary>
        /// Stop using this shader program, and use the default.
        /// </summary>
        public void Unbind()
        {
            Default.Bind();
        }

        /// <summary>
        /// Delete this program freeing memory.
        /// </summary>
        public void Delete()
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

        /// <summary>
        /// Checks whether the current shader and the one provided are the same.
        /// </summary>
        /// <param name="obj">The shader to compare with.</param>
        /// <returns>Whether the two shader programs are the same.</returns>
        public bool Equals(ShaderProgram obj)
        {
            return Pointer == obj.Pointer;
        }

        #region Uniform Upload

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public void SetUniformMatrix4(string name, Matrix4x4 matrix4)
        {
            SetUniformMatrix4(GetUniformLocation(name), matrix4);
            GLThread.CheckError("setting mat4 uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public void SetUniformInt(string name, int data)
        {
            SetUniformInt(GetUniformLocation(name), data);
            GLThread.CheckError("setting int uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public void SetUniformFloat(string name, float data)
        {
            SetUniformFloat(GetUniformLocation(name), data);
            GLThread.CheckError("setting float uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public void SetUniformColor(string name, Color data)
        {
            SetUniformColor(GetUniformLocation(name), data);
            GLThread.CheckError("setting color uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int array value to set it to.</param>
        public void SetUniformIntArray(string name, int[] data)
        {
            SetUniformIntArray(GetUniformLocation(name), data);
            GLThread.CheckError("setting int array uniform");
        }

        #endregion

        #region Uniform Upload Global

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public static void SetUniformMatrix4(int id, Matrix4x4 matrix4)
        {
            unsafe
            {
                float* matrixPtr = &matrix4.M11;
                GL.UniformMatrix4(id, 1, false, matrixPtr);
            }
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public static void SetUniformInt(int id, int data)
        {
            GL.Uniform1(id, data);
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public static void SetUniformFloat(int id, float data)
        {
            GL.Uniform1(id, data);
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public static void SetUniformColor(int id, Color data)
        {
            GL.Uniform4(id, data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f);
        }

        /// <summary>
        /// Sets the uniform of the specified id to the provided value.
        /// </summary>
        /// <param name="id">The id of the uniform to upload to.</param>
        /// <param name="data">The int array value to set it to.</param>
        public static void SetUniformIntArray(int id, int[] data)
        {
            GL.Uniform1(id, data.Length, data);
        }

        #endregion
    }
}