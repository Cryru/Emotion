using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.Graphics.Batching;
using Emotion.Graphics.Objects;
using Emotion.Libraries;
using OpenTK.Graphics.ES30;

namespace Emotion.Graphics
{
    /// <summary>
    /// Manages OpenGL state and such.
    /// </summary>
    public static class GraphicsManager
    {
        /// <summary>
        /// Setups the graphic's context and defaults.
        /// </summary>
        public static void Setup()
        {
            // Check for minimum version.
            if (Context.Flags.RenderFlags.OpenGLMajorVersion < 3 || Context.Flags.RenderFlags.OpenGLMinorVersion < 3) Context.Log.Error("Minimum OpenGL version is 3.3", MessageSource.Renderer);

            // Setup thread manager.
            GLThread.BindThread();

            // Renderer bootstrap.
            Context.Log.Info("Loading Emotion OpenTK-GLES Renderer...", MessageSource.Renderer);
            Context.Log.Info($"GL: {GL.GetString(StringName.Version)} on {GL.GetString(StringName.Renderer)}", MessageSource.Renderer);
            Context.Log.Info($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}", MessageSource.Renderer);

            // Set execution flags, used for abstracting different GPU behavior.
            SetFlags();

            // Create default shaders. This also sets some shader flags.
            CreateDefaultShaders();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram((Shader)null, null);
            defaultProgram.Bind();

            // Check if the setup encountered any errors.
            GLThread.CheckError("renderer setup");

            // Setup additional GL arguments.
            GL.Enable(EnableCap.Blend);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
    }
}
