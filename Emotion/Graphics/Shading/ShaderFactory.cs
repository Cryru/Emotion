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
            ("default", ExcludeCompliantRenderer),
            ("shader5", s => ExcludeCompliantRenderer(AddExtensionConstant(s, "GL_ARB_gpu_shader5"))),
            ("CompatTextureIndex", s => ApplyIndexUnwrap(AddPreprocessorConstant(s, "CompatTextureIndex"))),
            ("AttribLocationExtension", s => AddExtensionConstant(s, "GL_ARB_explicit_attrib_location")),
            ("CompatTextureIndex&AttribLocationExtension", s => ApplyIndexUnwrap(AddExtensionConstant(AddPreprocessorConstant(s, "CompatTextureIndex"), "GL_ARB_explicit_attrib_location")))
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
            bool vertCompiled = TryCompile(vertShaderSource, vertShader, out string configVert);
            if (!vertCompiled) Engine.Log.Warning("Vert shader compilation failed.", MessageSource.Renderer);

            uint fragShader = Gl.CreateShader(ShaderType.FragmentShader);
            bool fragCompiled = TryCompile(fragShaderSource, fragShader, out string configFrag);
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
                    string compileStatus = compileStatusReader.ToString();
                    Engine.Log.Warning($"Couldn't compile shader: {compileStatus}", MessageSource.GL);
                    Engine.Log.Trace(string.Join("", preprocessed), MessageSource.ShaderSource);
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
        /// Let me tell you a really sad story.
        /// Indexing sampler (and other opaque types) arrays with a varying is not allowed by the GL spec.
        /// Since version 400 (and with the shader5 extension) you can use dynamic uniform expressions to index,
        /// which basically means through a uniform a variable. The presumption is that the value will be the same for all
        /// executions of the
        /// shader.
        /// There are various workarounds to this, such as looping over the array and stopping to index it with the loop counter
        /// once it has reached the varying value, or creating a large switch case (as the constant "CompatTextureIndex" does).
        /// Theoretically this is still illegal and is a huge performance hit (as branching usually is in shaders).
        /// HOWEVER, most drivers will actually let you index the array directly and it is assumed that those who stick to the
        /// spec,
        /// and don't let you do that, will throw an error during shader compilation. The whole shader configuration logic here
        /// exists to try out different variations of the shader until one compiles.
        /// It turns out some drivers (AMD) compile silently without any errors. While they are technically up to spec, reporting
        /// success in such cases is not.
        /// One way (I've randomly found) of detecting this is to attempt a compilation of a GLES shader, in which case they do
        /// throw an error
        /// This function will attempt to compile such a shader once, and exclude non-compliant shader configurations if it fails.
        /// </summary>
        /// <param name="source">The shader source.</param>
        /// <returns>null if the shader configuration should be skipped, and the source if it shouldn't.</returns>
        private static string[] ExcludeCompliantRenderer(string[] source)
        {
            if (_compliantShader != null) return _compliantShader.Value ? null : source;

            uint testShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(testShader, _opaqueTypeIndexCheckShader);
            Gl.CompileShader(testShader);
            Gl.GetShader(testShader, ShaderParameterName.CompileStatus, out int status);

            Gl.GetShader(testShader, ShaderParameterName.InfoLogLength, out int lLength);
            if (lLength > 0)
            {
                // Get the info log.
                var compileStatusReader = new StringBuilder(lLength);
                Gl.GetShaderInfoLog(testShader, lLength, out int _, compileStatusReader);
                string compileStatus = compileStatusReader.ToString();
                Engine.Log.Info($"Compliant renderer compile status {status}, and reported: {compileStatus}", MessageSource.Renderer);
            }

            Gl.DeleteShader(testShader);
            _compliantShader = status == 0;
            return _compliantShader.Value ? null : source;
        }

        private static bool? _compliantShader;

        private static string[] _opaqueTypeIndexCheckShader =
        {
            "#version 300 es\n",
            "#ifdef GL_ES\n",
            "precision highp float;\n",
            "#endif\n",
            $"uniform sampler2D textures[{Engine.Renderer.TextureArrayLimit}];\n",
            "flat in int Tid;\n",
            "out vec4 fragColor;\n",
            "void main() {\n",
            " fragColor = texture(textures[Tid], vec2(0.0));\n",
            "}\n"
        };

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