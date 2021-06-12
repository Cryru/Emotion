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
        private static (string name, Func<string[], string[]> func)[] _shaderConfigurations =
        {
            ("default", s => s),
            ("AttribLocationExtension", s => ExcludeEs(AddExtensionConstant(s, "GL_ARB_explicit_attrib_location"))),
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

            Engine.Log.Info("Creating default shader...", MessageSource.Renderer);
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
            var uniformDefaults = new List<ShaderUniform>();

            uint vertShader = Gl.CreateShader(ShaderType.VertexShader);
            bool vertCompiled = TryCompile(vertShaderSource, vertShader, out string configVert, uniformDefaults);
            if (!vertCompiled) Engine.Log.Warning("Vert shader compilation failed.", MessageSource.Renderer);

            uint fragShader = Gl.CreateShader(ShaderType.FragmentShader);
            bool fragCompiled = TryCompile(fragShaderSource, fragShader, out string configFrag, uniformDefaults);
            if (!fragCompiled) Engine.Log.Warning("Frag shader compilation failed.", MessageSource.Renderer);

            // Try to compile with shader configurations.
            if (!vertCompiled || !fragCompiled)
            {
                if (DefaultProgram != null) return null;
                Engine.Log.Warning("Couldn't compile default shader.", MessageSource.Renderer);
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
                // Apply shader defaults.
                if (uniformDefaults.Count > 0)
                {
                    ShaderProgram.EnsureBound(newShader.Pointer);
                    for (var i = 0; i < uniformDefaults.Count; i++)
                    {
                        ShaderUniform uniDefault = uniformDefaults[i];
                        uniDefault.ApplySelf(newShader);
                    }
                }

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
        /// <param name="uniformDefaults">List to fill with default uniform values.</param>
        /// <returns>Whether the shader compiled successfully.</returns>
        private static bool TryCompile(string source, uint shaderId, out string config, List<ShaderUniform> uniformDefaults = null)
        {
            // Add two new lines (for the define and for the version tag), normalize new lines, and split by line.
            string[] preprocessed = Helpers.NormalizeNewLines(source).Split("\n");

            // Add version tag and preprocess the source.
            preprocessed = Preprocess(preprocessed, uniformDefaults);

            if (Engine.Configuration.DebugMode) WarningsCheck(preprocessed);

            // Find a configuration which will compile the shader.
            for (var i = 0; i < _shaderConfigurations.Length; i++)
            {
                (string name, Func<string[], string[]> func) configuration = _shaderConfigurations[i];

                // Apply configuration.
                var attempt = new string[preprocessed.Length];
                Array.Copy(preprocessed, 0, attempt, 0, preprocessed.Length);
                attempt = configuration.func(attempt);
                if (attempt == null)
                {
                    Engine.Log.Trace($"Config {configuration.name} skipped.", MessageSource.Renderer);
                    continue;
                }

                Gl.ShaderSource(shaderId, attempt);
                Gl.CompileShader(shaderId);

                // Check if there's a log to print.
                Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int lLength);
                if (lLength > 0)
                {
                    // Get the info log.
                    var compileStatusReader = new StringBuilder(lLength);
                    Gl.GetShaderInfoLog(shaderId, lLength, out int _, compileStatusReader);
                    var compileStatus = compileStatusReader.ToString();
                    Engine.Log.Warning($"Couldn't compile shader: {compileStatus}", MessageSource.GL);
                    Engine.Log.Trace(string.Join("", attempt), MessageSource.ShaderSource);
                }

                // Check if the shader compiled successfully, if not 0 is returned.
                Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int status);
                if (status != 1)
                {
                    Engine.Log.Trace($"Shader compilation with config - {configuration.name} failed.", MessageSource.Renderer);
                    continue;
                }

                config = configuration.name;

                // Swap the first configuration with the one which worked. This will speed up future compilations
                // as a known working one will be tried first. You generally don't want to have a configuration which
                // only works on some of your shaders, but rather polyfill for different hardware support.
                _shaderConfigurations.ArraySwap(i, 0);
                return true;
            }

            config = "invalid";
            return false;
        }

        private static Regex _uniformDefaultsDetect = new Regex("uniform ([\\S| ]+?) ([\\S| ]+?)(=)([\\S| ]+?)(;)");

        /// <summary>
        /// Inserts the version string, removes any user inputted version strings.
        /// Injects default functions.
        /// Trims all lines and inserts new lines.
        /// </summary>
        /// <param name="source">The shader source code.</param>
        /// <param name="uniformDefaults">The list to fill with found shader defaults when running under GLES</param>
        /// <returns>The preprocessed shader code.</returns>
        private static string[] Preprocess(IEnumerable<string> source, List<ShaderUniform> uniformDefaults = null)
        {
            bool es = Gl.CurrentShadingVersion.GLES;
            var code = new List<string>(source)
            {
                // Version string is required.
                [0] = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}\n"
            };

            // GLES support
            code.Insert(2, "#ifdef GL_ES\n");
            code.Insert(3, "precision highp float;\n");
            code.Insert(4, "#endif\n");
            code.Insert(5, "#line 2\n");

            var dependencyIdx = 1;
            for (var i = 6; i < code.Count; i++)
            {
                // Legacy texture uniform definitions.
                if (code[i].Trim() == "uniform sampler2D textures[16];") code[i] = "uniform sampler2D textures[TEXTURE_COUNT];";
                if (code[i].Trim() == "uniform sampler2D textures[TEXTURE_COUNT];") code[i] = "uniform sampler2D mainTexture;";

                // Uniform defaults are not allowed in ES.
                // Find them and extract them.
                if (es)
                {
                    Match match = _uniformDefaultsDetect.Match(code[i]);
                    while (match.Success && match.Groups.Count > 4)
                    {
                        string uniformType = match.Groups[1].Value.Trim();
                        string uniformName = match.Groups[2].Value.Trim();
                        string value = match.Groups[4].Value.Trim();
                        code[i] = _uniformDefaultsDetect.Replace(code[i], $"uniform {uniformType} {uniformName};", 1);
                        match = _uniformDefaultsDetect.Match(code[i]);
                        uniformDefaults?.Add(new ShaderUniform(uniformName, uniformType, value));
                    }
                }

                // Resolve file dependencies.
                // This will break with circular dependencies and co-dependencies - sooo don't do that.
                var codeAdded = false;
                if (code[i].StartsWith("#using \""))
                {
                    string line = code[i].Trim();
                    string file = line.Replace("#using \"", "");
                    file = file[..^1];

                    code[i] = $"#line 0 {dependencyIdx} \n// {file}\n{ResolveShaderDependency(file)}\n// End";
                    codeAdded = true;
                    dependencyIdx++;
                }

                // Old using
                if (code[i].StartsWith("//GetTextureColor"))
                {
                    code[i] = $"#line 0 {dependencyIdx} // Legacy Import GetTextureColor\n{ResolveShaderDependency("Shaders/GetTextureColor.c")}\n// End";
                    codeAdded = true;
                }
                else if (code[i].StartsWith("//GetTextureSize"))
                {
                    code[i] = $"#line 0 {dependencyIdx} // Legacy Import GetTextureSize\n{ResolveShaderDependency("Shaders/GetTextureSize.c")}\n// End";
                    codeAdded = true;
                }

                // Ensure shader errors are reported for the correct lines.
                if (codeAdded) code[i] += $"\n#line {i - 4} 0";

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
            return loadedFile == null ? string.Empty : Helpers.NormalizeNewLines(loadedFile.Content);
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

                if (line.Contains("uniform sampler2D textures[TEXTURE_COUNT];"))
                    Engine.Log.Warning("You are declaring your main texture as an array - please use `uniform sampler2D mainTexture;` instead.", MessageSource.Debug);
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

        /// <summary>
        /// Exclude shader configuration on OpenGL ES.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static string[] ExcludeEs(string[] source)
        {
            if (source == null || Gl.CurrentVersion.GLES) return null;
            return source;
        }

        #endregion
    }
}