#region Using

using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Logging;
using Adfectus.Platform.DesktopGL.Assets;
using Adfectus.Primitives;
using OpenGL;
using DataType = Adfectus.Graphics.DataType;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    /// <summary>
    /// A glfw graphics manager using OpenGL.Net
    /// </summary>
    public sealed class GlfwGraphicsManager : GraphicsManager
    {
        #region Cache

        private string _defaultFragShader;
        private string _defaultVertShader;

        #endregion

        /// <inheritdoc />
        public override void Setup(Vector2 renderSize)
        {
            RenderSize = renderSize;

            Gl.BindAPI(s => Glfw.GetProcAddress(s));
            Gl.QueryContextVersion();
            // Check if context was created.
            if (Gl.CurrentVersion == null)
            {
                ErrorHandler.SubmitError(new Exception("Couldn't connect to OpenGL context."));
                return;
            }

            // Bind current thread as the GLThread.
            GLThread.BindThread();

            // Renderer bootstrap.
            Engine.Log.Info("Creating OpenGL GraphicsManager...", MessageSource.GL);
            Engine.Log.Info($"GL: {Gl.CurrentVersion} on {Gl.CurrentRenderer}", MessageSource.GL);
            Engine.Log.Info($"GLSL: {Gl.CurrentShadingVersion}", MessageSource.GL);

            CreateDefaultShader();
            CreateDefaultIbo();

            // Set default state.
            DefaultGLState();
            ResetState();

            // Clear to transparent black.
            Gl.ClearColor(0, 0, 0, 0);

            Engine.Log.Info("GraphicsManager ready.", MessageSource.GL);
        }

        /// <inheritdoc />
        public override void CheckError(string location = "")
        {
            ErrorCode errorCheck = Gl.GetError();
            if (errorCheck != ErrorCode.NoError) ErrorHandler.SubmitError(new Exception($"OpenGL error at {location}: {errorCheck}"));
        }

        /// <inheritdoc />
        public override void DefaultGLState()
        {
            CheckError("after setting default state");

            // Reset blend.
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Reset depth test.
            StateDepthTest(true);
            Gl.DepthFunc(DepthFunction.Lequal);

            // Reset stencil.
            StateStencilTest(false);

            // Reset cull face.
            Gl.Disable(EnableCap.CullFace);

            // Reset depth mask.
            Gl.DepthMask(true);

            // Reset scissor.
            SetClipRect(0, 0, (int) RenderSize.X, (int) RenderSize.Y);
            Gl.FrontFace(FrontFaceDirection.Ccw);

            // Reset color mask.
            Gl.ColorMask(true, true, true, true);

            CheckError("before setting default state");
        }

        #region State API

        /// <inheritdoc />
        public override void StateDepthTest(bool enable)
        {
            Engine.Renderer?.Submit();

            GLThread.ExecuteGLThread(() =>
            {
                if (enable)
                    Gl.Enable(EnableCap.DepthTest);
                else
                    Gl.Disable(EnableCap.DepthTest);

                CheckError("setting depth test");
            });
        }

        /// <inheritdoc />
        public override void StateClip(bool enable)
        {
            GLThread.ExecuteGLThread(() =>
            {
                if (enable)
                    Gl.Enable(EnableCap.ScissorTest);
                else
                    Gl.Disable(EnableCap.ScissorTest);

                CheckError("setting scissor (clip) state");
            });
        }

        /// <inheritdoc />
        public override void SetClipRect(int x, int y, int width, int height)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.Enable(EnableCap.ScissorTest);
                Gl.Scissor(x, (int) (RenderSize.Y - height - y), width, height);

                CheckError("setting clip rect");
            });
        }

        /// <inheritdoc />
        public override void SetViewport(int x, int y, int width, int height)
        {
            Viewport = new Rectangle(x, y, width, height);

            GLThread.ExecuteGLThread(() => { Gl.Viewport(x, y, width, height); });
        }

        /// <inheritdoc />
        public override void ClearScreen()
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            CheckError("clear");

            // Fix for certain intel cards. They need a sync somewhere, and it's either this or a glFinish.
            Gl.GetError();
        }

        #endregion

        #region Texture API

        /// <inheritdoc />
        public override uint CreateTexture()
        {
            uint newTexture = 0;

            GLThread.ExecuteGLThread(() =>
            {
                newTexture = Gl.GenTexture();
                CheckError("after texture creation");
            });

            return newTexture;
        }

        /// <inheritdoc />
        public override void DeleteTexture(uint pointer)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.DeleteTextures(pointer);
                CheckError("after texture deletion");
            });
        }

        public override bool BindTexture(Texture texture, uint slot = 0)
        {
            return BindTexture(((GLTexture) texture).Pointer);
        }

        /// <inheritdoc />
        public override bool BindTexture(uint pointer, uint slot = 0)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.ActiveTexture(TextureUnit.Texture0 + (int) slot);
                Gl.BindTexture(TextureTarget.Texture2d, pointer);

                CheckError("after binding texture");
            });

            return true;
        }

        /// <inheritdoc />
        public override void SetTextureMask(uint r = 0xff000000, uint g = 0x00ff0000, uint b = 0x0000ff00, uint a = 0x000000ff)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleR, MaskMeaning(r));
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleG, MaskMeaning(g));
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleB, MaskMeaning(b));
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleA, MaskMeaning(a));

                CheckError("setting texture mask");
            });
        }

        /// <inheritdoc />
        public override void UploadToTexture(IntPtr data, Vector2 size, TextureInternalFormat internalFormat, TexturePixelFormat pixelFormat)
        {
            InternalFormat intFormat = (InternalFormat) Enum.Parse(typeof(InternalFormat), internalFormat.ToString());
            PixelFormat glFormat = (PixelFormat) Enum.Parse(typeof(PixelFormat), pixelFormat.ToString());

            GLThread.ExecuteGLThread(() =>
            {
                // Set scaling to pixel perfect.
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);

                Gl.TexImage2D(TextureTarget.Texture2d, 0, intFormat, (int) size.X, (int) size.Y, 0, glFormat, PixelType.UnsignedByte, data);
                Gl.GenerateMipmap(TextureTarget.Texture2d);

                CheckError("after uploading texture");
            });
        }

        /// <inheritdoc />
        public override void SetTextureSmooth(bool smooth)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, smooth ? Gl.LINEAR : Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, smooth ? Gl.LINEAR : Gl.NEAREST);
            });
        }

        #endregion

        #region Data Buffer API

        /// <inheritdoc />
        public override uint CreateDataBuffer()
        {
            uint newBufferId = 0;

            GLThread.ExecuteGLThread(() =>
            {
                // Create buffer.
                newBufferId = Gl.GenBuffer();

                CheckError("after data buffer creation");
            });

            return newBufferId;
        }

        /// <inheritdoc />
        public override bool BindDataBuffer(uint bufferId)
        {
#if DEBUG
            uint actualBound = GetBoundDataBuffer();

            if (BoundDataBuffer != 0 && BoundDataBuffer != actualBound)
                Engine.Log.Warning($"Bound data buffer was thought to be {BoundDataBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundDataBuffer != 0 && BoundDataBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
                BoundDataBuffer = bufferId;
                CheckError("after binding data buffer");
            });

            return true;
        }

        /// <inheritdoc />
        public override void UploadToDataBuffer<T>(T[] data)
        {
            if (BoundDataBuffer == 0) Engine.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

            int byteSize = Marshal.SizeOf(data[0]);
            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (data.Length * byteSize), data, BufferUsage.StreamDraw);
                CheckError("after uploading data buffer data");
            });
        }

        /// <inheritdoc />
        public override void UploadToDataBuffer(IntPtr data, uint size)
        {
            if (BoundDataBuffer == 0) Engine.Log.Warning("You are trying to upload data, but no data buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferData(BufferTarget.ArrayBuffer, size, data, BufferUsage.StreamDraw);
                CheckError("after uploading data buffer data from pointer");
            });
        }

        /// <inheritdoc />
        public override void MapDataBuffer(IntPtr data, uint size, uint offset = 0)
        {
            if (BoundDataBuffer == 0) Engine.Log.Warning("You are trying to map data, but no data buffer is bound.", MessageSource.GL);
            IntPtr offsetPtr = (IntPtr) offset;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferSubData(BufferTarget.ArrayBuffer, offsetPtr, size, data);
                CheckError("after mapping data buffer data from pointer");
            });
        }

        /// <inheritdoc />
        public override void DestroyDataBuffer(uint bufferId)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.DeleteBuffers(bufferId);
                CheckError("after deleting data buffer");
            });
            // Revert binding if deleted bound.
            if (bufferId == BoundDataBuffer) BoundDataBuffer = 0;
        }

        #endregion

        #region Vertex Array buffer API

        /// <summary>
        /// Create a vao and bind it to a vbo.
        /// </summary>
        /// <param name="vbo">The VBO to bind to the vao.</param>
        /// <typeparam name="T">The structure which describes the vao format.</typeparam>
        /// <returns>The id of the created vao.</returns>
        private uint CreateVao<T>(uint vbo)
        {
            uint vaoId = Gl.GenVertexArray();
            Gl.BindVertexArray(vaoId);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            Type structFormat = typeof(T);

            FieldInfo[] fields = structFormat.GetFields(BindingFlags.Public | BindingFlags.Instance);
            int size = Marshal.SizeOf(Activator.CreateInstance(structFormat));

            for (uint i = 0; i < fields.Length; i++)
            {
                Attribute[] fieldAttributes = fields[i].GetCustomAttributes().ToArray();
                VertexAttributeAttribute vertexAttributeData = null;

                // Go through all attributes and find the VertexAttributeAttribute.
                foreach (Attribute attribute in fieldAttributes)
                {
                    if (!(attribute is VertexAttributeAttribute data)) continue;
                    vertexAttributeData = data;
                    break;
                }

                // If the vertex attribute data not found, stop searching.
                if (vertexAttributeData == null) continue;

                IntPtr offset = Marshal.OffsetOf(structFormat, fields[i].Name);
                Type fieldType = vertexAttributeData.TypeOverride ?? fields[i].FieldType;

                Gl.EnableVertexAttribArray(i);
                Gl.VertexAttribPointer(i, vertexAttributeData.ComponentCount, GetAttribTypeFromManagedType(fieldType), vertexAttributeData.Normalized, size, offset);
            }

            return vaoId;
        }


        /// <summary>
        /// Convert a managed C# type to a GL vertex attribute type.
        /// </summary>
        /// <param name="type">The managed type to convert.</param>
        /// <returns>The vertex attribute type corresponding to the provided managed type.</returns>
        public static VertexAttribType GetAttribTypeFromManagedType(MemberInfo type)
        {
            switch (type.Name)
            {
                default:
                    return VertexAttribType.Float;
                case "Int32":
                    return VertexAttribType.Int;
                case "UInt32":
                    return VertexAttribType.UnsignedInt;
                case "SByte":
                    return VertexAttribType.Byte;
                case "Byte":
                    return VertexAttribType.UnsignedByte;
            }
        }

        /// <inheritdoc />
        public override uint CreateVertexArrayBuffer()
        {
            uint newBufferId = 0;

            GLThread.ExecuteGLThread(() =>
            {
                // Create buffer.
                newBufferId = Gl.GenVertexArray();

                CheckError("after vertex array buffer creation");
            });

            return newBufferId;
        }

        /// <inheritdoc />
        public override bool BindVertexArrayBuffer(uint bufferId)
        {
#if DEBUG
            uint actualBound = GetBoundVertexArrayBuffer();

            if (BoundVertexArrayBuffer != 0 && BoundVertexArrayBuffer != actualBound)
                Engine.Log.Warning($"Bound vertex array buffer was thought to be {BoundVertexArrayBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundVertexArrayBuffer != 0 && BoundVertexArrayBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BindVertexArray(bufferId);
                BoundVertexArrayBuffer = bufferId;
                BoundIndexBuffer = GetBoundIndexBuffer();
                BoundDataBuffer = GetBoundDataBuffer();
                CheckError("after binding vertex array buffer");
            });

            return true;
        }

        /// <inheritdoc />
        public override void AttachDataBufferToVertexArray(uint dataBufferId, uint vertexArrayBufferId, uint shaderIndex, uint componentCount, DataType dataType, bool normalized, uint stride,
            IntPtr offset)
        {
            GLThread.ExecuteGLThread(() =>
            {
                BindVertexArrayBuffer(vertexArrayBufferId);
                BindDataBuffer(dataBufferId);
                Gl.EnableVertexAttribArray(shaderIndex);
                CheckError($"after enabling vertex array attributes {shaderIndex}");
                Gl.VertexAttribPointer(shaderIndex, (int) componentCount, ToGLPointerType(dataType), normalized, (int) stride, offset);

                CheckError($"after setting vertex array attribute {shaderIndex}");
            });
        }

        /// <inheritdoc />
        public override void DestroyVertexArrayBuffer(uint bufferId)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Gl.DeleteVertexArrays(bufferId);
                CheckError("after deleting vertex array buffer");
            });
            // Revert binding if deleted bound.
            if (bufferId == BoundVertexArrayBuffer) BoundVertexArrayBuffer = 0;
        }

        #endregion

        #region Index Buffer API

        /// <inheritdoc />
        public override bool BindIndexBuffer(uint bufferId)
        {
#if DEBUG
            uint actualBound = GetBoundIndexBuffer();

            if (BoundIndexBuffer != 0 && BoundIndexBuffer != actualBound)
                Engine.Log.Warning($"Bound index buffer was thought to be {BoundIndexBuffer} but is actually {actualBound}.", MessageSource.GL);

#endif

            // Check if already bound.
            if (BoundIndexBuffer != 0 && BoundIndexBuffer == bufferId) return false;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, bufferId);
                BoundIndexBuffer = bufferId;
                CheckError("after binding index buffer");
            });

            return true;
        }

        /// <inheritdoc />
        public override void UploadToIndexBuffer<T>(T[] data)
        {
            if (BoundIndexBuffer == 0) Engine.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

            int byteSize = Marshal.SizeOf(data[0]);
            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint) (data.Length * byteSize), data, BufferUsage.StaticDraw);
                CheckError("after uploading index buffer data");
            });
        }

        /// <inheritdoc />
        public override void UploadToIndexBuffer(IntPtr data, uint size)
        {
            if (BoundIndexBuffer == 0) Engine.Log.Warning("You are trying to upload data, but no index buffer is bound.", MessageSource.GL);

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferData(BufferTarget.ElementArrayBuffer, size, data, BufferUsage.StaticDraw);
                CheckError("after uploading index buffer data from pointer");
            });
        }

        /// <inheritdoc />
        public override void MapIndexBuffer(IntPtr data, uint size, uint offset = 0)
        {
            if (BoundIndexBuffer == 0) Engine.Log.Warning("You are trying to map data, but no index buffer is bound.", MessageSource.GL);
            IntPtr offsetPtr = (IntPtr) offset;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.BufferSubData(BufferTarget.ElementArrayBuffer, offsetPtr, size, data);
                CheckError("after uploading index buffer data from pointer");
            });
        }

        #endregion

        #region Shader API

        /// <inheritdoc />
        public override ShaderProgram CreateShaderProgram(string vert, string frag)
        {
            // Set defaults if missing.
            if (_defaultFragShader == null)
                _defaultFragShader = frag;
            if (_defaultVertShader == null)
                _defaultVertShader = vert;

            uint vertId = 0;
            uint fragId = 0;
            ShaderProgram newProgram = null;

            // Check for Adfectus flags.
            bool forceVersion = vert != null && vert.Contains("ADFECTUS_VERSION_FORCE");

            GLThread.ExecuteGLThread(() =>
            {
                // If forcing the version then don't attempt the version list.
                if (forceVersion)
                {
                    vertId = CompileShader(ShaderType.VertexShader, vert);
                    fragId = CompileShader(ShaderType.FragmentShader, frag);
                }
                else
                {
                    // Go through all versions compilation.
                    foreach (string version in ShaderVersionsList)
                    {
                        // Check if vert will be using default.
                        if (string.IsNullOrEmpty(vert)) vert = _defaultVertShader;

                        vertId = CompileShader(ShaderType.VertexShader, vert.Replace("#version v", $"#version {version}"));
                        // If the vert failed to compile, go to next fallback version.
                        if (vertId == 0) continue;

                        // Check if frag will be using default.
                        if (string.IsNullOrEmpty(frag)) frag = _defaultFragShader;

                        fragId = CompileShader(ShaderType.FragmentShader, frag.Replace("#version v", $"#version {version}"));
                        // If the frag compiled, then the vert has also compiled, so the entire shader is compiled.
                        if (fragId != 0) break;
                    }
                }


                // Set to default if compilation failed.
                if (vertId == 0 || fragId == 0) return;

                newProgram = new GlShaderProgram(vertId, fragId);

                // Check if the shader's linking failed.
                if (newProgram.Broken)
                {
                    newProgram = null;
                    return;
                }

                // Delete shaders.
                Gl.DeleteShader(fragId);
                Gl.DeleteShader(vertId);

                // Save current as not to creation to override the shader.
                ShaderProgram current = CurrentShader;

                // Set default uniforms.
                BindShaderProgram(newProgram);
                newProgram.SetUniformIntArray("textures", Enumerable.Range(0, 15).ToArray());
                newProgram.SetUniformFloat("time", 0);

                // Restore bound.
                BindShaderProgram(current);
            });

            return newProgram;
        }

        /// <inheritdoc />
        public override bool BindShaderProgram(ShaderProgram shaderProgram)
        {
            // Check if restoring default.
            if (shaderProgram == null) shaderProgram = _defaultProgram;

            if (CurrentShader == shaderProgram) return false;

            GLThread.ExecuteGLThread(() =>
            {
                Gl.UseProgram(shaderProgram?.Id ?? 0);
                CurrentShader = shaderProgram;
            });

            return true;
        }

        #endregion

        #region RenderTargets and Sampling

        /// <inheritdoc />
        public override RenderTarget CreateRenderTarget(Vector2 size, bool smooth = false, TextureInternalFormat internalFormat = TextureInternalFormat.Rgba,
            TexturePixelFormat pixelFormat = TexturePixelFormat.Rgba, bool attachStencil = false)
        {
            RenderTarget resultTarget = null;

            InternalFormat intFormat = (InternalFormat) Enum.Parse(typeof(InternalFormat), internalFormat.ToString());
            PixelFormat glFormat = (PixelFormat) Enum.Parse(typeof(PixelFormat), pixelFormat.ToString());

            GLThread.ExecuteGLThread(() =>
            {
                // Create the FBO which rendering will be done to.
                uint newFbo = Gl.GenFramebuffer();
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, newFbo);

                // Create the texture.
                uint renderTexture = Gl.GenTexture();
                Gl.BindTexture(TextureTarget.Texture2d, renderTexture);
                Gl.TexImage2D(TextureTarget.Texture2d, 0, intFormat, (int) size.X, (int) size.Y, 0, glFormat,
                    PixelType.UnsignedByte, IntPtr.Zero);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, smooth ? Gl.LINEAR : Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, smooth ? Gl.LINEAR : Gl.NEAREST);

                // Attach the texture to the frame buffer.
                Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, renderTexture, 0);

                // Attach color components.
                int[] modes = {Gl.COLOR_ATTACHMENT0};
                Gl.DrawBuffers(modes);

                // Create render buffer.
                uint depthBuffer = Gl.GenRenderbuffer();
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
                Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (int) size.X,
                    (int) size.Y);
                Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

                // Check status.
                FramebufferStatus status = Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferStatus.FramebufferComplete) Engine.Log.Warning($"Framebuffer creation failed. Error code {status}.", MessageSource.GL);

                // Create the texture object.
                Texture targetTexture = new GLTexture(renderTexture, new Vector2(size.X, size.Y), null, $"FBO {newFbo} Texture");

                // Create the render target object.
                resultTarget = new GlRenderTarget(newFbo, size, targetTexture);

                // Clear the target.
                ClearScreen();

                CheckError("creating scale fbo");

                // Restore bindings and so on.
                Engine.Renderer?.EnsureRenderTarget();
            });

            return resultTarget;
        }

        /// <inheritdoc />
        public override RenderTarget CreateMSAARenderTarget(int samples, Vector2 size, TextureInternalFormat internalFormat = TextureInternalFormat.Rgba, bool attachStencil = false)
        {
            RenderTarget resultTarget = null;

            InternalFormat intFormat = (InternalFormat) Enum.Parse(typeof(InternalFormat), internalFormat.ToString());

            GLThread.ExecuteGLThread(() =>
            {
                // Create the FBO.
                uint newFbo = Gl.GenFramebuffer();
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, newFbo);

                // Create the texture.
                uint renderTexture = Gl.GenTexture();
                Gl.BindTexture(TextureTarget.Texture2dMultisample, renderTexture);
                Gl.TexImage2DMultisample(TextureTarget.Texture2dMultisample, samples, intFormat, (int) size.X, (int) size.Y, true);

                // Attach the texture to the frame buffer.
                Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2dMultisample, renderTexture, 0);

                // Attach color components.
                int[] modes = {Gl.COLOR_ATTACHMENT0};
                Gl.DrawBuffers(modes);

                // Create render buffer.
                uint depthBuffer = Gl.GenRenderbuffer();
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
                Gl.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, InternalFormat.Depth24Stencil8, (int) size.X, (int) size.Y);
                Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

                // Check status.
                FramebufferStatus status = Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferStatus.FramebufferComplete) Engine.Log.Warning($"MSAA Framebuffer creation failed. Error code {status}.", MessageSource.GL);

                // Construct a texture object for it.
                Texture targetTexture = new GLTexture(renderTexture, new Vector2(size.X, size.Y), null, $"MSAA{samples} FBO Texture");

                // Create the render target object.
                resultTarget = new GlRenderTarget(newFbo, size, targetTexture);

                // Clear the target.
                ClearScreen();

                CheckError("creating msaa fbo");

                // Restore bindings and so on.
                Engine.Renderer?.EnsureRenderTarget();
            });

            return resultTarget;
        }

        /// <inheritdoc />
        public override void BindRenderTarget(RenderTarget t)
        {
            GLThread.ExecuteGLThread(() =>
            {
                // Check if restoring the window target.
                if (t == null)
                {
                    Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    Gl.Viewport((int) Viewport.X, (int) Viewport.Y, (int) Viewport.Width, (int) Viewport.Height);
                    return;
                }

                GlRenderTarget target = (GlRenderTarget) t;
                Gl.BindFramebuffer(FramebufferTarget.Framebuffer, target.Pointer);
                Gl.Viewport((int) target.Viewport.X, (int) target.Viewport.Y, (int) target.Viewport.Width, (int) target.Viewport.Height);
            });
        }

        /// <inheritdoc />
        public override void CopyRenderTarget(RenderTarget source, RenderTarget dest, Rectangle? sourceRect = null, Rectangle? destRect = null, bool smooth = false)
        {
            GLThread.ExecuteGLThread(() =>
            {
                Vector2 sourceSize = source?.Size ?? RenderSize;
                Vector2 destSize = dest?.Size ?? RenderSize;

                Rectangle srcRect = sourceRect ?? new Rectangle(0, 0, sourceSize);
                Rectangle dstRect = destRect ?? new Rectangle(0, 0, destSize);

                uint srcPointer = ((GlRenderTarget) source)?.Pointer ?? 0;
                uint destPointer = ((GlRenderTarget) dest)?.Pointer ?? 0;

                Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, srcPointer);
                Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, destPointer);
                Gl.BlitFramebuffer((int) srcRect.X, (int) srcRect.Y, (int) srcRect.Width, (int) srcRect.Height, (int) dstRect.X, (int) dstRect.Y, (int) dstRect.Width, (int) dstRect.Height,
                    ClearBufferMask.ColorBufferBit, smooth ? BlitFramebufferFilter.Linear : BlitFramebufferFilter.Nearest);

                // Restore bindings and so on.
                Engine.Renderer.EnsureRenderTarget();
            });
        }

        #endregion

        #region Stencil Testing

        /// <inheritdoc />
        public override void StateStencilTest(bool enable)
        {
            Engine.Renderer?.Submit();

            GLThread.ExecuteGLThread(() =>
            {
                if (enable)
                {
                    Gl.Enable(EnableCap.StencilTest);
                    Gl.StencilMask(0xFF);
                    Gl.Clear(ClearBufferMask.StencilBufferBit);
                    StencilModeDefault();
                }
                else
                {
                    Gl.Disable(EnableCap.StencilTest);
                    StencilModeDefault();
                }

                CheckError("setting stencil test");
            });
        }

        /// <inheritdoc />
        public override void StencilModeDefault()
        {
            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
            Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        }

        /// <inheritdoc />
        public override void StencilStartDraw(int value = 0xFF)
        {
            Engine.Renderer.Submit();

            Gl.StencilMask(0xFF);
            Gl.StencilFunc(StencilFunction.Always, value, 0xFF);
        }

        /// <inheritdoc />
        public override void StencilModeCutOutFrom(int threshold = 0xFF)
        {
            Engine.Renderer.Submit();

            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Greater, threshold, 0xFF);
        }

        /// <inheritdoc />
        public override void StencilModeMask(int filter = 0xFF)
        {
            Engine.Renderer.Submit();

            Gl.StencilMask(0x00);
            Gl.StencilFunc(StencilFunction.Less, filter, 0xFF);
        }

        /// <inheritdoc />
        public override void StencilModeCustom(uint mask, string mode = "Always", int funcRef = 1, uint modeMask = 0xFF)
        {
            Engine.Renderer.Submit();

            bool successful = Enum.TryParse(typeof(StencilFunction), mode, out object modeParsed);
            if (!successful)
            {
                Engine.Log.Warning("Invalid stencil mode - {mode}.", MessageSource.GL);
                return;
            }

            Gl.StencilMask(mask);
            Gl.StencilFunc((StencilFunction) modeParsed, funcRef, modeMask);
        }

        #endregion

        #region Helpers

        /// <inheritdoc />
        public override StreamBuffer CreateStreamBuffer(uint vbo, uint vao, uint ibo, uint objectSize, uint size, uint indicesPerObject, bool polygonMode = false)
        {
            StreamBuffer streamBuffer = null;

            GLThread.ExecuteGLThread(() =>
            {
                // Create a GL stream buffer.
                streamBuffer = new GlStreamBuffer(vbo, vao, ibo, objectSize, size * objectSize, indicesPerObject, polygonMode ? PrimitiveType.TriangleFan : PrimitiveType.Triangles);
            });

            return streamBuffer;
        }

        /// <summary>
        /// Convert a mask for SetTextureMask to its Gl const meaning.
        /// </summary>
        /// <param name="mask">The mask to convert</param>
        private int MaskMeaning(uint mask)
        {
            switch (mask)
            {
                case 0xff000000:
                    return Gl.RED;
                case 0x00ff0000:
                    return Gl.GREEN;
                case 0x0000ff00:
                    return Gl.BLUE;
                case 0x000000ff:
                    return Gl.ALPHA;
                case 0xffffffff:
                    return Gl.ONE;
                case 0x00000000:
                    return Gl.ZERO;
                default:
                    return Gl.ONE;
            }
        }

        /// <summary>
        /// Convert an internal data type to a native gl one.
        /// </summary>
        /// <returns></returns>
        private VertexAttribType ToGLPointerType(DataType emotionDataType)
        {
            bool parsed = Enum.TryParse(emotionDataType.ToString(), out VertexAttribType nativeDataType);
            if (!parsed) Engine.Log.Warning($"Couldn't parse data type - {emotionDataType}", MessageSource.GL);

            return nativeDataType;
        }

        /// <inheritdoc />
        public override uint GetBoundIndexBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => Gl.Get(Gl.ELEMENT_ARRAY_BUFFER_BINDING, out id));
            return (uint) id;
        }

        /// <inheritdoc />
        public override uint GetBoundDataBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => Gl.Get(Gl.ARRAY_BUFFER_BINDING, out id));
            return (uint) id;
        }

        /// <inheritdoc />
        public override uint GetBoundVertexArrayBuffer()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => Gl.Get(Gl.VERTEX_ARRAY_BINDING, out id));
            return (uint) id;
        }

        /// <inheritdoc />
        public override uint GetBoundRenderTarget()
        {
            int id = 0;
            GLThread.ExecuteGLThread(() => Gl.Get(Gl.DRAW_FRAMEBUFFER_BINDING, out id));
            return (uint) id;
        }

        /// <summary>
        /// Compiles a shader.
        /// </summary>
        /// <param name="type">The type of shader to compile.</param>
        /// <param name="source">The shader source.</param>
        /// <returns>The id of the compiled shader.</returns>
        private uint CompileShader(ShaderType type, string source)
        {
            // Create and compile the shader.
            uint shaderPointer = Gl.CreateShader(type);
            Gl.ShaderSource(shaderPointer, new[] {source});
            Gl.CompileShader(shaderPointer);

            CheckError($"shader compilation\n{source}");

            // Check if there's a log to print.
            Gl.GetShader(shaderPointer, ShaderParameterName.InfoLogLength, out int lLength);
            if (lLength > 0)
            {
                // Get the info log.
                StringBuilder compileStatusReader = new StringBuilder(lLength);
                Gl.GetShaderInfoLog(shaderPointer, lLength, out int _, compileStatusReader);
                string compileStatus = compileStatusReader.ToString();
                Engine.Log.Warning($"Compilation log for shader of type {type}: {compileStatus}.\nSource:{source}", MessageSource.GL);
            }

            // Check if the shader compiled successfully, if not return 0.
            Gl.GetShader(shaderPointer, ShaderParameterName.CompileStatus, out int status);
            return status == 1 ? shaderPointer : 0;
        }

        /// <summary>
        /// Get debugging information about the gl state.
        /// </summary>
        /// <returns>A string of all currently bound gl objects</returns>
        public string GetBoundDebugInfo()
        {
            Gl.Get(GetPName.ElementArrayBufferBinding, out int boundIbo);
            Gl.Get(GetPName.ArrayBufferBinding, out int boundVbo);
            Gl.Get(GetPName.VertexArrayBinding, out int boundVao);
            Gl.Get(GetPName.CurrentProgram, out int boundShader);
            Gl.GetBufferParameter(BufferTarget.ArrayBuffer, Gl.BUFFER_SIZE, out int boundVboSize);

            return $"IBO: {boundIbo}\r\nVBO: {boundVbo} [size:{boundVboSize}]\r\nVAO: {boundVao}\r\nShader: {boundShader}";
        }

        #endregion
    }
}