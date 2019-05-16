#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Logging;
using Adfectus.Primitives;
using OpenGL;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    /// <summary>
    /// A GL shader program.
    /// </summary>
    public class GlShaderProgram : ShaderProgram
    {
        /// <summary>
        /// A cache of uniform locations.
        /// </summary>
        private Dictionary<string, uint> _uniformLocationsMap;

        #region Initialization

        internal GlShaderProgram(uint vertPointer, uint fragPointer)
        {
            try
            {
                _uniformLocationsMap = new Dictionary<string, uint>();

                // Create and link the program with the vert and fragment shaders.
                Id = Gl.CreateProgram();
                Gl.AttachShader(Id, vertPointer);
                Gl.AttachShader(Id, fragPointer);

                if (Engine.Flags.RenderFlags.SetVertexAttribLocations)
                {
                    Gl.BindAttribLocation(Id, Engine.GraphicsManager.VertexLocation, "vertPos");
                    Gl.BindAttribLocation(Id, Engine.GraphicsManager.UvLocation, "uv");
                    Gl.BindAttribLocation(Id, Engine.GraphicsManager.TidLocation, "tid");
                    Gl.BindAttribLocation(Id, Engine.GraphicsManager.ColorLocation, "color");

                    Engine.GraphicsManager.CheckError("setting locations");
                }

                Gl.LinkProgram(Id);

                Gl.GetProgram(Id, ProgramProperty.LinkStatus, out int state);
                if (state == 0)
                {
                    Engine.Log.Warning($"Couldn't link shader - {vertPointer} / {fragPointer}.", MessageSource.GL);
                    Broken = true;
                }

                // Check linking status.
                StringBuilder compileStatusReader = new StringBuilder(1024);
                Gl.GetProgramInfoLog(Id, 1024, out int length, compileStatusReader);
                string programStatus = compileStatusReader.ToString(0, length);
                if (programStatus != "") Engine.Log.Warning($"Log for linking shader {Id} is {programStatus}", MessageSource.GL);

                Gl.DetachShader(Id, vertPointer);
                Gl.DetachShader(Id, fragPointer);

                Gl.ValidateProgram(Id);

                Engine.GraphicsManager.CheckError("making program and shaders");
            }
            catch (Exception ex)
            {
                ErrorHandler.SubmitError(ex);
            }
        }

        #endregion

        /// <inheritdoc />
        public override void Delete()
        {
            GLThread.ExecuteGLThread(() => { Gl.DeleteProgram(Id); });
        }

        /// <inheritdoc />
        public override uint GetUniformLocation(string name)
        {
            // Check if uniform location is present in cache.
            if (_uniformLocationsMap.ContainsKey(name)) return _uniformLocationsMap[name];

            // If not, request it from OpenGL and cache it.
            uint location = (uint) Gl.GetUniformLocation(Id, name);
            _uniformLocationsMap.Add(name, location);

            return location;
        }

        #region Uniform Upload

        /// <inheritdoc />
        public override void SetUniformMatrix4(string name, Matrix4x4 matrix4)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            unsafe
            {
                float* matrixPtr = &matrix4.M11;
                Gl.UniformMatrix4((int) id, 1, false, matrixPtr);
            }

            Engine.GraphicsManager.CheckError("setting mat4 uniform");
        }

        /// <inheritdoc />
        public override void SetUniformInt(string name, int data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform1((int) id, data);
            Engine.GraphicsManager.CheckError("setting int uniform");
        }

        /// <inheritdoc />
        public override void SetUniformFloat(string name, float data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform1((int) id, data);
            Engine.GraphicsManager.CheckError("setting float uniform");
        }

        /// <inheritdoc />
        public override void SetUniformColor(string name, Color data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform4((int) id, data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f);
            Engine.GraphicsManager.CheckError("setting color uniform");
        }

        /// <inheritdoc />
        public override void SetUniformIntArray(string name, int[] data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform1((int) id, data);
            Engine.GraphicsManager.CheckError("setting int array uniform");
        }

        /// <inheritdoc />
        public override void SetUniformVector3(string name, Vector3 data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform3f((int) id, 1, data);

            Engine.GraphicsManager.CheckError("setting vec3 uniform");
        }

        /// <inheritdoc />
        public override void SetUniformVector4(string name, Vector4 data)
        {
            uint id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == uint.MaxValue) return;

            Gl.Uniform4f((int) id, 1, data);

            Engine.GraphicsManager.CheckError("setting vec4 uniform");
        }

        #endregion
    }
}