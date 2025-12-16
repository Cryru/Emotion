#nullable enable

#region Using

using System.Text;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Shader;
using OpenGL;

#endregion

namespace Emotion.Graphics.Shading;

public static class ShaderFactory
{
    /// <summary>
    /// List of shader configurations to try to compile with.
    /// </summary>
    private static (string name, Func<string[], string[]> func)[] _shaderConfigurations =
    {
        ("default", DefaultCompilation),
        ("AttribLocationExtension", ExplicitAttribCompilation),
    };

    /// <summary>
    /// Create a shader.
    /// </summary>
    /// <param name="vertShaderSource">The source code of the vertex shader.</param>
    /// <param name="fragShaderSource">The source code of the fragment shader.</param>
    /// <param name="compileConstant">Additional shader compilation constants</param>
    /// <returns>A compiled and linked shader program.</returns>
    public static ShaderProgram? CreateShader(string vertShaderSource, string fragShaderSource, string? compileConstant = null)
    {
        List<ShaderUniform> uniformDefaults = null;
        if (Gl.CurrentShadingVersion.GLES) uniformDefaults = new List<ShaderUniform>();

        // Normalize new lines, and split into line array.
        string[] preprocessedVert = Helpers.NormalizeNewLines(vertShaderSource).Split("\n");
        // Preprocess the shader, adding version and other supporting code.
        preprocessedVert = Preprocess(preprocessedVert, uniformDefaults, compileConstant);
        if (Engine.Configuration.DebugMode) WarningsCheck(preprocessedVert);

        uint vertShader = Gl.CreateShader(ShaderType.VertexShader);
        bool vertCompiled = TryCompile(preprocessedVert, vertShader);
        if (!vertCompiled) Engine.Log.Warning("Vert shader compilation failed.", MessageSource.Renderer);

        // Repeat for fragment shader.
        string[] preprocessedFrag = Helpers.NormalizeNewLines(fragShaderSource).Split("\n");
        preprocessedFrag = Preprocess(preprocessedFrag, uniformDefaults, compileConstant);
        if (Engine.Configuration.DebugMode) WarningsCheck(preprocessedFrag);

        uint fragShader = Gl.CreateShader(ShaderType.FragmentShader);
        bool fragCompiled = TryCompile(preprocessedFrag, fragShader);
        if (!fragCompiled) Engine.Log.Warning("Frag shader compilation failed.", MessageSource.Renderer);

        // Try to compile with shader configurations.
        if (!vertCompiled || !fragCompiled)
        {
            Gl.DeleteShader(vertShader);
            Gl.DeleteShader(fragShader);
            return null;
        }

        // Link into a program and add meta data in debug mode.
        var newShader = ShaderProgram.CreateFromShaders(vertShader, fragShader);
        newShader.AllowTextureBatch = fragShaderSource.Contains("ALLOW_TEXTURE_BATCHING");
        if (Engine.Configuration.DebugMode)
        {
            newShader.DebugFragSource = fragShaderSource;
            newShader.DebugVertSource = vertShaderSource;
        }

        // Once the shaders are compiled into a program it is save to clean them up.
        // One might think that it's a good idea to reuse them, especially those
        // which can be used as fallbacks, but some drivers prevent them
        // from being reused once compiled.
        Gl.DeleteShader(vertShader);
        Gl.DeleteShader(fragShader);

        // Check if the program compiled successfully.
        if (!newShader.Valid) return null;

        // Apply shader default uniforms. This requires binding, so save last binding.
        uint previouslyBound = ShaderProgram.Bound;
        ShaderProgram.EnsureBound(newShader.Pointer);
        newShader.SetUniformInt("mainTexture", 0);
        newShader.SetUniformFloat("iTime", 0);

        // Apply shader defaults that were found inlined in the code.
        // This is needed for GLES as it doesn't supported inlined default values.
        if (uniformDefaults != null)
            for (var i = 0; i < uniformDefaults.Count; i++)
            {
                ShaderUniform uniDefault = uniformDefaults[i];
                uniDefault.ApplySelf(newShader);
            }

        ShaderProgram.EnsureBound(previouslyBound);
        return newShader;
    }

    /// <summary>
    /// Attempt to compile the shader with all configurations.
    /// </summary>
    /// <param name="preProcSrc">The shader source.</param>
    /// <param name="shaderId">The OpenGL object id for the shader.</param>
    /// <returns>Whether the shader compiled successfully.</returns>
    private static bool TryCompile(string[] preProcSrc, uint shaderId)
    {
        // Find a configuration which will compile the shader.
        for (var i = 0; i < _shaderConfigurations.Length; i++)
        {
            (string name, Func<string[], string[]> func) configuration = _shaderConfigurations[i];

            // Apply configuration.
            // Note: This will modify the preprocessed source.
            if (configuration.func(preProcSrc) == null)
            {
                Engine.Log.Trace($"Config {configuration.name} skipped.", MessageSource.Renderer);
                continue;
            }

            int[] lengths = new int[preProcSrc.Length];
            for (int l = 0; l < preProcSrc.Length; l++)
            {
                lengths[l] = preProcSrc[l].Length;
            }

            Gl.ShaderSource(shaderId, preProcSrc, lengths);
            Gl.CompileShader(shaderId);

            // Check if there's a log to print.
            Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int lLength);
            if (lLength > 1) // 0 without null termination
            {
                // Get the info log.
                var compileStatusReader = new StringBuilder(lLength);
                Gl.GetShaderInfoLog(shaderId, lLength, out int _, compileStatusReader);
                var compileStatus = compileStatusReader.ToString();
                Engine.Log.Warning($"Couldn't compile shader: {compileStatus}", MessageSource.GL);
                Engine.Log.Trace(string.Join("", preProcSrc), MessageSource.ShaderSource);
            }

            // Check if the shader compiled successfully, if not 0 is returned.
            Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int status);
            if (status != 1)
            {
                Engine.Log.Trace($"Shader compilation with config - {configuration.name} failed.", MessageSource.Renderer);
                continue;
            }

            // Swap the first configuration with the one which worked. This will speed up future compilations
            // as a known working one will be tried first. You generally don't want to have a configuration which
            // only works on some of your shaders, but rather polyfill for different hardware support.
            _shaderConfigurations.ArraySwap(i, 0);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Inserts the version string, removes any user inputted version strings.
    /// Injects default functions.
    /// Trims all lines and inserts new lines.
    /// </summary>
    /// <param name="source">The shader source code.</param>
    /// <param name="uniformDefaults">The list to fill with found shader defaults when running under GLES</param>
    /// <param name="compileConstant">Compilation constant to add.</param>
    /// <returns>The preprocessed shader code.</returns>
    private static string[] Preprocess(string[] source, List<ShaderUniform>? uniformDefaults = null, string? compileConstant = null)
    {
        bool es = Gl.CurrentShadingVersion.GLES;
        var code = new List<string>(source);
        if (code.Count == 0 || string.IsNullOrEmpty(code[0])) return Array.Empty<string>();

        // Version string is required to be first.
        code[0] = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}";

        // GLES support
        code.Insert(1, "#using \"Shaders/GLESSupport.c\"");

        // Add user defined compilation constant.
        if (compileConstant != null) code.Insert(1, $"#define {compileConstant} 1");

        Span<Range> uniformDeclLeftAndRight = stackalloc Range[2];
        Span<Range> uniformDeclTokens = stackalloc Range[4];

        var dependencyIdx = 1;
        for (var i = 0; i < code.Count; i++)
        {
            // Legacy texture uniform definitions.
            if (code[i].Trim() == "uniform sampler2D textures[16];") code[i] = "uniform sampler2D textures[TEXTURE_COUNT];";
            if (code[i].Trim() == "uniform sampler2D textures[TEXTURE_COUNT];") code[i] = "uniform sampler2D mainTexture;";

            // Uniform defaults are not allowed in ES.
            // Find them and extract them.
            if (uniformDefaults != null)
            {
                var lineofCode = code[i].AsSpan();
                bool isUniformDecl = lineofCode.Contains("uniform", StringComparison.InvariantCulture);
                if (isUniformDecl)
                {
                    int parts = lineofCode.Split(uniformDeclLeftAndRight, '=');
                    if (parts == 2)
                    {
                        ReadOnlySpan<char> leftPart = lineofCode[uniformDeclLeftAndRight[0]];
                        ReadOnlySpan<char> rightPart = lineofCode[uniformDeclLeftAndRight[1]];

                        if (rightPart[^1] == ';') rightPart = rightPart[..^1];
                        string value = rightPart.Trim().ToString();
                        string type = string.Empty;
                        string typeQualifier = string.Empty;
                        string name = string.Empty;

                        int nameAndTypeTokens = leftPart.Trim().Split(uniformDeclTokens, ' ', StringSplitOptions.TrimEntries);
                        if (nameAndTypeTokens == 3)
                        {
                            // 0 is uniform
                            // 1 is type
                            // 2 is name
                            type = leftPart[uniformDeclTokens[1]].Trim().ToString();
                            name = leftPart[uniformDeclTokens[2]].Trim().ToString();
                        }
                        else if (nameAndTypeTokens == 4)
                        {
                            // 0 is uniform
                            // 1 is type qualifier
                            // 2 is type
                            // 3 is name
                            typeQualifier = leftPart[uniformDeclTokens[1]].Trim().ToString();
                            type = leftPart[uniformDeclTokens[2]].Trim().ToString();
                            name = leftPart[uniformDeclTokens[3]].Trim().ToString();
                        }

                        code[i] = $"uniform {typeQualifier} {type} {name};";
                        uniformDefaults.Add(new ShaderUniform(name, type, value));
                    }
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

            // Legacy using statements
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
            if (codeAdded) code[i] += $"\n#line {i} 0";

            code[i] = code[i].Trim() + "\n";
        }

        return code.ToArray();
    }

    /// <summary>
    /// Resolves file dependencies in shaders.
    /// </summary>
    private static string ResolveShaderDependency(string file)
    {
        var loadedFile = Engine.AssetLoader.LEGACY_Get<TextAsset>(file);
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

    private static string[] DefaultCompilation(string[] src)
    {
        return src;
    }

    /// <summary>
    /// Really old GPUs might require this.
    /// </summary>
    private static string[] ExplicitAttribCompilation(string[] src)
    {
        if (src == null || Gl.CurrentVersion.GLES) return null;
        src[0] = $"{src[0]}#extension GL_ARB_explicit_attrib_location : require\n";
        return src;
    }

    #endregion

    #region ONE

    /// <summary>
    /// The default shader is a basic 2D shader with an alpha discard and MVP multiplied vertices.
    /// It can also be used as a fallback shader.
    /// </summary>
    public static ShaderProgram DefaultProgram { get; private set; } = new ShaderProgram("im sure");

    public static TextAsset? DefaultProgram_Vert { get; private set; }

    public static TextAsset? DefaultProgram_Frag { get; private set; }

    public static ShaderProgram Blit { get; private set; } = new ShaderProgram("im sure");

    public static ShaderProgram BlitPremultAlpha { get; private set; } = new ShaderProgram("im sure");

    internal static IEnumerator LoadDefaultShadersRoutineAsync()
    {
        Engine.Log.Info($"Loading default shaders...", MessageSource.Renderer);

        TextAsset vert = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultVert.vert");
        TextAsset frag = Engine.AssetLoader.Get<TextAsset>("Shaders/DefaultFrag.frag");

        yield return vert;
        yield return frag;
        DefaultProgram_Vert = vert;
        DefaultProgram_Frag = frag;

        if (!vert.Loaded || !frag.Loaded)
        {
            Engine.CriticalError(new Exception("Couldn't find default shader code."));
            yield break;
        }

        Engine.Log.Info("Compiling default shaders...", MessageSource.Renderer);
        yield return GLThread.ExecuteOnGLThreadAsync(() => // todo: execute gl thread coroutine waiter
        {
            ShaderProgram? defaultShaderProgram = CreateShader(vert.Content, frag.Content);
            DefaultProgram.CopyFrom(defaultShaderProgram);
        });
        if (DefaultProgram.Pointer == 0)
        {
            Engine.CriticalError(new Exception("Couldn't compile default shader."));
            yield break;
        }

        var blit = Engine.AssetLoader.Get<ShaderAsset>("Shaders/Blit.xml");
        var blitPremultAlpha = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BlitPremultAlpha.xml");

        yield return blit;
        yield return blitPremultAlpha;

        Blit.CopyFrom(blit.Shader);
        BlitPremultAlpha.CopyFrom(blitPremultAlpha.Shader);

        // We could theoretically run /kinda/ without these, right?
        Assert(Blit.Pointer != 0);
        Assert(BlitPremultAlpha.Pointer != 0);

        Engine.Log.Info($"Default shaders created!", MessageSource.Renderer);
    }

    public static ShaderProgram? CreateShaderRaw(string vertShaderSource, string fragShaderSource)
    {
        uint vertShader = Gl.CreateShader(ShaderType.VertexShader);
        bool vertCompiled = TryCompileRaw(vertShaderSource, vertShader);
        if (!vertCompiled) Engine.Log.Warning("Vert shader compilation failed.", MessageSource.Renderer);

        uint fragShader = Gl.CreateShader(ShaderType.FragmentShader);
        bool fragCompiled = TryCompileRaw(fragShaderSource, fragShader);
        if (!fragCompiled) Engine.Log.Warning("Frag shader compilation failed.", MessageSource.Renderer);

        // If none of the stages compiled - we're cooked.
        if (!vertCompiled || !fragCompiled)
        {
            Gl.DeleteShader(vertShader);
            Gl.DeleteShader(fragShader);
            return null;
        }

        // Link into a program and add meta data in debug mode.
        ShaderProgram newShader = ShaderProgram.CreateFromShaders(vertShader, fragShader);
        if (Engine.Configuration.DebugMode)
        {
            newShader.DebugFragSource = fragShaderSource;
            newShader.DebugVertSource = vertShaderSource;
        }

        // Once the shaders are compiled into a program it is save to clean them up.
        // One might think that it's a good idea to reuse them, especially those
        // which can be used as fallbacks, but some drivers prevent them
        // from being reused once compiled.
        Gl.DeleteShader(vertShader);
        Gl.DeleteShader(fragShader);

        // Check if the program compiled successfully.
        if (!newShader.Valid) return null;

        return newShader;
    }

    private static bool TryCompileRaw(string shaderSource, uint shaderId)
    {
        Gl.ShaderSource(shaderId, [shaderSource], [shaderSource.Length]);
        Gl.CompileShader(shaderId);

        // Check if there's a log to print.
        Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int lLength);
        if (lLength > 1) // 0 without null termination
        {
            // Get the info log.
            var compileStatusReader = new StringBuilder(lLength);
            Gl.GetShaderInfoLog(shaderId, lLength, out int _, compileStatusReader);
            var compileStatus = compileStatusReader.ToString();
            Engine.Log.Warning($"Shader Compilation Log\n{compileStatus}", MessageSource.GL);
            Engine.Log.Trace(shaderSource, MessageSource.ShaderSource);
        }

        // Check if the shader compiled successfully, if not 0 is returned.
        Gl.GetShader(shaderId, ShaderParameterName.CompileStatus, out int status);
        if (status != 1)
        {
            Engine.Log.Trace($"Shader compilation failed.", MessageSource.Renderer);
            return false;
        }

        return true;
    }

    #endregion
}