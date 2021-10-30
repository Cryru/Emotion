#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics.Shading
{
    [DontSerialize]
    public class ShaderProgram
    {
        /// <summary>
        /// The currently bound shader program.
        /// </summary>
        public static uint Bound;

        /// <summary>
        /// The GL pointer of this shader program.
        /// </summary>
        public uint Pointer { get; protected set; }

        /// <summary>
        /// Whether the program is valid.
        /// </summary>
        public bool Valid { get; protected set; }

        #region Debug

        /// <summary>
        /// The configuration the shader program was built using.
        /// For debugging purposes.
        /// </summary>
        public string CompiledConfig;

        /// <summary>
        /// The source of the vertex shader the program was compiled with.
        /// Visible only when debug mode is enabled.
        /// </summary>
        public string DebugVertSource;

        /// <summary>
        /// The source of the fragment shader the program was compiled with.
        /// Visible only when debug mode is enabled.
        /// </summary>
        public string DebugFragSource;

        #endregion

        #region Default Shader Parameter Constants

        /// <summary>
        /// The location of vertices within the shader.
        /// </summary>
        public readonly uint VertexLocation = 0;

        /// <summary>
        /// The location of the texture UV within the shader.
        /// </summary>
        public readonly uint UvLocation = 1;

        /// <summary>
        /// The location of the colors within the shader.
        /// </summary>
        public readonly uint ColorLocation = 2;

        #endregion

        /// <summary>
        /// A cache of uniform locations.
        /// </summary>
        private Dictionary<string, int> _uniformLocationsMap = new Dictionary<string, int>();

        public ShaderProgram(uint vertShader, uint fragShader)
        {
            Pointer = Gl.CreateProgram();
            Gl.AttachShader(Pointer, vertShader);
            Gl.AttachShader(Pointer, fragShader);

            // Set default parameter locations.
            Gl.BindAttribLocation(Pointer, VertexLocation, "vertPos");
            Gl.BindAttribLocation(Pointer, UvLocation, "uv");
            Gl.BindAttribLocation(Pointer, ColorLocation, "color");

            Gl.LinkProgram(Pointer);

            // Check linking status.
            var programCompileStatusReader = new StringBuilder(1024);
            Gl.GetProgramInfoLog(Pointer, 1024, out int length, programCompileStatusReader);
            if (length > 0)
            {
                var programStatus = programCompileStatusReader.ToString(0, length);
                if (programStatus != "") Engine.Log.Warning($"Log for linking shader {Pointer} is {programStatus}", MessageSource.GL);
            }

            Gl.GetProgram(Pointer, ProgramProperty.LinkStatus, out int state);
            if (state == 0)
                Engine.Log.Warning($"Couldn't link shader program {Pointer}.", MessageSource.GL);
            else
                Valid = true;

            if (!Valid) return;

            // Set default uniforms - this requires binding, so save the currently bound.
            uint previouslyBound = Bound;
            EnsureBound(Pointer);
            SetUniformInt("mainTexture", 0);
            SetUniformFloat("iTime", 0);
            EnsureBound(previouslyBound);
        }

        #region Uniform Manipulation

        /// <summary>
        /// Get the location of the uniform of the specified name in the shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <returns>The location of the uniform.</returns>
        public int GetUniformLocation(string name)
        {
            if (Bound != Pointer)
            {
                Engine.Log.Warning($"Tried to upload uniform {name} to a shader that is not current.", MessageSource.GL, true);
                return -1;
            }

            // Check if uniform location is present in cache.
            if (_uniformLocationsMap.ContainsKey(name)) return _uniformLocationsMap[name];

            // If not, request it from OpenGL and cache it.
            int location = Gl.GetUniformLocation(Pointer, name);
            _uniformLocationsMap.Add(name, location);

            return location;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="matrix4">The matrix value to set it to.</param>
        public bool SetUniformMatrix4(string name, Matrix4x4 matrix4)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            unsafe
            {
                float* matrixPtr = &matrix4.M11;
                Gl.UniformMatrix4(id, 1, false, matrixPtr);
            }

            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int value to set it to.</param>
        public bool SetUniformInt(string name, int data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform1(id, data);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The float value to set it to.</param>
        public bool SetUniformFloat(string name, float data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform1(id, data);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// The uniform is expected to be a vector4 in the shader.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The color value to set it to.</param>
        public bool SetUniformColor(string name, Color data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform4(id, data.R / 255f, data.G / 255f, data.B / 255f, data.A / 255f);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The int array value to set it to.</param>
        public bool SetUniformIntArray(string name, int[] data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform1(id, data);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector 2 to set it to.</param>
        public bool SetUniformVector2(string name, Vector2 data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform2f(id, 1, data);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector 3 to set it to.</param>
        public bool SetUniformVector3(string name, Vector3 data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform3f(id, 1, data);
            return true;
        }

        /// <summary>
        /// Sets the uniform of the specified name to the provided value.
        /// </summary>
        /// <param name="name">The name of the uniform to upload to.</param>
        /// <param name="data">The vector4 to set it to.</param>
        public bool SetUniformVector4(string name, Vector4 data)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            Gl.Uniform4f(id, 1, data);
            return true;
        }

        #endregion

        /// <summary>
        /// Destroy the shader, cleaning up used resources.
        /// </summary>
        public void Dispose()
        {
            uint ptr = Pointer;
            Pointer = 0;
            Valid = false;

            if (Engine.Host == null) return;
            if (Bound == ptr) Bound = 0;

            GLThread.ExecuteGLThreadAsync(() => { Gl.DeleteProgram(ptr); });
        }

        /// <summary>
        /// Ensures the provided program is the currently bound shader program.
        /// </summary>
        /// <param name="pointer">A pointer to the program object to ensure is bound.</param>
        public static void EnsureBound(uint pointer)
        {
            // Check if it is already bound.
            if (Bound == pointer)
            {
                // If in debug mode, verify this with OpenGL.
                if (!Engine.Configuration.GlDebugMode) return;

                Gl.GetInteger(GetPName.CurrentProgram, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed bound shader was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            Gl.UseProgram(pointer);
            Bound = pointer;
        }
    }
}