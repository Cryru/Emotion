// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Engine;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;
using Matrix4 = OpenTK.Matrix4;
using Vector4 = OpenTK.Vector4;

#endregion

namespace Emotion.GLES
{
    /// <summary>
    /// A Program Object represents fully processed executable code, in the OpenGL Shading Language, for one or more Shader
    /// stages.
    /// </summary>
    public class GlProgram
    {
        public int Pointer;

        /// <summary>
        /// Attaches uniform variables.
        /// </summary>
        private Dictionary<string, int> _uniformVariables;

        /// <summary>
        /// Create a new empty GL Program.
        /// </summary>
        public GlProgram()
        {
            Pointer = GL.CreateProgram();
        }

        /// <summary>
        /// Links the program.
        /// </summary>
        public void Link()
        {
            GL.LinkProgram(Pointer);

            // Check link status.
            string programStatus = GL.GetProgramInfoLog(Pointer);
            if (programStatus != "") throw new Exception("Failed to link program " + Pointer + " : " + programStatus);

            // Declare uniform variables holder.
            _uniformVariables = new Dictionary<string, int>();
        }

        public void Use()
        {
            ThreadManager.ExecuteGLThread(() => { GL.UseProgram(Pointer); });
        }

        public void Destroy()
        {
            GL.DeleteProgram(Pointer);
        }

        #region Shaders

        /// <summary>
        /// Attaches a shader to the program.
        /// </summary>
        /// <param name="shader">The shader object to attach.</param>
        public void AttachShader(Shader shader)
        {
            GL.AttachShader(Pointer, shader.Pointer);
        }

        /// <summary>
        /// Detaches a shader from the program.
        /// </summary>
        /// <param name="shader">The shader object to detach.</param>
        public void DetachShader(Shader shader)
        {
            GL.DetachShader(Pointer, shader.Pointer);
        }

        #endregion

        #region Uniforms

        /// <summary>
        /// Adds a uniform variable to be used in the shader code.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        private void AddUniformVariable(string name)
        {
            if (_uniformVariables.ContainsKey(name)) return;

            _uniformVariables.Add(name, GL.GetUniformLocation(Pointer, name));
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public void SetUniformMatrix4(string name, Matrix4 matrix4)
        {
            ThreadManager.ExecuteGLThread(() =>
            {
                if (!_uniformVariables.ContainsKey(name)) AddUniformVariable(name);

                int id = _uniformVariables[name];
                GL.UniformMatrix4(id, false, ref matrix4);
            });
        }

        /// <summary>
        /// Adds data to the uniform of the specified name.
        /// </summary>
        /// <param name="name">The variable name of the uniform.</param>
        /// <param name="data">The data to add to the location.</param>
        public void SetUniformInt(string name, int data)
        {
            ThreadManager.ExecuteGLThread(() =>
            {
                if (!_uniformVariables.ContainsKey(name)) AddUniformVariable(name);

                int id = _uniformVariables[name];
                GL.Uniform1(id, data);
            });
        }

        /// <summary>
        /// Adds data to the uniform of the specified name.
        /// </summary>
        /// <param name="name">The variable name of the uniform.</param>
        /// <param name="data">The data to add to the location.</param>
        public void SetUniformFloat(string name, float data)
        {
            ThreadManager.ExecuteGLThread(() =>
            {
                if (!_uniformVariables.ContainsKey(name)) AddUniformVariable(name);

                int id = _uniformVariables[name];
                GL.Uniform1(id, data);
            });
        }


        /// <summary>
        /// Add an Emotion color as a uniform.
        /// </summary>
        /// <param name="name">The variable name of the uniform.</param>
        /// <param name="data">The color to add.</param>
        public void SetUniformColor(string name, Color data)
        {
            ThreadManager.ExecuteGLThread(() =>
            {
                if (!_uniformVariables.ContainsKey(name)) AddUniformVariable(name);

                int id = _uniformVariables[name];
                GL.Uniform4(id, new Vector4(data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f));
            });
        }

        #endregion
    }
}