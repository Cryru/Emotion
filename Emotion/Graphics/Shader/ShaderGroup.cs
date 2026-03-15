#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using OpenGL;
using System.Collections.Concurrent;
using System.Text;

namespace Emotion.Graphics.Shader;

public class ShaderGroup
{
    public bool Valid { get; private set; }
    public string Name;
    public string ShaderContent;

    public string VersionString = "Invalid";
    private StringBuilder _includes = new StringBuilder();

    // Dynamic vertex format
    private StringBuilder _vertexFormatDefine = new();
    private StringBuilder _vertShaderPassParameters = new();

    private ConcurrentDictionary<ShaderGroupDefinition, ShaderGroupCompileStatus> _compilations = new();
    private List<ShaderVertexAttribute> _vertAttribute = new();

    public ShaderGroup(
        string name,
        string shaderContent
    )
    {
        Name = name;
        ShaderContent = shaderContent;
    }

    public IEnumerator Init()
    {
        bool es = Gl.CurrentShadingVersion.GLES;
        VersionString = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}\n";

        // Process includes
        StringBuilder include = _includes;
        include.AppendLine("// INCLUDES");
        include.AppendLine();

        int includeCount = 1;

        // Include GLES support if running on GLES
        if (es)
        {
            TextAsset includeFile = Engine.AssetLoader.Get<TextAsset>("Shaders/GLESSupport.c");
            yield return includeFile;

            include.AppendLine("// Shaders/GLESSupport.c");
            include.AppendLine();
            include.Append($"\n#line 0 {includeCount}\n");
            include.Append(includeFile.Content);
            includeCount++;
        }

        // Always include common
        {
            TextAsset includeFile = Engine.AssetLoader.Get<TextAsset>("Shaders/Common.h");
            yield return includeFile;

            include.AppendLine("// Shaders/Common.h");
            include.AppendLine();
            include.Append($"\n#line 0 {includeCount}\n");
            include.Append(includeFile.Content);
            includeCount++;
        }

        foreach (string includePath in ForEachInclude(ShaderContent))
        {
            if (includePath == "Shaders/Common.h") continue;

            TextAsset includeFile = Engine.AssetLoader.Get<TextAsset>(includePath, this, false, true);
            yield return includeFile;

            include.AppendLine($"// {includePath}");
            include.AppendLine();
            include.Append($"\n#line 0 {includeCount}\n");
            include.Append(includeFile.Content);
            includeCount++;
        }

        // Determine the vertex format support
        VertexDataFormat format = new VertexDataFormat();
        foreach (ShaderVertexAttribute attribute in ForEachDefinedVertexAttribute(ShaderContent))
        {
            format.AddAttribute(attribute.AttributeType);
            _vertAttribute.Add(attribute);
        }
        format.Build();
        GenerateCodeForVertexFormat(format);
        Valid = true;
    }

    public ShaderProgram? GetShader(in ShaderGroupDefinition def)
    {
        if (!Valid)
            return null;

        VertexDataFormat defFormat = def.Format;

        // Check if shader format is a subset of the definiton format.

        // Check if the compilation has already been queued/finished.
        if (_compilations.TryGetValue(def, out ShaderGroupCompileStatus? compilation))
            return compilation.Shader;

        _compilations.TryAdd(def, new ShaderGroupCompileStatus());
        Engine.Jobs.AddNoFeedback(CompileShaderVariantRoutineAsync(this, def));
        return null;
    }

    public IEnumerator GetShaderRoutine(ShaderOut shaderOut, ShaderGroupDefinition def)
    {
        if (!Valid)
            yield break;

        VertexDataFormat defFormat = def.Format;

        // Check if shader format is a subset of the definiton format.

        // Check if the compilation has already been queued/finished.
        if (_compilations.TryGetValue(def, out ShaderGroupCompileStatus? compilation))
        {
            shaderOut.OutShaderProgram = compilation.Shader;
            yield break;
        }

        _compilations.TryAdd(def, new ShaderGroupCompileStatus());
        yield return CompileShaderVariantRoutine(this, shaderOut, def);
    }

    public (StringBuilder vertShader, StringBuilder fragShader) GetShaderSourceForPipelineDef(in ShaderGroupDefinition def)
    {
        VertexDataFormat defFormat = def.Format;

        // Write the code brought by the pipeline def
        StringBuilder pipelineDefCode = new StringBuilder();

        pipelineDefCode.Append("\n// VERTEX ATTRIBUTE DEFINES\n");
        bool hasBones = false;
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool add = false;
            if (attr.Optional)
                add = defFormat.HasAttribute(attr.AttributeType);
            else
                add = true;

            if (add)
            {
                pipelineDefCode.AppendLine($"#define HAS_{attr.AttributeType} 1");
                hasBones = hasBones || attr.AttributeType == VertexDataFormatAttributeType.BoneIds || attr.AttributeType == VertexDataFormatAttributeType.BoneWeights;
            }
        }
        if (hasBones)
            pipelineDefCode.AppendLine("#define HAS_BONES 1");

        if (def.Defines != null)
        {
            pipelineDefCode.Append("\n// PIPELINE VARIATION\n");
            foreach (string item in def.Defines)
            {
                pipelineDefCode.AppendLine($"#define {item}");
            }
        }

        pipelineDefCode.Append("\n// VERTEX DATA FORMAT\n");
        pipelineDefCode.Append(_vertexFormatDefine);

        StringBuilder vertexShader = new StringBuilder();
        vertexShader.Append(VersionString);
        vertexShader.Append("\n#define VERT_SHADER 1\n");

        vertexShader.Append(_includes);
        vertexShader.Append(pipelineDefCode);

        vertexShader.AppendLine();
        vertexShader.Append("#line 1 0\n");
        vertexShader.Append(ShaderContent);
        vertexShader.Append("\n\n// GENERATED MAIN\n");
        vertexShader.Append("void main()\n");
        vertexShader.Append("{\n");
        vertexShader.Append(_vertShaderPassParameters);
        vertexShader.Append("\n    gl_Position = VertexShaderMain();\n");
        vertexShader.Append("}\n");

        StringBuilder fragmentShader = new StringBuilder();
        fragmentShader.Append(VersionString);
        fragmentShader.Append("\n#define FRAG_SHADER 1\n");

        fragmentShader.Append(_includes);
        fragmentShader.Append(pipelineDefCode);

        fragmentShader.Append("\n");
        fragmentShader.Append("#line 1 0\n");
        fragmentShader.Append(ShaderContent);
        fragmentShader.Append("\n\n// GENERATED MAIN\n");
        fragmentShader.Append("\nout vec4 fragColor;\n");
        fragmentShader.Append("void main()\n");
        fragmentShader.Append("{\n");
        fragmentShader.Append("    fragColor = FragmentShaderMain();\n");
        fragmentShader.Append("}\n");

        return (vertexShader, fragmentShader);
    }

    public IEnumerator GetCompiledShaderForPipelineDefRoutine(ShaderOut compiledOut, ShaderGroupDefinition def)
    {
        (StringBuilder vert, StringBuilder frag) = GetShaderSourceForPipelineDef(def);

        Engine.Log.ONE_Info("Pipeline", $"Compiling {Name} {def}");
        compiledOut.VertexSource = vert;
        compiledOut.FragmentSource = frag;
        yield return GLThread.ExecuteOnGLThreadAsync(static (ShaderOut param) =>
        {
            string? vertSource = param.VertexSource?.ToString();
            string? fragSource = param.FragmentSource?.ToString();
            if (vertSource == null || fragSource == null) return;

            ShaderProgram? compiled = ShaderFactory.CreateShaderRaw(vertSource, fragSource);
            param.OutShaderProgram = compiled;
        }, compiledOut);

        ShaderProgram? compiledShader = compiledOut.OutShaderProgram;
        if (compiledShader == null)
        {
            Engine.Log.ONE_Error("Pipeline", $"Failed compilation of {Name} {def}");
            yield break;
        }
        Engine.Log.ONE_Info("Pipeline", $"Compiled {Name} {def}");
    }

    private static IEnumerator CompileShaderVariantRoutine(ShaderGroup group, ShaderOut shaderOut, ShaderGroupDefinition def)
    {
        yield return group.GetCompiledShaderForPipelineDefRoutine(shaderOut, def);

        ShaderProgram? compiledShader = shaderOut.OutShaderProgram;
        if (compiledShader == null) yield break;

        lock (group)
        {
            if (!group.Valid)
            {
                compiledShader?.Dispose();
                yield break;
            }

            if (group._compilations.TryGetValue(def, out ShaderGroupCompileStatus? compilation))
                compilation.SetShader(compiledShader);
            AssertNotNull(compilation);
        }
    }

    private static IEnumerator CompileShaderVariantRoutineAsync(ShaderGroup group, ShaderGroupDefinition def)
    {
        ShaderOut create = new ShaderOut(); // todo: pool
        yield return CompileShaderVariantRoutine(group, create, def);
    }

    public void Dispose()
    {
        lock (this)
        {
            Valid = false;
            foreach (KeyValuePair<ShaderGroupDefinition, ShaderGroupCompileStatus> item in _compilations)
            {
                item.Value.Shader?.Dispose();
            }
            _compilations.Clear();
        }
    }

    #region Helpers

    private static IEnumerable<string> ForEachInclude(string source)
    {
        const string includeToken = "INCLUDE_FILE";
        int tokenLength = includeToken.Length;

        int offset = 0;
        while (offset < source.Length)
        {
            int idx = source.IndexOf(includeToken, offset);
            if (idx < 0) yield break; // No more

            idx += tokenLength;

            // Skip whitespaces
            while (idx < source.Length && char.IsWhiteSpace(source[idx]))
                idx++;
            if (idx == source.Length) break;

            // Check if opening
            if (source[idx] == '<')
            {
                // Read to closing
                int end = source.IndexOf('>', idx + 1);
                if (end != -1)
                {
                    yield return source.Substring(idx + 1, end - idx - 1);
                }

                offset = end + 1;
            }
        }
    }

    private static IEnumerable<ShaderVertexAttribute> ForEachDefinedVertexAttribute(string source)
    {
        const string defineToken = "DEFINE_VERTEX_ATTRIBUTE";
        int tokenLength = defineToken.Length;

        ReflectorTypeHandlerBase<VertexDataFormatAttributeType>? attributeTypeHandler = ReflectorEngine.GetTypeHandler<VertexDataFormatAttributeType>();
        EnumTypeHandler<VertexDataFormatAttributeType, int>? enumHandler = attributeTypeHandler as EnumTypeHandler<VertexDataFormatAttributeType, int>;
        AssertNotNull(enumHandler);
        if (enumHandler == null)
        {
            Engine.Log.ONE_Error("Shader", "VertexDataFormatAttributeType reflector handler missing!");
            yield break;
        }

        Range[] ranges = new Range[3]; // pool, since we cant stack alloc in the iterator :P
        int offset = 0;
        while (offset < source.Length)
        {
            int idx = source.IndexOf(defineToken, offset);
            if (idx < 0) yield break; // No more

            int lineEnd = source.IndexOf('\n', idx);
            if (lineEnd == -1) lineEnd = source.Length;

            int lineStart = idx + tokenLength;
            ReadOnlySpan<char> lineSpan = source.AsSpan(lineStart, lineEnd - lineStart);

            int found = lineSpan.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (found >= 2) // Expecting two arguments
            {
                ReadOnlySpan<char> vertexAttribute = lineSpan[ranges[0]];
                if (enumHandler.TryParse(vertexAttribute, out VertexDataFormatAttributeType typ))
                {
                    ReadOnlySpan<char> vertexAttributeName = lineSpan[ranges[1]];
                    bool isOptional = vertexAttributeName.EndsWith('?');
                    if (isOptional) vertexAttributeName = vertexAttributeName[..^1];

                    ShaderVertexAttribute attr = new ShaderVertexAttribute(vertexAttributeName.ToString(), typ, isOptional);
                    yield return attr;
                }
                else
                {
                    Engine.Log.ONE_Error("Shader", $"Couldn't parse VertexDataFormatAttributeType {vertexAttribute}");
                }
            }

            offset = lineEnd;
        }
    }

    private void GenerateCodeForVertexFormat(VertexDataFormat format)
    {
        // Assign locations and shader types
        int uvCount = 0;
        for (int i = 0; i < _vertAttribute.Count; i++)
        {
            ShaderVertexAttribute item = _vertAttribute[i];
            Assert(format.HasAttribute(item.AttributeType));

            switch (item.AttributeType)
            {
                case VertexDataFormatAttributeType.Position:
                    {
                        item.AttributeLocation = 0;
                        item.DataType = "vec3";
                    }
                    break;
                case VertexDataFormatAttributeType.UV:
                    {
                        Assert(format.HasUVCount > uvCount);

                        int location = (format.HasPosition ? 1 : 0) + uvCount;
                        uvCount++;
                        item.AttributeLocation = location;
                        item.DataType = "vec2";
                    }
                    break;
                case VertexDataFormatAttributeType.Normal:
                    {
                        Assert(format.HasNormals);

                        int location = (format.HasPosition ? 1 : 0) + format.HasUVCount;
                        item.AttributeLocation = location;
                        item.DataType = "vec3";
                    }
                    break;
                case VertexDataFormatAttributeType.VertexColor:
                    {
                        Assert(format.HasVertexColors);

                        int location = (format.HasPosition ? 1 : 0) + format.HasUVCount + (format.HasNormals ? 1 : 0);
                        item.AttributeLocation = location;
                        item.DataType = "vec4";
                    }
                    break;
                case VertexDataFormatAttributeType.BoneIds:
                    {
                        Assert(format.HasBones);

                        int location = (format.HasPosition ? 1 : 0) + format.HasUVCount + (format.HasNormals ? 1 : 0) + (format.HasVertexColors ? 1 : 0);
                        item.AttributeLocation = location;
                        item.DataType = "vec4";
                    }
                    break;
                case VertexDataFormatAttributeType.BoneWeights:
                    {
                        Assert(format.HasBones);

                        int location = (format.HasPosition ? 1 : 0) + format.HasUVCount + (format.HasNormals ? 1 : 0) + (format.HasVertexColors ? 1 : 0) + 1;
                        item.AttributeLocation = location;
                        item.DataType = "vec4";
                    }
                    break;
                default:
                    {
                        if (item.AttributeType > VertexDataFormatAttributeType.CustomStart)
                        {
                            int customIdx = (item.AttributeType - VertexDataFormatAttributeType.CustomStart) - 1;
                            int location = (format.HasPosition ? 1 : 0) +
                                format.HasUVCount +
                                (format.HasNormals ? 1 : 0) +
                                (format.HasVertexColors ? 1 : 0) +
                                (format.HasBones ? 2 : 0) +
                                customIdx;
                            item.AttributeLocation = location;
                            item.DataType = "vec4";
                        }
                    }
                    break;
            }
        }


        // Vertex shader vertex attributes
        _vertexFormatDefine.AppendLine($"#ifdef VERT_SHADER");
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool endIf = false;
            if (attr.Optional)
            {
                _vertexFormatDefine.AppendLine($"#ifdef HAS_{attr.AttributeType}");
                endIf = true;
            }

            _vertexFormatDefine.AppendLine($"layout(location = {attr.AttributeLocation}) in {attr.DataType} {attr.AttributeName};");
            _vertexFormatDefine.AppendLine($"out {attr.DataType} pass_{attr.AttributeName};");

            if (endIf)
                _vertexFormatDefine.AppendLine($"#endif");
        }
        _vertexFormatDefine.AppendLine($"#endif");
        _vertexFormatDefine.AppendLine();

        // Fragment shader vertex attributes
        _vertexFormatDefine.AppendLine($"#ifdef FRAG_SHADER");
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool endIf = false;
            if (attr.Optional)
            {
                _vertexFormatDefine.AppendLine($"#ifdef HAS_{attr.AttributeType}");
                endIf = true;
            }

            _vertexFormatDefine.AppendLine($"in {attr.DataType} pass_{attr.AttributeName};");
            _vertexFormatDefine.AppendLine($"{attr.DataType} {attr.AttributeName} = pass_{attr.AttributeName};");

            if (endIf)
                _vertexFormatDefine.AppendLine($"#endif");
        }
        _vertexFormatDefine.AppendLine($"#endif");

        // Write pass code for the vertex shader
        _vertShaderPassParameters.AppendLine("    // PASS LOGIC");
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool endIf = false;
            if (attr.Optional)
            {
                _vertShaderPassParameters.AppendLine($"#ifdef HAS_{attr.AttributeType}");
                endIf = true;
            }
            _vertShaderPassParameters.AppendLine($"    pass_{attr.AttributeName} = {attr.AttributeName};");
            if (endIf)
                _vertShaderPassParameters.AppendLine("#endif");
        }
    }

    #endregion

    public class ShaderOut
    {
        public StringBuilder? FragmentSource;
        public StringBuilder? VertexSource;
        public ShaderProgram? OutShaderProgram;
    }

    private class ShaderVertexAttribute
    {
        public VertexDataFormatAttributeType AttributeType;
        public string AttributeName;
        public bool Optional;

        // Assigned programatically
        public int AttributeLocation;
        public string DataType = string.Empty;

        public ShaderVertexAttribute(string attributeName, VertexDataFormatAttributeType attributeType, bool optional)
        {
            AttributeName = attributeName;
            AttributeType = attributeType;
            Optional = optional;
        }
    }

    private class ShaderGroupCompileStatus
    {
        public ShaderProgram? Shader;
        public bool Ready;

        public ShaderGroupCompileStatus()
        {
            Shader = null;
            Ready = false;
        }

        public void SetShader(ShaderProgram shader)
        {
            Shader = shader;
            Ready = true;
        }
    }
}

public struct ShaderGroupDefinition : IEquatable<ShaderGroupDefinition>
{
    public VertexDataFormat Format;
    public IReadOnlyList<string>? Defines;
    public int Hash;

    public ShaderGroupDefinition(VertexDataFormat format, List<string>? defines = null)
    {
        Format = format;
        Defines = defines;

        int hash = Format.Hash;
        if (Defines != null)
        {
            foreach (string item in Defines)
            {
                hash += item.GetStableHashCode();
            }
        }
        Hash = hash;
    }

    public readonly override int GetHashCode()
    {
        return Hash;
    }

    public override readonly string ToString()
    {
        if (Defines == null) return Format.ToString() ?? "Unknown";
        return $"{Format} Defs: {string.Join(", ", Defines)}";
    }

    public readonly bool Equals(ShaderGroupDefinition other)
    {
        return Hash == other.Hash;
    }

    public static bool operator ==(ShaderGroupDefinition a, ShaderGroupDefinition b)
    {
        return a.Hash == b.Hash;
    }

    public static bool operator !=(ShaderGroupDefinition a, ShaderGroupDefinition b)
    {
        return a.Hash != b.Hash;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is ShaderGroupDefinition objAsType && Equals(objAsType);
    }
}