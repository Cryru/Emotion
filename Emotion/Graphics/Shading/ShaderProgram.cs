#region Using

using System.Text;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
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
        /// The source of the vertex shader the program was compiled with.
        /// Visible only when debug mode is enabled.
        /// </summary>
        public string DebugVertSource;

        /// <summary>
        /// The source of the fragment shader the program was compiled with.
        /// Visible only when debug mode is enabled.
        /// </summary>
        public string DebugFragSource;

        /// <summary>
        /// Name used for identifying the shader while debugging.
        /// </summary>
        public string DebugName;

        #endregion

        #region Default Shader Parameter Constants

        /// <summary>
        /// The location of vertices within the shader.
        /// </summary>
        public static readonly uint VertexLocation = 0;

        /// <summary>
        /// The location of the texture UV within the shader.
        /// </summary>
        public static readonly uint UvLocation = 1;

        /// <summary>
        /// The location of the colors within the shader.
        /// </summary>
        public static readonly uint ColorLocation = 2;

        #endregion

        /// <summary>
        /// A cache of uniform locations.
        /// </summary>
        private Dictionary<string, int> _uniformLocationsMap = new Dictionary<string, int>();

        protected ShaderProgram()
        {
        }

        /// <summary>
        /// Create a shader program by linking compiled shaders.
        /// </summary>
        /// <param name="vertShader">The vert shader to attach.</param>
        /// <param name="fragShader">The frag shader to attach.</param>
        /// <returns></returns>
        public static ShaderProgram CreateFromShaders(uint vertShader, uint fragShader)
        {
            Assert(GLThread.IsGLThread());

            uint pointer = Gl.CreateProgram();
            Gl.AttachShader(pointer, vertShader);
            Gl.AttachShader(pointer, fragShader);

            // Set default parameter locations.
            Gl.BindAttribLocation(pointer, VertexLocation, "vertPos");
            Gl.BindAttribLocation(pointer, UvLocation, "uv");
            Gl.BindAttribLocation(pointer, ColorLocation, "color");

            Gl.LinkProgram(pointer);
            if (Engine.Configuration.GlDebugMode) Gl.ValidateProgram(pointer);

            // Check linking status.
            var programCompileStatusReader = new StringBuilder(1024);
            Gl.GetProgramInfoLog(pointer, 1024, out int length, programCompileStatusReader);
            if (length > 0)
            {
                var programStatus = programCompileStatusReader.ToString(0, length);
                if (programStatus != "") Engine.Log.Warning($"Log for linking shaders (v:{vertShader} f:{fragShader}) is {programStatus}", MessageSource.GL);
            }

            var valid = false;
            Gl.GetProgram(pointer, ProgramProperty.LinkStatus, out int state);
            if (state == 0)
                Engine.Log.Warning($"Couldn't link shader program {pointer}.", MessageSource.GL);
            else
                valid = true;

            var newProgram = new ShaderProgram
            {
                Pointer = pointer,
                Valid = valid
            };
            return newProgram;
        }

        /// <summary>
        /// Create a new instance that is a copy of another program.
        /// This is used to prevent shaders which use fallbacks from using the same
        /// reference, which we don't want because of reloading.
        /// </summary>
        public static ShaderProgram CreateCopied(ShaderProgram other)
        {
            var newProgram = new ShaderProgram();
            newProgram.CopyFrom(other);
            return newProgram;
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
            if (_uniformLocationsMap.TryGetValue(name, out int val)) return val;

            // If not, request it from OpenGL and cache it.
            int location = Gl.GetUniformLocation(Pointer, name);
            _uniformLocationsMap.Add(name, location);

            return location;
        }

        /// <summary>
        /// Set the value of the uniform of the specified name to the provided matrix4x4.
        /// </summary>
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
        /// Set the value of the uniform of the specified name to the provided matrix4x4 array.
        /// </summary>
        public bool SetUniformMatrix4(string name, Matrix4x4[] matrix4, int length)
        {
            int id = GetUniformLocation(name);
            // Check if the id exists.
            if (id == -1) return false;

            unsafe
            {
                ref Matrix4x4 mat = ref matrix4[0];
                fixed (float* matrixPtr = &mat.M11)
                {
                    Gl.UniformMatrix4(id, length, false, matrixPtr);
                }
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
        /// Copy the properties of a shader program into this one.
        /// Used for maintaining the shader reference when reloading shaders.
        /// </summary>
        /// <param name="otherProgram"></param>
        public void CopyFrom(ShaderProgram otherProgram)
        {
            if (otherProgram == null)
            {
                Pointer = 0;
                Valid = false;
                return;
            }

            Pointer = otherProgram.Pointer;
            Valid = otherProgram.Valid;
            DebugFragSource = otherProgram.DebugFragSource;
            DebugVertSource = otherProgram.DebugVertSource;
            DebugName = otherProgram.DebugName;
            _uniformLocationsMap.Clear();
        }

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

                Gl.Get(GetPName.CurrentProgram, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed bound shader was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            Gl.UseProgram(pointer);
            Bound = pointer;
        }
    }
}