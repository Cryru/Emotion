// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.IO;
using Emotion.Libraries;
using Emotion.Primitives;
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

        /// <summary>
        /// The currently bound data buffer.
        /// </summary>
        public static uint BoundDataBuffer { get; private set; }

        /// <summary>
        /// The currently bound vertex array buffer.
        /// </summary>
        public static uint BoundVertexArrayBuffer { get; private set; }

        /// <summary>
        /// The currently bound index buffer.
        /// </summary>
        public static uint BoundIndexBuffer { get; private set; }

        /// <summary>
        /// The shader currently bound.
        /// </summary>
        public static ShaderProgram CurrentShader { get; private set; }

        #endregion

        #region Shader State

        /// <summary>
        /// The location of vertices within the shader.
        /// </summary>
        public static readonly uint VertexLocation = 0;

        /// <summary>
        /// The location of the texture UV within the shader.
        /// </summary>
        public static readonly uint UvLocation = 1;

        /// <summary>
        /// The location of the texture id within the shader.
        /// </summary>
        public static readonly uint TidLocation = 2;

        /// <summary>
        /// The location of the colors within the shader.
        /// </summary>
        public static readonly uint ColorLocation = 3;

        #endregion

        #region Cache

        private static uint _defaultQuadIbo;
        private static ShaderProgram _defaultProgram;
        private static string _defaultFragShader;
        private static string _defaultVertShader;
        private static uint _renderFBO;
        private static Texture _renderFBOTexture;
        private static Rectangle actualViewport = new Rectangle();

        #endregion

        private static bool _isSetup;

        /// <summary>
        /// Setups the graphic's context and defaults. Should be ran on the GL thread.
        /// </summary>
        public static void Setup()
        {
            if (_isSetup) return;
            _isSetup = true;

            // Bind this thread as the GL thread.
            GLThread.BindThread();

            // Get version.
            string versionString = GL.GetString(StringName.Version);
            Context.Log.Info($"GL: {versionString} on {GL.GetString(StringName.Renderer)}", MessageSource.GL);
            int spaceIndex = versionString.IndexOf(' ');
            string[] version = new string[0];
            if(spaceIndex != -1 ) version = versionString.Substring(0, spaceIndex).Split('.');
            if (version.Length > 0)
            {
                bool parsed = int.TryParse(version[0].Trim(), out int majorVer);
                if (!parsed)
                    Context.Log.Warning($"Couldn't parse OpenGL major version - {version[0]}", MessageSource.GL);
                else
                    OpenGLMajorVersion = majorVer;
                parsed = int.TryParse(version[1].Trim(), out int minorVer);
                if (!parsed)
                    Context.Log.Warning($"Couldn't parse OpenGL minor version - {version[1]}", MessageSource.GL);
                else
                    OpenGLMinorVersion = minorVer;
            }
            else
            {
                Context.Log.Warning("Couldn't parse OpenGL version.", MessageSource.GL);
            }

            // Check for minimum version.
            if (OpenGLMajorVersion < 3 && OpenGLMinorVersion < 3)
                Context.Log.Warning("An OpenGL context lower than version 3.3 was created. This is not fully supported. Expect the unexpected.", MessageSource.Renderer);

            // Diagnostic dump.
            Context.Log.Info($"Creating GraphicsManager. Detected OGL is {OpenGLMajorVersion}.{OpenGLMinorVersion}", MessageSource.GL);
            Context.Log.Info($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}", MessageSource.GL);

            // Set execution flags, used for abstracting different GPU behavior.
            SetFlags();

            // Create default shaders and use them.
            _defaultProgram = Context.AssetLoader.Get<ShaderAsset>("Shaders/DefaultShader.xml").Shader;
            BindShaderProgram(_defaultProgram);

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

            // Create the FBO which rendering will be done to.
            GL.GenFramebuffers(1, out _renderFBO);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _renderFBO);

            // Create the texture.
            GL.GenTextures(1, out uint renderTexture);
            GL.BindTexture(TextureTarget.Texture2D, renderTexture);
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgb, (int) Context.Settings.RenderSettings.Width, (int) Context.Settings.RenderSettings.Height, 0, PixelFormat.Rgb,
                PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

            // Attach the texture to the frame buffer.
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget2d.Texture2D, renderTexture, 0);

            // Attach color components.
            DrawBufferMode[] modes = {DrawBufferMode.ColorAttachment0};
            GL.DrawBuffers(1, modes);

            // Create depth buffer.
            GL.GenRenderbuffers(1, out uint depthBuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, (int)Context.Settings.RenderSettings.Width,
                (int)Context.Settings.RenderSettings.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

            // Check status.
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Context.Log.Warning($"Framebuffer creation failed. Error code {status}.", MessageSource.GL);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _renderFBOTexture = new Texture(renderTexture, new Vector2(Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height));

            // Set default state.
            DefaultGLState();
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
                return;
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
            // If shader version 400 is supported enable it. It should replace the need for that extension.
            if (OpenGLMajorVersion >= 4)
            {
                Context.Log.Warning("The extension GL_ARB_GPU_SHADER5 was not found. Shader version changed from '300 es` to 400'.", MessageSource.GL);
                Context.Flags.RenderFlags.ShaderVersionOverride = "#version 400";
            }
        }

        /// <summary>
        /// Reset the GL state to the default one.
        /// </summary>
        public static void DefaultGLState()
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
            // Reset bound buffers. Some drivers unbind objects when swapping buffers.
            BoundDataBuffer = 0;
            BoundVertexArrayBuffer = 0;
            BoundIndexBuffer = 0;
            CurrentShader = null;
            BindShaderProgram(_defaultProgram);
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

        /// <summary>
        /// Set the viewport within the window.
        /// </summary>
        /// <param name="x">X position of the viewport.</param>
        /// <param name="y">Y position of the viewport.</param>
        /// <param name="width">Width of the viewport.</param>
        /// <param name="height">Height of the viewport.</param>
        public static void SetViewport(int x, int y, int width, int height)
        {
            actualViewport = new Rectangle(x, y, width, height);

            GLThread.ExecuteGLThread(() =>
            {
                GL.Viewport(x, y, width, height);
                GL.Scissor(x, y, width, height);
            });
        }

        /// <summary>
        /// Clears the framebuffer. Must be run on the GL thread.
        /// </summary>
        public static void ClearScreen()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GLThread.CheckError("clear");

            // Start rendering to the custom framebuffer.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _renderFBO);
            GL.Viewport(0, 0, (int) Context.Settings.RenderSettings.Width, (int) Context.Settings.RenderSettings.Height);
            GL.Scissor(0, 0, (int) Context.Settings.RenderSettings.Width, (int) Context.Settings.RenderSettings.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GLThread.CheckError("setting framebuffer");
        }

        /// <summary>
        /// Flushes the internal FBO to the main FBO.
        /// </summary>
        public static void FlushBackbuffer()
        {
            // Restore the actual frame buffer.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport((int) actualViewport.X, (int) actualViewport.Y, (int) actualViewport.Width, (int) actualViewport.Height);
            GL.Viewport((int) actualViewport.X, (int) actualViewport.Y, (int) actualViewport.Width, (int) actualViewport.Height);

            // Render the internal fbo.
            CurrentShader.SetUniformMatrix4("modelMatrix", Matrix4x4.Identity);
            CurrentShader.SetUniformMatrix4("viewMatrix", Matrix4x4.Identity);
            Context.Renderer.Render(Vector3.Zero, _renderFBOTexture.Size, Color.White, _renderFBOTexture);
            Context.Renderer.Submit();

            GLThread.CheckError("flushing framebuffer");
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
        /// <returns>Whether the binding was changed.</returns>
        public static bool BindDataBuffer(uint bufferId)
        {
#if DEBUG

            uint actualBound = GetBoundDataBuffer();

            if (BoundDataBuffer != 0 && BoundDataBuffer != actualBound)
                Context.Log.Warning($"Bound data buffer was thought to be {BoundDataBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundDataBuffer != 0 && BoundDataBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
                BoundDataBuffer = bufferId;
                GLThread.CheckError("after binding data buffer");
            });

            return true;
        }

        /// <summary>
        /// Upload data to the currently bound data buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data to upload.</param>
        public static void UploadToDataBuffer<T>(T[] data) where T : struct
        {
            if (BoundDataBuffer == 0) Context.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

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
            if (BoundDataBuffer == 0) Context.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (int) size, data, BufferUsageHint.StreamDraw);
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
            if (bufferId == BoundDataBuffer) BoundDataBuffer = 0;
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
        /// <returns>Whether the binding was changed.</returns>
        public static bool BindVertexArrayBuffer(uint bufferId)
        {
#if DEBUG

            uint actualBound = GetBoundVertexArrayBuffer();

            if (BoundVertexArrayBuffer != 0 && BoundVertexArrayBuffer != actualBound)
                Context.Log.Warning($"Bound vertex array buffer was thought to be {BoundVertexArrayBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundVertexArrayBuffer != 0 && BoundVertexArrayBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                GL.BindVertexArray(bufferId);
                BoundVertexArrayBuffer = bufferId;
                BoundIndexBuffer = GetBoundIndexBuffer();
                BoundDataBuffer = GetBoundDataBuffer();
                GLThread.CheckError("after binding vertex array buffer");
            });

            return true;
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
        public static void AttachDataBufferToVertexArray(uint dataBufferId, uint vertexArrayBufferId, uint shaderIndex, uint componentCount, DataType dataType, bool normalized, uint stride,
            uint offset)
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
            if (bufferId == BoundVertexArrayBuffer) BoundVertexArrayBuffer = 0;
        }

        #endregion

        #region Index Buffer API

        /// <summary>
        /// Bind a data buffer as an index buffer.
        /// </summary>
        /// <param name="bufferId">The id of the data buffer to bind as an index buffer.</param>
        /// <returns>Whether the binding was changed.</returns>
        public static bool BindIndexBuffer(uint bufferId)
        {
#if DEBUG

            uint actualBound = GetBoundIndexBuffer();

            if (BoundIndexBuffer != 0 && BoundIndexBuffer != actualBound)
                Context.Log.Warning($"Bound index buffer was thought to be {BoundIndexBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundIndexBuffer != 0 && BoundIndexBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferId);
                BoundIndexBuffer = bufferId;
                GLThread.CheckError("after binding index buffer");
            });

            return true;
        }

        /// <summary>
        /// Upload data to the currently bound index buffer.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="data">The data to upload.</param>
        public static void UploadToIndexBuffer<T>(T[] data) where T : struct
        {
            if (BoundIndexBuffer == 0) Context.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

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
            if (BoundIndexBuffer == 0) Context.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, (int) size, data, BufferUsageHint.StaticDraw);
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
                BindVertexArrayBuffer(vao);
                AttachDataBufferToVertexArray(vbo, vao, VertexLocation, 3, DataType.Float, false, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"));
                AttachDataBufferToVertexArray(vbo, vao, UvLocation, 2, DataType.Float, false, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "UV"));
                AttachDataBufferToVertexArray(vbo, vao, TidLocation, 1, DataType.Float, true, (uint) VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Tid"));
                AttachDataBufferToVertexArray(vbo, vao, ColorLocation, 4, DataType.UnsignedByte, true, (uint) VertexData.SizeInBytes,
                    (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));
                BindVertexArrayBuffer(0);
                BindDataBuffer(0);

                // Create a GL stream buffer.
                streamBuffer = new GLStreamBuffer(vbo, vao, _defaultQuadIbo, 4, size * 4, 6, PrimitiveType.Triangles);
            });

            return streamBuffer;
        }

        #endregion

        #region Shader API

        /// <summary>
        /// Compile a vertex shader and fragment shader into a shader program.
        /// </summary>
        /// <param name="vert">The vertex shader source to compile. If empty or null the default one will be used.</param>
        /// <param name="frag">The fragment shader source to compile. If empty or null the default one will be used.</param>
        /// <returns>A shader program implementation.</returns>
        public static ShaderProgram CreateShaderProgram(string vert, string frag)
        {
            // Set defaults if missing.
            if (_defaultFragShader == null)
                _defaultFragShader = frag;
            if (_defaultVertShader == null)
                _defaultVertShader = vert;

            uint vertId;
            uint fragId;
            ShaderProgram newProgram = null;

            GLThread.ExecuteGLThread(() =>
            {
                if (string.IsNullOrEmpty(vert))
                {
                    vert = _defaultVertShader;
                }

                vertId = CompileShader(ShaderType.VertexShader, vert);
                if (vertId == 0)
                {
                    newProgram = null;
                    return;
                }

                if (string.IsNullOrEmpty(frag))
                {
                    frag = _defaultFragShader;
                }

                fragId = CompileShader(ShaderType.FragmentShader, frag);
                if (fragId == 0)
                {
                    newProgram = null;
                    return;
                }

                newProgram = new GLShaderProgram(vertId, fragId);
                
                // Delete shaders.
                GL.DeleteShader(fragId);
                GL.DeleteShader(vertId);

                // Save current as not to creation to override the shader.
                ShaderProgram current = CurrentShader;

                // Set default uniform.
                BindShaderProgram(newProgram);
                newProgram.SetUniformIntArray("textures", Enumerable.Range(0, 15).ToArray());
                newProgram.SetUniformFloat("time", 0);

                // Restore bound.
                BindShaderProgram(current);
            });

            return newProgram;
        }

        /// <summary>
        /// Bind a shader as the current shader.
        /// </summary>
        /// <param name="shaderProgram">The shader to bind, or null to bind the default one.</param>
        /// <returns>Whether the binding was changed.</returns>
        public static bool BindShaderProgram(ShaderProgram shaderProgram)
        {
            // Check if restoring default.
            if (shaderProgram == null) shaderProgram = _defaultProgram;

            if (CurrentShader == shaderProgram) return false;

            GLThread.ExecuteGLThread(() =>
            {
                GL.UseProgram(shaderProgram?.Id ?? 0);
                CurrentShader = shaderProgram;
            });

            return true;
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
            if (!parsed) Context.Log.Warning($"Couldn't parse data type - {emotionDataType}", MessageSource.GL);

            return nativeDataType;
        }

        /// <summary>
        /// Get actual currently bound index buffer. Skipping the cache.
        /// </summary>
        /// <returns>The id of the currently bound index buffer.</returns>
        public static uint GetBoundIndexBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => GL.GetInteger(GetPName.ElementArrayBufferBinding, out id));
            return (uint) id;
        }

        /// <summary>
        /// Get actual currently bound data buffer. Skipping the cache.
        /// </summary>
        /// <returns>The id of the currently bound data buffer.</returns>
        public static uint GetBoundDataBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => GL.GetInteger(GetPName.ArrayBufferBinding, out id));
            return (uint) id;
        }

        /// <summary>
        /// Get actual currently bound vertex array buffer. Skipping the cache.
        /// </summary>
        /// <returns>The id of the currently bound vertex array buffer.</returns>
        public static uint GetBoundVertexArrayBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => GL.GetInteger(GetPName.VertexArrayBinding, out id));
            return (uint) id;
        }

        /// <summary>
        /// Compiles a shader.
        /// </summary>
        /// <param name="type">The type of shader to compile.</param>
        /// <param name="source">The shader source.</param>
        /// <returns>The id of the compiled shader.</returns>
        private static uint CompileShader(ShaderType type, string source)
        {
            // Check if a version override is set.
            if (!string.IsNullOrEmpty(Context.Flags.RenderFlags.ShaderVersionOverride)) source = source.Replace("#version 300 es", Context.Flags.RenderFlags.ShaderVersionOverride);

            // Create and compile the shader.
            uint shaderPointer = (uint) GL.CreateShader(type);
            GL.ShaderSource((int) shaderPointer, source);
            GL.CompileShader(shaderPointer);

            GLThread.CheckError($"shader compilation\n{source}");

            // Check if the shader compiled successfully.
            GL.GetShader(shaderPointer, ShaderParameter.CompileStatus, out int status);
            if (status == 1)
            {
                return shaderPointer;
            }

            // Check compilation status.
            string compileStatus = GL.GetShaderInfoLog((int) shaderPointer);
            Context.Log.Warning($"Failed to compile shader of type {type} with error {compileStatus}.\nSource:{source}", MessageSource.GL);
            return 0;
        }

        #endregion
    }
}