#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Shading;
using OpenGL;
using System;
using System.IO;
using System.Text;

namespace Emotion.Graphics.Shader;

public class ShaderAsset : TextAsset, IAssetContainingObject<ShaderProgram>
{
    public ShaderProgram? CompiledShader;

    public ShaderAsset()
    {
        _useNewLoading = true;
    }

    public ShaderProgram? GetObject()
    {
        return CompiledShader;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        // Old shader asset compatibility
        if (Name.Contains(".xml"))
        {
            var shaderDescription = new XMLAsset<ShaderDescription>();
            shaderDescription.AssetLoader_CreateLegacy(data);
            var content = shaderDescription.Content;
            if (content == null) yield break;
            Content = content;
            yield return OldShaderCreationRoutine();
            yield break;
        }

        base.CreateInternal(data);

        // Content <- The content of our file :)
        bool es = Gl.CurrentShadingVersion.GLES;
        string versionString = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}\n";

        StringBuilder vertexShader = new StringBuilder();
        vertexShader.Append(versionString);
        vertexShader.Append("\n#define VERT_SHADER 1\n");
        vertexShader.Append(base.Content);
        vertexShader.Append("\n");
        vertexShader.Append("void main()\n{\nVertexShaderMain();\n}");

        StringBuilder fragmentShader = new StringBuilder();
        fragmentShader.Append(versionString);
        fragmentShader.Append("\n#define FRAG_SHADER 1\n");
        fragmentShader.Append(base.Content);
        fragmentShader.Append("\n");
        fragmentShader.Append("\nout vec4 fragColor;\n");
        fragmentShader.Append("void main()\n{\nfragColor = FragmentShaderMain();\n}");

        CreateShaderParams param = new CreateShaderParams(vertexShader.ToString(), fragmentShader.ToString());
        yield return GLThread.ExecuteOnGLThreadAsync(static (CreateShaderParams param) =>
        {
            ShaderProgram? compiled = ShaderFactory.CreateShaderRaw(param.VertShaderSource, param.FragShaderSource);
            param.OutShaderProgram = compiled;
        }, param);
        ShaderProgram? newShader = param.OutShaderProgram;
        ShaderProgram? oldShader = CompiledShader;
        CompiledShader = newShader;
        oldShader?.Dispose(); // When hot reloading we need to destroy the previous shader
    }

    #region Async Shader Compile

    private class CreateShaderParams
    {
        public string VertShaderSource;
        public string FragShaderSource;
        public string? CompileConstant;

        public ShaderProgram? OutShaderProgram;

        public CreateShaderParams(string vertShader, string fragShader, string? compilationConstant = null)
        {
            VertShaderSource = vertShader;
            FragShaderSource = fragShader;
            CompileConstant = compilationConstant;
        }
    }

    #endregion

    #region Legacy

    public ShaderProgram? Shader { get => CompiledShader; }

    public class ShaderDescription
    {
        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Frag { get; set; }

        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Vert { get; set; }

        /// <summary>
        /// The path to another shader description to fallback to if the creation of this shader fails.
        /// </summary>
        public string Fallback { get; set; } = "Shaders/DefaultShader.xml";
    }

    public ShaderDescription? Content { get; set; }

    /// <summary>
    /// Whether using the fallback shader.
    /// </summary>
    public bool IsFallback { get; set; }

    /// <summary>
    /// The name of the fallback shader used - if any.
    /// </summary>
    public string? FallbackName { get; set; }

    /// <summary>
    /// The compilation constant used, if this asset is a shader variation.
    /// </summary>
    public string? CompilationConstant { get; set; }

    private IEnumerator OldShaderCreationRoutine()
    {
        string? compileConstant = CompilationConstant;

        // Get the text contents of the shader files referenced. If any of them are missing substitute with the default one.
        string assetDirectory = AssetLoader.GetDirectoryName(Name);

        TextAsset? vertShader = null;
        if (!string.IsNullOrEmpty(Content.Vert))
        {
            string path = AssetLoader.GetNonRelativePath(assetDirectory, Content.Vert, false);
            vertShader = LoadAssetDependency<TextAsset>(path);
        }

        TextAsset? fragShader = null;
        if (!string.IsNullOrEmpty(Content.Frag))
        {
            string path = AssetLoader.GetNonRelativePath(assetDirectory, Content.Frag, false);
            fragShader = LoadAssetDependency<TextAsset>(path);
        }

        yield return WaitAllDependenciesToLoad();
        if (vertShader != null && !vertShader.Loaded)
        {
            Engine.Log.Warning($"Couldn't find shader file {vertShader.Name}. Using default.", MessageSource.AssetLoader);
            vertShader = null;
        }
        if (fragShader != null && !fragShader.Loaded)
        {
            Engine.Log.Warning($"Couldn't find shader file {fragShader.Name}. Using default.", MessageSource.AssetLoader);
            fragShader = null;
        }

        vertShader ??= ShaderFactory.DefaultProgram_Vert;
        fragShader ??= ShaderFactory.DefaultProgram_Frag;

        var shaderLogName = $"v:{vertShader!.Name}, f:{fragShader!.Name} {compileConstant}";
        Engine.Log.Info($"Loading shader {shaderLogName}...", MessageSource.AssetLoader);

        // Compile the shader on the GL thread.
        var compileParams = new CreateShaderParams(vertShader.Content, fragShader.Content, compileConstant);
        yield return GLThread.ExecuteOnGLThreadAsync(static (CreateShaderParams compileParams) =>
        {
            ShaderProgram? compiled = ShaderFactory.CreateShader(compileParams.VertShaderSource, compileParams.FragShaderSource, compileParams.CompileConstant);
            compileParams.OutShaderProgram = compiled;
        }, compileParams);
        ShaderProgram? compiledProgram = compileParams.OutShaderProgram;

        // Reloading shader. Keep reference of current object, substitute OpenGL pointer only.
        if (CompiledShader != null)
        {
            if (compiledProgram == null || !compiledProgram.Valid) yield break;

            DisposeInternal();
            CompiledShader.CopyFrom(compiledProgram);
            CompiledShader.DebugName = Name;
        }
        else
        {
            CompiledShader = compiledProgram;
        }

        // Check if compilation was successful.
        if (CompiledShader != null && CompiledShader.Valid)
        {
            IsFallback = false;
            FallbackName = null;
            Engine.Log.Info($"Compiled {shaderLogName}!", MessageSource.AssetLoader);
            yield break;
        }

        Engine.Log.Warning($"Shader {Name} compilation failed. Falling back.", MessageSource.AssetLoader);
        IsFallback = true;

        // If there is no fallback, fallback to default.
        if (string.IsNullOrEmpty(Content.Fallback))
        {
            Engine.Log.Warning("No fallback specified, falling back to default.", MessageSource.AssetLoader);
            FallbackName = "Default";
            CompiledShader = ShaderProgram.CreateCopied(ShaderFactory.DefaultProgram);
            yield break;
        }

        var fallBackShader = Engine.AssetLoader.Get<ShaderAsset>(Content.Fallback);
        yield return fallBackShader;

        // If fallback not found, fallback to default.
        if (fallBackShader.CompiledShader == null)
        {
            Engine.Log.Warning($"Fallback {Content.Fallback} not found. Falling back to default.", MessageSource.AssetLoader);
            FallbackName = "Default";
            CompiledShader = ShaderProgram.CreateCopied(ShaderFactory.DefaultProgram);
            yield break;
        }

        Engine.Log.Warning($"Shader {Name} fell back to {Content.Fallback}.", MessageSource.AssetLoader);
        FallbackName = Content.Fallback;
        CompiledShader = ShaderProgram.CreateCopied(fallBackShader.CompiledShader);
    }
    public string Variation { get; set; } = string.Empty;

    private Dictionary<string, ShaderAsset> _variations = new();

    /// <summary>
    /// Create a shader variation by compiling it with a specified compile constant
    /// constant. The asset created by this function isn't managed by the AssetLoader.
    /// </summary>
    /// <returns></returns>
    public ShaderAsset GetShaderVariation(string compileConstant)
    {
        lock (_variations)
        {
            if (_variations.TryGetValue(compileConstant, out ShaderAsset? shaderAsset)) return shaderAsset;

            var newAsset = new ShaderAsset
            {
                Content = Content,
                Name = Name,
                Variation = $"{Variation} #{compileConstant}",
                CompilationConstant = compileConstant
            };
            Engine.CoroutineManager.StartCoroutine(newAsset.OldShaderCreationRoutine());
            _variations.Add(compileConstant, newAsset);

            return newAsset;
        }
    }


    #endregion
}
