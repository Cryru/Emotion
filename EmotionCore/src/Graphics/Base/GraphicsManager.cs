// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.Objects;
using Emotion.Libraries;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Base
{
    /// <summary>
    /// Manages OpenGL state and such.
    /// </summary>
    public static class GraphicsManager
    {
        #region Properties

        /// <summary>
        /// The OpenGL major version loaded.
        /// </summary>
        public static int OpenGLMajorVersion { get; private set; }

        /// <summary>
        /// The OpenGL minor version loaded.
        /// </summary>
        public static int OpenGLMinorVersion { get; private set; }

        #endregion

        #region Render State

        private static uint _boundDataBuffer;
        private static uint _boundVertexArrayBuffer;
        private static uint _boundIndexBuffer;

        #endregion

        #region Cache

        private static uint _defaultQuadIbo;

        #endregion

        private static bool _isSetup;

        /// <summary>
        /// Setups the graphic's context and defaults. Should be ran on the GL thread.
        /// </summary>
        public static void Setup()
        {
            if (_isSetup) return;
            _isSetup = true;

            // Check for minimum version.
            if (Context.Flags.RenderFlags.OpenGLMajorVersion < 3 || Context.Flags.RenderFlags.OpenGLMinorVersion < 3) Context.Log.Error("Minimum OpenGL version is 3.3", MessageSource.Renderer);

            // Bind this thread as the GL thread.
            GLThread.BindThread();

            // Get version.
            string[] version = GL.GetString(StringName.Version).Split('.');
            if (version.Length > 0)
            {
                bool parsed = int.TryParse(version[0], out int majorVer);
                if (!parsed)
                    Context.Log.Warning($"Couldn't parse OpenGL major version - {version[0]}", MessageSource.GL);
                else
                    OpenGLMajorVersion = majorVer;
                parsed = int.TryParse(version[1], out int minorVer);
                if (!parsed)
                    Context.Log.Warning($"Couldn't parse OpenGL minor version - {version[1]}", MessageSource.GL);
                else
                    OpenGLMinorVersion = minorVer;
            }
            else
            {
                Context.Log.Warning("Couldn't parse OpenGL version.", MessageSource.GL);
            }

            // Diagnostic dump.
            Context.Log.Info($"Creating GraphicsManager. Detected OGL is {OpenGLMajorVersion}.{OpenGLMinorVersion}", MessageSource.GL);
            Context.Log.Info($"GL: {GL.GetString(StringName.Version)} on {GL.GetString(StringName.Renderer)}", MessageSource.GL);
            Context.Log.Info($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}", MessageSource.GL);

            // Set execution flags, used for abstracting different GPU behavior.
            SetFlags();

            // Create default shaders. This also sets some shader flags.
            CreateDefaultShaders();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram((Shader)null, null);
            defaultProgram.Bind();

            // Check if the setup encountered any errors.
            GLThread.CheckError("graphics setup");

            // Create default quad ibo.
            ushort[] indices = new ushort[Renderer.MaxRenderable * 6];
            uint offset = 0;
            for (int i = 0; i < indices.Length; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }
            _defaultQuadIbo = CreateDataBuffer();
            BindIndexBuffer(_defaultQuadIbo);
            UploadToIndexBuffer(indices);

            // Set default state.
            ResetGLState();
            ResetState();
        }

        /// <summary>
        /// Sets flags needed to align GPU behavior.
        /// </summary>
        private static void SetFlags()
        {
            // Override shader version for Macs.
            if (CurrentPlatform.OS == PlatformName.Mac)
            {
                Context.Flags.RenderFlags.ShaderVersionOverride = "#version 330";
                Context.Log.Warning("Shader version changed from '300 es' to '330' because Mac platform was detected.", MessageSource.GL);
            }

            // Flag missing extensions.
            int extCount = GL.GetInteger(GetPName.NumExtensions);
            bool found = false;
            for (int i = 0; i < extCount; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension.ToLower() != "gl_arb_gpu_shader5") continue;
                found = true;
                break;
            }

            if (found) return;
            Context.Log.Warning("The extension GL_ARB_GPU_SHADER5 was not found. Shader version changed from '300 es` to 400'.", MessageSource.GL);
            Context.Flags.RenderFlags.ShaderVersionOverride = "#version 400";
        }

        /// <summary>
        /// Creates the default shaders embedded into the binary.
        /// </summary>
        private static void CreateDefaultShaders()
        {
            string defaultVert = Helpers.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultVert.glsl");
            string defaultFrag = Helpers.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultFrag.glsl");

            try
            {
                ShaderProgram.DefaultVertShader = new Shader(ShaderType.VertexShader, defaultVert);
                ShaderProgram.DefaultFragShader = new Shader(ShaderType.FragmentShader, defaultFrag);
            }
            catch (Exception ex)
            {
                // Check if one of the expected exceptions.
                if (new Regex("gl_arb_gpu_shader5").IsMatch(ex.ToString().ToLower()))
                {
                    // So the extension is not supported. Try to compile with shader version "400".
                    Context.Log.Warning("The extension GL_ARB_GPU_SHADER5 was found, but is not supported. Shader version changed from '300 es` to 400'.", MessageSource.GL);
                    Context.Flags.RenderFlags.ShaderVersionOverride = "#version 400";

                    // Cleanup created ones if any.
                    ShaderProgram.DefaultVertShader?.Destroy();
                    ShaderProgram.DefaultFragShader?.Destroy();

                    // Recreate shaders. If version 400 is not supported too, then minimum requirements aren't met.
                    CreateDefaultShaders();
                }
                else
                {
                    // Some other error was found.
                    throw;
                }
            }

            GLThread.CheckError("making default shaders");
        }

        /// <summary>
        /// Reset the GL state to the default one.
        /// </summary>
        public static void ResetGLState()
        {
            GLThread.CheckError("after setting default state");

            // Reset blend.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Reset depth test.
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            // Reset cull face.
            GL.Disable(EnableCap.CullFace);

            // Reset depth mask.
            GL.DepthMask(true);

            // Reset scissor.
            GL.Disable(EnableCap.ScissorTest);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Reset color mask.
            GL.ColorMask(true, true, true, true);

            GLThread.CheckError("before setting default state");
        }

        /// <summary>
        /// Reset the state and defaults.
        /// </summary>
        public static void ResetState()
        {
            // Reset bound buffers.
            _boundDataBuffer = 0;
            _boundVertexArrayBuffer = 0;
            _boundIndexBuffer = 0;
        }

        #region State API

        /// <summary>
        /// Enables or disabled depth testing.
        /// </summary>
        /// <param name="enable">Whether to enable or disable depth testing. Is on by default.</param>
        public static void StateDepthTest(bool enable)
        {
            GLThread.ExecuteGLThread(() =>
            {
                if (enable)
                    GL.Enable(EnableCap.DepthTest);
                else
                    GL.Disable(EnableCap.DepthTest);

                GLThread.CheckError("setting depth test");
            });
        }

        #endregion

        #region Data Buffer API

        /// <summary>
        /// Create a new data buffer.
        /// </summary>
        public static uint CreateDataBuffer()
        {
            uint newBufferId = 0;

            GLThread.ExecuteGLThread(() =>
            {
                // Create buffer.
                GL.GenBuffers(1, out newBufferId);

                GLThread.CheckError("after data buffer creation");
            });

            return newBufferId;
        }

        /// <summary>
        /// Bind a data buffer.
        /// </summary>
        /// <param name="bufferId">The id of the data buffer to bind.</param>
        public static void BindDataBuffer(uint bufferId)
        {
            // Check if already bound.
            if (_boundDataBuffer == bufferId) return;

            GLThread.ExecuteGLThread(() =>
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
                _boundDataBuffer = bufferId;
                GLThread.CheckError("after binding data buffer");
            });
        }

        /// <summary>
        /// Upload data to the currently bound data buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data to upload.</param>
        public static void UploadToDataBuffer<T>(T[] data) where T : struct
        {
            if (_boundDataBuffer == 0) Context.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

            int byteSize = Marshal.SizeOf(data[0]);
            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ArrayBuffer, data.Length * byteSize, data, BufferUsageHint.StreamDraw);
                GLThread.CheckError("after uploading data buffer data");
            });
        }

        /// <summary>
        /// Upload data to the currently bound data buffer.
        /// </summary>
        /// <param name="data">The data to upload.</param>
        /// <param name="size">The size of the data to upload.</param>
        public static void UploadToDataBuffer(IntPtr data, uint size)
        {
            if (_boundDataBuffer == 0) Context.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (int)size, data, BufferUsageHint.StreamDraw);
                GLThread.CheckError("after uploading data buffer data from pointer");
            });
        }

        /// <summary>
        /// Destroy a data buffer.
        /// </summary>
        /// <param name="bufferId">The id of the data buffer to destroy.</param>
        public static void DestroyDataBuffer(uint bufferId)
        {
            GLThread.ExecuteGLThread(() =>
            {
                GL.DeleteBuffer(bufferId);
                GLThread.CheckError("after deleting data buffer");
            });
            // Revert binding if deleted bound.
            if (bufferId == _boundDataBuffer) _boundDataBuffer = 0;
        }

        #endregion

        #region Vertex Array buffer API

        /// <summary>
        /// Create a new vertex array buffer.
        /// </summary>
        /// <returns>The id of the created vertex array buffer. Or 0 if creation failed.</returns>
        public static uint CreateVertexArrayBuffer()
        {
            uint newBufferId = 0;

            GLThread.ExecuteGLThread(() =>
            {
                // Create buffer.
                GL.GenVertexArrays(1, out newBufferId);
               
                GLThread.CheckError("after vertex array buffer creation");
            });

            return newBufferId;
        }

        /// <summary>
        /// Bind a vertex array buffer.
        /// </summary>
        /// <param name="bufferId">The id of the vertex array buffer to bind.</param>
        public static void BindVertexArrayBuffer(uint bufferId)
        {
            // Check if already bound.
            if (_boundVertexArrayBuffer == bufferId) return;

            GLThread.ExecuteGLThread(() =>
            {
                GL.BindVertexArray(bufferId);
                _boundVertexArrayBuffer = bufferId;
                GLThread.CheckError("after binding vertex array buffer");
            });
        }

        /// <summary>
        /// Attach a data buffer to the vertex array data. Overwrites the currently bound data buffer and vertex array buffer.
        /// </summary>
        /// <param name="dataBufferId">The data buffer to attach to the vertex array.</param>
        /// <param name="vertexArrayBufferId">The vertex array buffer to attach to.</param>
        /// <param name="shaderIndex">The index of the buffer data for the shader.</param>
        /// <param name="componentCount">The component count of the buffer. For instance Vector3 is three components of type float.</param>
        /// <param name="dataType">The type of data within the buffer.</param>
        /// <param name="normalized">Whether the value is normalized.</param>
        /// <param name="stride">The byte offset between consecutive vertex attributes.</param>
        /// <param name="offset">The offset of the first piece of data.</param>
        public static void AttachDataBufferToVertexArray(uint dataBufferId, uint vertexArrayBufferId, uint shaderIndex, uint componentCount, DataType dataType, bool normalized, uint stride, uint offset)
        {
            GLThread.ExecuteGLThread(() =>
            {
                BindVertexArrayBuffer(vertexArrayBufferId);
                BindDataBuffer(dataBufferId);
                GL.EnableVertexAttribArray(shaderIndex);
                GL.VertexAttribPointer(shaderIndex, (int) componentCount, EmotionToNativePointerType(dataType), normalized, (int) stride, (int) offset);

                GLThread.CheckError("after binding vertex array buffer");
            });
        }

        /// <summary>
        /// Destroy a vertex array buffer.
        /// </summary>
        /// <param name="bufferId">The id of the vertex array buffer to destroy.</param>
        public static void DestroyVertexArrayBuffer(uint bufferId)
        {
            GLThread.ExecuteGLThread(() =>
            {
                GL.DeleteVertexArray(bufferId);
                GLThread.CheckError("after deleting vertex array buffer");
            });
            // Revert binding if deleted bound.
            if (bufferId == _boundVertexArrayBuffer) _boundVertexArrayBuffer = 0;
        }

        #endregion

        #region Index Buffer API

        /// <summary>
        /// Bind a data buffer as an index buffer.
        /// </summary>
        /// <param name="bufferId">The id of the data buffer to bind as an index buffer.</param>
        public static void BindIndexBuffer(uint bufferId)
        {
            // Check if already bound.
            if (_boundIndexBuffer == bufferId) return;

            _boundIndexBuffer = bufferId;
            GLThread.ExecuteGLThread(() =>
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferId);
                GLThread.CheckError("after binding index buffer");
            });
        }

        /// <summary>
        /// Upload data to the currently bound index buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data to upload.</param>
        public static void UploadToIndexBuffer<T>(T[] data) where T : struct
        {
            if (_boundIndexBuffer == 0) Context.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

            int byteSize = Marshal.SizeOf(data[0]);
            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * byteSize, data, BufferUsageHint.StaticDraw);
                GLThread.CheckError("after uploading index buffer data");
            });
        }

        /// <summary>
        /// Upload data to the currently bound index buffer.
        /// </summary>
        /// <param name="data">The data to upload.</param>
        /// <param name="size">The size of the data to upload.</param>
        public static void UploadToIndexBuffer(IntPtr data, uint size)
        {
            if (_boundIndexBuffer == 0) Context.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, (int)size, data, BufferUsageHint.StaticDraw);
                GLThread.CheckError("after uploading index buffer data from pointer");
            });
        }

        #endregion

        #region MapBuffer API

        /// <summary>
        /// Create a streaming buffer used for drawing 2d quads.
        /// </summary>
        /// <param name="size">The size of the buffer.</param>
        /// <returns>A streaming buffer used for drawing 2d quads.</returns>
        public static StreamBuffer CreateQuadMapBuffer(uint size)
        {
            StreamBuffer streamBuffer = null;

            GLThread.ExecuteGLThread(() =>
            {
                // First create the vbo.
                uint vbo = CreateDataBuffer();
                BindDataBuffer(vbo);
                UploadToDataBuffer(IntPtr.Zero, (uint) (size * 4 * VertexData.SizeInBytes));

                // Create the ibo.
                uint vao = CreateVertexArrayBuffer();
                AttachDataBufferToVertexArray(vbo, vao, ShaderProgram.VertexLocation, 3, DataType.Float, false, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"));
                AttachDataBufferToVertexArray(vbo, vao, ShaderProgram.UvLocation, 2, DataType.Float, false, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "UV"));
                AttachDataBufferToVertexArray(vbo, vao, ShaderProgram.TidLocation, 1, DataType.Float, true, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Tid"));
                AttachDataBufferToVertexArray(vbo, vao, ShaderProgram.ColorLocation, 4, DataType.UnsignedByte, true, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));
                BindVertexArrayBuffer(0);
                BindDataBuffer(0);

                // Create a GL stream buffer.
                streamBuffer = new GLStreamBuffer(vbo, vao, _defaultQuadIbo, 4, size * 4, 6, PrimitiveType.Triangles);
            });

            return streamBuffer;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert an Emotion engine data type to a native one.
        /// </summary>
        /// <returns></returns>
        private static VertexAttribPointerType EmotionToNativePointerType(DataType emotionDataType)
        {
            bool parsed = Enum.TryParse(emotionDataType.ToString(), out VertexAttribPointerType nativeDataType);
            if (!parsed)
            {
                Context.Log.Warning($"Couldn't parse data type - {emotionDataType}", MessageSource.GL);
            }

            return nativeDataType;
        }

        #endregion
    }
}