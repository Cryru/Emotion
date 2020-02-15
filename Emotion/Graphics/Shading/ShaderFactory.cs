#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
            {"CompatTextureIndex", s => ApplyIndexUnwrap(AddPreprocessorConstant(s, "CompatTextureIndex"))},
            {"AttribLocationExtension", s => AddExtensionConstant(s, "GL_ARB_explicit_attrib_location")},
            {"CompatTextureIndex&AttribLocationExtension", s => ApplyIndexUnwrap(AddExtensionConstant(AddPreprocessorConstant(s, "CompatTextureIndex"), "GL_ARB_explicit_attrib_location"))}
        };

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

            if (Engine.Configuration.DebugMode) WarningsCheck(preprocessed);

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
        private static string[] Preprocess(IEnumerable<string> source)
        {
            var code = new List<string>(source)
            {
                [0] = $"#version {Gl.CurrentShadingVersion.VersionId}\n"
            };

            // Version string is required.

            // Defines
            code.Insert(1, $"#define TEXTURE_COUNT {Engine.Renderer.TextureArrayLimit}\n");

            // GLES support
            code.Insert(2, "#ifdef GL_ES\n");
            code.Insert(3, "precision highp float;\n");
            code.Insert(4, "#endif\n");

            for (var i = 4; i < code.Count; i++)
            {
                // Legacy texture length definition.
                if (code[i].Trim() == "uniform sampler2D textures[16];") code[i] = "uniform sampler2D textures[TEXTURE_COUNT];";

                // Resolve file dependencies.
                // This will break with circular dependencies and co-dependencies - sooo don't do that.
                if (code[i].StartsWith("#using \""))
                {
                    string line = code[i].Trim();
                    string file = line.Replace("#using \"", "");
                    file = file.Substring(0, file.Length - 1);

                    code[i] = ResolveShaderDependency(file);
                }

                // Old using
                if (code[i].StartsWith("//GetTextureColor")) code[i] = ResolveShaderDependency("Shaders/GetTextureColor.c");
                if (code[i].StartsWith("//GetTextureSize")) code[i] = ResolveShaderDependency("Shaders/GetTextureSize.c");

                code[i] = code[i].Trim() + "\n";
            }

            return code.ToArray();
        }

        /// <summary>
        /// Resolves file dependencies in shaders.
        /// </summary>
        private static string ResolveShaderDependency(string file)
        {
            var loadedFile = Engine.AssetLoader.Get<TextAsset>(file);
            if (loadedFile == null) return "";
            return Helpers.NormalizeNewLines(loadedFile.Content);
        }

        /// <summary>
        /// Used in Debug Mode to generate more telling warnings from a preprocessed shader source.
        /// </summary>
        /// <param name="source">The shader source.</param>
        private static void WarningsCheck(IReadOnlyList<string> source)
        {
            for (var i = 0; i < source.Count; i++)
            {
                string line = source[i].Trim();

                // Check if version exists.
                if (i == 0 && source[i][0] != '#') Engine.Log.Warning($"The first character is the shader is not '#' but is {source[i][0]}.", MessageSource.Debug);

                if (i == 0 && !line.Contains("#version ")) Engine.Log.Warning("The shader is missing the version tag.", MessageSource.Debug);

                // Legacy warnings.
                if (line.Contains("float Tid"))
                    Engine.Log.Warning("The shader defines the 'Tid' uniform as float, which is how it used to be. Newer versions of Emotion expect it to be a 'flat in int'.", MessageSource.Debug);

                if (line.Contains("//GetTextureColor"))
                    Engine.Log.Warning("You are using an old 'using' statement to import Shaders/GetTextureColor.c please use `#using \"Shaders/getTextureColor.c\"` instead.", MessageSource.Debug);

                if (line.Contains("//GetTextureSize"))
                    Engine.Log.Warning("You are using an old 'using' statement to import Shaders/GetTextureSize.c please use `#using \"Shaders/getTextureSize.c\"` instead.", MessageSource.Debug);

                if (line.Contains("uniform sampler2D textures[16];"))
                    Engine.Log.Warning("You are declaring your textures array as a static size of 16 - please use `uniform sampler2D textures[TEXTURE_COUNT];` instead.", MessageSource.Debug);
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

        #region Index Unwrapping CodeGen

        private static Regex _indexUnwrapRegex = new Regex("(\\/\\/TEXTURE_INDEX_UNWRAP)([\\s\\S]*?)(\\/\\/END)", RegexOptions.Multiline);

        /// <summary>
        /// Horrible code-gen.
        /// Used to generate indexing unwraps.
        /// </summary>
        private static string[] ApplyIndexUnwrap(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                Match match = _indexUnwrapRegex.Match(lines[i]);
                while (match.Success && match.Groups.Count == 4)
                {
                    Group code = match.Groups[2];
                    string unwrapped = UnwrapIndex(Engine.Renderer.TextureArrayLimit, code.Value.Trim());
                    lines[i] = lines[i].Substring(0, match.Index) + unwrapped + lines[i].Substring(match.Index + match.Length);
                    match = match.NextMatch();
                }
            }

            return lines;
        }

        /// <summary>
        /// Old OpenGL doesn't support indexing arrays.
        /// This means that we must unwrap them into if checks.
        /// </summary>
        private static string UnwrapIndex(int length, string code)
        {
            var unwrapped = new List<string>();
            for (var i = 0; i < length; i++)
            {
                unwrapped.Add(i == 0 ? $"if(value == {i})" : $"else if(value == {i})");

                unwrapped.Add("{");
                unwrapped.Add(code.Replace("value", $"{i}"));
                unwrapped.Add("}");
            }

            return string.Join("\n", unwrapped);
        }

        #endregion
    }
}