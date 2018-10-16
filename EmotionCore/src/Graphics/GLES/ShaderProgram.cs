// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Linq;
using Emotion.Debug;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
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
        private int _pointer;

        #region Initialization

        /// <summary>
        /// Create a shader program.
        /// </summary>
        /// <param name="fragmentShader">The program's fragment shader.</param>
        /// <param name="vertexShader">The program's vertex shader.</param>
        public ShaderProgram(Shader vertexShader, Shader fragmentShader)
        {
            ThreadManager.ExecuteGLThread(() =>
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
        /// <param name="fragmentShader">The program's fragment shader.</param>
        /// <param name="vertexShader">The program's vertex shader.</param>
        public ShaderProgram(string vertexShader, string fragmentShader)
        {
            ThreadManager.ExecuteGLThread(() =>
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
            _pointer = GL.CreateProgram();
            GL.AttachShader(_pointer, vert.Pointer);
            GL.AttachShader(_pointer, frag.Pointer);
            Link();
            Bind();
            GL.DetachShader(_pointer, vert.Pointer);
            GL.DetachShader(_pointer, frag.Pointer);
            GL.ValidateProgram(_pointer);

            Helpers.CheckError("making program");

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
            GL.LinkProgram(_pointer);

            // Check link status.
            string programStatus = GL.GetProgramInfoLog(_pointer);
            if (programStatus != "") throw new Exception("Failed to link program " + _pointer + " : " + programStatus);
        }

        /// <summary>
        /// Set this program as the current program.
        /// </summary>
        public void Bind()
        {
            GL.UseProgram(_pointer);
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
            GL.DeleteProgram(_pointer);
        }

        /// <summary>
        /// Returns the id of a uniform with a specified name within the shader program.
        /// </summary>
        /// <param name="name">The name of the uniform whose id to return.</param>
        /// <returns>The id of the uniform the name belongs to.</returns>
        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(_pointer, name);
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
            Helpers.CheckError("setting mat4 uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public void SetUniformInt(string name, int data)
        {
            SetUniformInt(GetUniformLocation(name), data);
            Helpers.CheckError("setting int uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public void SetUniformFloat(string name, float data)
        {
            SetUniformFloat(GetUniformLocation(name), data);
            Helpers.CheckError("setting float uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public void SetUniformColor(string name, Color data)
        {
            SetUniformColor(GetUniformLocation(name), data);
            Helpers.CheckError("setting color uniform");
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int array value to set it to.</param>
        public void SetUniformIntArray(string name, int[] data)
        {
            SetUniformIntArray(GetUniformLocation(name), data);
            Helpers.CheckError("setting int array uniform");
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