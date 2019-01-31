// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Base
{
    /// <summary>
    /// A GL shader program.
    /// </summary>
    public class GLShaderProgram : ShaderProgram
    {
        /// <summary>
        /// A cache of uniform locations.
        /// </summary>
        private Dictionary<string, uint> _uniformLocationsMap;

        #region Initialization

        internal GLShaderProgram(uint vertPointer, uint fragPointer)
        {
            _uniformLocationsMap = new Dictionary<string, uint>();

            // Create and link the program with the vert and fragment shaders.
            Id = (uint) GL.CreateProgram();
            GL.AttachShader((int) Id, (int) vertPointer);
            GL.AttachShader((int) Id, (int) fragPointer);
            GL.LinkProgram(Id);

            // Check linking status.
            string programStatus = GL.GetProgramInfoLog((int) Id);
            if (programStatus != "") Context.Log.Warning($"Failed to link shader program {Id} with shaders. Error is {programStatus}", MessageSource.GL);

            GraphicsManager.BindShaderProgram(this);
            GL.DetachShader((int) Id, (int) vertPointer);
            GL.DetachShader((int) Id, (int) fragPointer);
            GL.ValidateProgram(Id);

            GLThread.CheckError("making program and shaders");
        }

        #endregion

        /// <inheritdoc />
        public override void Delete()
        {
            GL.DeleteProgram(Id);
        }

        /// <inheritdoc />
        public override uint GetUniformLocation(string name)
        {
            // Check if uniform location is present in cache.
            if (_uniformLocationsMap.ContainsKey(name)) return _uniformLocationsMap[name];

            // If not, request it from OpenGL and cache it.
            uint location = (uint) GL.GetUniformLocation(Id, name);
            _uniformLocationsMap.Add(name, location);

            return location;
        }

        #region Uniform Upload

        /// <inheritdoc />
        public override void SetUniformMatrix4(string name, Matrix4x4 matrix4)
        {
            uint id = GetUniformLocation(name);
            unsafe
            {
                float* matrixPtr = &matrix4.M11;
                GL.UniformMatrix4((int) id, 1, false, matrixPtr);
            }

            GLThread.CheckError("setting mat4 uniform");
        }

        /// <inheritdoc />
        public override void SetUniformInt(string name, int data)
        {
            uint id = GetUniformLocation(name);
            GL.Uniform1((int) id, data);
            GLThread.CheckError("setting int uniform");
        }

        /// <inheritdoc />
        public override void SetUniformFloat(string name, float data)
        {
            uint id = GetUniformLocation(name);
            GL.Uniform1((int) id, data);
            GLThread.CheckError("setting float uniform");
        }

        /// <inheritdoc />
        public override void SetUniformColor(string name, Color data)
        {
            uint id = GetUniformLocation(name);
            GL.Uniform4((int) id, data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f);
            GLThread.CheckError("setting color uniform");
        }

        /// <inheritdoc />
        public override void SetUniformIntArray(string name, int[] data)
        {
            uint id = GetUniformLocation(name);
            GL.Uniform1((int) id, data.Length, data);
            GLThread.CheckError("setting int array uniform");
        }

        #endregion
    }
}