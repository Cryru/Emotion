#region Using

using System;
using System.Collections.Generic;
using System.Text;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Shading
{
    public static class ShaderFactory
    {
        /// <summary>
        /// The default shader.
        /// </summary>
        public static ShaderProgram DefaultProgram { get; private set; }

        /// <summary>
        /// List of shader configurations to try to compile with.
        /// </summary>
        private static Dictionary<string, Func<string[], string[]>> _shaderConfigurations = new Dictionary<string, Func<string[], string[]>>
        {
            {"default", s => s},
            {"CompatTextureColor", s => AddPreprocessorConstant(s, "CompatTextureColor")},
            {"AttribLocationExtension", s => AddExtensionConstant(s, "GL_ARB_explicit_attrib_location")},
            {"CompatTextureColor&AttribLocationExtension", s => AddExtensionConstant(AddPreprocessorConstant(s, "CompatTextureColor"), "GL_ARB_explicit_attrib_location")}
        };

        /// <summary>
        /// Functions to be put in the shader.
        /// </summary>
        private static Dictionary<string, TextAsset> _shaderParts = new Dictionary<string, TextAsset>();

        static ShaderFactory()
        {
            // Make sure the asset loader is setup.
            if (Engine.AssetLoader == null) return;

            var getTextureColor = Engine.AssetLoader.Get<TextAsset>("Shaders/GetTextureColor.c");
            if (getTextureColor == null)
            {
                Engine.SubmitError(new Exception("Couldn't load default shader parts - GetTextureColor."));
                return;
            }

            _shaderParts.Add("GetTextureColor", getTextureColor);

            var getTextureSize = Engine.AssetLoader.Get<TextAsset>("Shaders/GetTextureSize.c");
            if (getTextureSize == null)
            {
                Engine.SubmitError(new Exception("Couldn't load default shader parts - GetTextureSize."));
                return;
            }

            _shaderParts.Add("GetTextureSize", getTextureSize);
        }

        /// <summary>
        /// Add a custom shader part to the shader preprocessor.
        /// Instances of "//key" will be replaced by the content of the text asset.
        /// </summary>
        /// <param name="key">The key to replace.</param>
        /// <param name="text">The text to replace it with.</param>
        public static void AddShaderPart(string key, TextAsset text)
        {
            _shaderParts.Add(key, text);
        }

        /// <summary>
        /// Create the default shader.
        /// </summary>
        /// <returns>The default shader, will not recreate it if it already exists.</returns>
        public static ShaderProgram CreateDefaultShader()
        {
            if (DefaultProgram != null) return DefaultProgram;

            var vert = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultVert.vert");
            var frag = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultFrag.frag");

            DefaultProgram = CreateShader(vert.Content, frag.Content);
            return DefaultProgram;
        }

        /// <summary>
        /// Create a shader.
        /// </summary>
        /// <param name="vertShaderSource">The source code of the vertex shader.</param>
        /// <param name="fragShaderSource">The source code of the fragment shader.</param>
        /// <returns>A compiled and linked shader program.</returns>
        public static ShaderProgram CreateShader(string vertShaderSource, string fragShaderSource)
        {
            uint vertShader = Gl.CreateShader(ShaderType.VertexShader);
            uint fragShader = Gl.CreateShader(ShaderType.FragmentShader);

            Engine.Log.Info("Compiling vert shader...", MessageSource.Renderer);
            bool vertCompiled = TryCompile(vertShaderSource, vertShader, out string configVert);
            Engine.Log.Info("Compiling frag shader...", MessageSource.Renderer);
            bool fragCompiled = TryCompile(fragShaderSource, fragShader, out string configFrag);

            // Try to compile with shader configurations.
            if (!vertCompiled || !fragCompiled)
            {
                if (DefaultProgram == null)
                {
                    Engine.Log.Warning("Couldn't compile default shader.", MessageSource.Renderer);
                    return null;
                }

                Engine.Log.Warning("Couldn't compile shader.", MessageSource.Renderer);
                return null;
            }

            // Link into a program.
            var newShader = new ShaderProgram(vertShader, fragShader)
            {
                CompiledConfig = $"v:{configVert}, f:{configFrag}"
            };

            // Add source if in debug mode.
            if (Engine.Configuration.DebugMode)
            {
                newShader.DebugFragSource = fragShaderSource;
                newShader.DebugVertSource = vertShaderSource;
            }

            // Check if the program is valid.
            if (newShader.Valid)
            {
                Engine.Log.Info($"Compiled and linked shader, with config - {newShader.CompiledConfig}.", MessageSource.Renderer);
                return newShader;
            }

            if (DefaultProgram == null)
            {
                Engine.Log.Warning("Couldn't link default shader program.", MessageSource.Renderer);
                return null;
            }

            Engine.Log.Warning("Couldn't link shader program - falling back to default.", MessageSource.Renderer);
            return DefaultProgram;
        }

        /// <summary>
        /// Attempt to compile the shader with all configurations.
        /// </summary>
        /// <param name="source">The shader source.</param>
        /// <param name="shaderId">The OpenGL object id for the shader.</param>
        /// <param name="config">The configuration the shader successfully compiled with.</param>
        /// <returns>Whether the shader compiled successfully.</returns>
        private static bool TryCompile(string source, uint shaderId, out string config)
        {
            // Add two new lines (for the define and for the version tag), normalize new lines, and split by line.
            string[] preprocessed = Helpers.NormalizeNewLines(source).Split("\n");

            // Add version tag and preprocess the source.
            preprocessed = Preprocess(preprocessed);

            if (Engine.Configuration.DebugMode)
            {
                WarningsCheck(preprocessed);
            }

            // Find a configuration which will compile the shader.
            foreach (KeyValuePair<string, Func<string[], string[]>> configuration in _shaderConfigurations)
            {
                Engine.Log.Trace($"Attempting shader compilation with config - {configuration.Key}...", MessageSource.Renderer);

                // Apply configuration.
                var attempt = new string[preprocessed.Length];
                Array.Copy(preprocessed, 0, attempt, 0, preprocessed.Length);
                attempt = configuration.Value(attempt);

                Gl.ShaderSource(shaderId, attempt);
                Gl.CompileShader(shaderId);

                // Check if there's a log to print.
                Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int lLength);
                if (lLength > 0)
                {
                    // Get the info log.
                    var compileStatusReader = new StringBuilder(lLength);
                    Gl.GetShaderInfoLog(shaderId, lLength, out int _, compileStatusReader);
                    string compileStatus = compileStatusReader.ToString();
                    Engine.Log.Warning($"Couldn't compile shader: {compileStatus}", MessageSource.GL);
                    Engine.Log.Trace(string.Join("", preprocessed), MessageSource.ShaderSource);
                }

                // Check if the shader compiled successfully, if not 0 is returned.
                Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int status);
                if (status != 1) continue;
                config = configuration.Key;
                return true;
            }

            config = "invalid";
            return false;
        }

        /// <summary>
        /// Inserts the version string, removes any user inputted version strings.
        /// Injects default functions.
        /// Trims all lines and inserts new lines.
        /// </summary>
        /// <param name="source">The shader source code.</param>
        /// <returns>The preprocessed shader code.</returns>
        private static string[] Preprocess(string[] source)
        {
            source[0] = $"#version {Gl.CurrentShadingVersion.VersionId}\n";

            for (var i = 1; i < source.Length; i++)
            {
                // Remove user's version tag.
                if (source[i].Contains("#version")) source[i] = "\n";

                // Inject texture parts.
                foreach (var part in _shaderParts)
                {
                    if (source[i].Contains($"//{part.Key}")) source[i] = Helpers.NormalizeNewLines(part.Value.Content);
                }

                source[i] = source[i].Trim() + "\n";
            }

            return source;
        }

        /// <summary>
        /// Used in Debug Mode to generate more telling warnings from a preprocessed shader source.
        /// </summary>
        /// <param name="source">The shader source.</param>
        private static void WarningsCheck(string[] source)
        {
            for (var i = 0; i < source.Length; i++)
            {
                // Check if version exists.
                if (source[i][0] != '#')
                {
                    Engine.Log.Warning($"The first character is the shader is not '#' but is {source[i][0]}.", MessageSource.Debug);
                }

                if (!source[i].Contains("#version "))
                {
                    Engine.Log.Warning($"The shader is missing the version tag.", MessageSource.Debug);
                }

                // Legacy warnings.
                if (source[i].Contains("float Tid"))
                {
                    Engine.Log.Warning($"The shader defines the 'Tid' uniform as float, which is how it used to be. Newer versions of Emotion expect it to be a 'flat in int'.", MessageSource.Debug);
                }
            }
        }

        #region Shader Configurations

        private static string[] AddPreprocessorConstant(string[] source, string constant)
        {
            source[0] = $"{source[0]}#define {constant} 1\n";
            return source;
        }

        private static string[] AddExtensionConstant(string[] source, string extension)
        {
            source[0] = $"{source[0]}#extension {extension} : require\n";
            return source;
        }

        #endregion
    }
}