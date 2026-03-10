#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using System.Text;

namespace Emotion.Graphics.Shader;

public class ShaderGroup
{
    public bool Valid { get; private set; }

    public string Name;

    public string VersionString;
    public string ShaderContent;
    public StringBuilder Includes;

    // Dynamic vertex format
    private StringBuilder _defines = new();
    private StringBuilder _vertexFormatDefine = new();
    private StringBuilder _fragmentFormatDefine = new();
    private StringBuilder _vertexShaderExtraCode = new();
    private StringBuilder _fragmentShaderExtraCode = new();

    private Dictionary<ShaderGroupDefinition, ShaderGroupCompileStatus> _compilations = new();
    private List<ShaderVertexAttribute> _vertAttribute = new();

    public ShaderGroup(
        string name,
        string versionString,
        string shaderContent,
        StringBuilder include
    )
    {
        Name = name;
        VersionString = versionString;
        ShaderContent = shaderContent;
        Includes = include;
    }

    internal void Init()
    {
        // Determine the vertex format binding
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

        _compilations.Add(def, new ShaderGroupCompileStatus());
        Engine.Jobs.AddNoFeedback(CompileShaderVariantRoutineAsync(this, def));
        return null;
    }

    private static IEnumerator CompileShaderVariantRoutineAsync(ShaderGroup group, ShaderGroupDefinition def)
    {
        VertexDataFormat defFormat = def.Format;

        StringBuilder vertexShader = new StringBuilder();
        vertexShader.Append(group.VersionString);
        vertexShader.Append("\n#define VERT_SHADER 1\n");

        vertexShader.Append("\n// OPTIONAL ATTRIBUTES\n");
        foreach (ShaderVertexAttribute attr in group._vertAttribute)
        {
            if (defFormat.HasAttribute(attr.AttributeType))
                vertexShader.Append(attr.OptionalDefine);
        }

        if (def.Defines != null)
        {
            vertexShader.Append("\n// COMPILATION DEFINITION DEFINES\n");
            foreach (string item in def.Defines)
            {
                vertexShader.Append($"#define {item}");
            }
        }

        vertexShader.Append("\n// DEFINES\n");
        vertexShader.Append(group._defines);
        vertexShader.Append("\n// INCLUDES\n");
        vertexShader.Append(group.Includes);
        vertexShader.Append("\n");
        vertexShader.Append("\n// VERTEX DATA FORMAT\n");
        vertexShader.Append(group._vertexFormatDefine);
        vertexShader.Append("\n");
        vertexShader.Append("#line 1 0\n");
        vertexShader.Append(group.ShaderContent);
        vertexShader.Append("\n\n// GENERATED MAIN\n");
        vertexShader.Append("void main()");
        vertexShader.Append("{\n");
        vertexShader.Append(group._vertexShaderExtraCode);
        vertexShader.Append("\n    gl_Position = VertexShaderMain();\n");
        vertexShader.Append("}\n");

        StringBuilder fragmentShader = new StringBuilder();
        fragmentShader.Append(group.VersionString);
        fragmentShader.Append("\n#define FRAG_SHADER 1\n");

        fragmentShader.Append("\n// OPTIONAL ATTRIBUTES\n");
        foreach (ShaderVertexAttribute attr in group._vertAttribute)
        {
            if (defFormat.HasAttribute(attr.AttributeType))
                fragmentShader.Append(attr.OptionalDefine);
        }

        if (def.Defines != null)
        {
            fragmentShader.Append("\n// COMPILATION DEFINITION DEFINES\n");
            foreach (string item in def.Defines)
            {
                fragmentShader.Append($"#define {item}");
            }
        }

        fragmentShader.Append("\n// DEFINES\n");
        fragmentShader.Append(group._defines);
        fragmentShader.Append("\n// INCLUDES\n");
        fragmentShader.Append(group.Includes);
        fragmentShader.Append("\n");
        fragmentShader.Append("\n// VERTEX DATA FORMAT\n");
        fragmentShader.Append(group._fragmentFormatDefine);
        fragmentShader.Append("\n");
        fragmentShader.Append("#line 1 0\n");
        fragmentShader.Append(group.ShaderContent);
        fragmentShader.Append("\n\n// GENERATED MAIN\n");
        fragmentShader.Append("\nout vec4 fragColor;\n");
        fragmentShader.Append("void main()");
        fragmentShader.Append("{\n");
        fragmentShader.Append(group._fragmentShaderExtraCode);
        fragmentShader.Append("\n    fragColor = FragmentShaderMain();\n");
        fragmentShader.Append("}\n");

        Engine.Log.ONE_Info("Shader", $"{group.Name} {def} compiling...");
        CreateShaderParams param = new CreateShaderParams(vertexShader.ToString(), fragmentShader.ToString()); // todo: pool
        yield return GLThread.ExecuteOnGLThreadAsync(static (CreateShaderParams param) =>
        {
            ShaderProgram? compiled = ShaderFactory.CreateShaderRaw(param.VertShaderSource, param.FragShaderSource);
            param.OutShaderProgram = compiled;
        }, param);

        ShaderProgram? compiledShader = param.OutShaderProgram;
        if (compiledShader == null) yield break; // Didn't compile
        Engine.Log.ONE_Info("Shader", $"{group.Name} {def} compiled!");

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

        // Write defines
        bool wroteHasBones = false;
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            StringBuilder writeDefineTo = _defines;

            bool optionalDefineWritten = false;
            if (attr.Optional)
            {
                optionalDefineWritten = true;
                attr.OptionalIfDef = $"#ifdef HAS_{attr.AttributeType}";
                writeDefineTo = new StringBuilder();
            }

            if (!wroteHasBones &&
                (attr.AttributeType == VertexDataFormatAttributeType.BoneIds || attr.AttributeType == VertexDataFormatAttributeType.BoneWeights)
            )
            {
                writeDefineTo.AppendLine($"#define HAS_BONES 1");
                wroteHasBones = true;
            }

            writeDefineTo.AppendLine($"#define HAS_{attr.AttributeType} 1");

            if (optionalDefineWritten)
                attr.OptionalDefine = writeDefineTo.ToString();
        }

        // Write format code
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool endIf = false;
            if (attr.Optional)
            {
                _vertexFormatDefine.AppendLine(attr.OptionalIfDef);
                _fragmentFormatDefine.AppendLine(attr.OptionalIfDef);
                endIf = true;
            }

            _vertexFormatDefine.AppendLine($"layout(location = {attr.AttributeLocation}) in {attr.DataType} {attr.AttributeName};");
            _fragmentFormatDefine.AppendLine($"in {attr.DataType} pass_{attr.AttributeName};");

            if (endIf)
            {
                _vertexFormatDefine.AppendLine($"#endif");
                _fragmentFormatDefine.AppendLine($"#endif");
            }
        }

        _vertexFormatDefine.AppendLine();
        _vertexFormatDefine.AppendLine("// PASS LOGIC");
        _vertexFormatDefine.AppendLine();

        _fragmentFormatDefine.AppendLine();
        _fragmentFormatDefine.AppendLine("// PASS LOGIC");
        _fragmentFormatDefine.AppendLine();

        // Write pass code
        foreach (ShaderVertexAttribute attr in _vertAttribute)
        {
            bool endIf = false;
            if (attr.Optional)
            {
                _vertexFormatDefine.AppendLine(attr.OptionalIfDef);
                _vertexShaderExtraCode.AppendLine(attr.OptionalIfDef);
                _fragmentFormatDefine.AppendLine(attr.OptionalIfDef);
                _fragmentShaderExtraCode.AppendLine(attr.OptionalIfDef);
                endIf = true;
            }

            _vertexFormatDefine.AppendLine($"out {attr.DataType} pass_{attr.AttributeName};");
            _vertexShaderExtraCode.AppendLine($"    pass_{attr.AttributeName} = {attr.AttributeName};");

            _fragmentFormatDefine.AppendLine($"{attr.DataType} {attr.AttributeName};");
            _fragmentShaderExtraCode.AppendLine($"    {attr.AttributeName} = pass_{attr.AttributeName};");

            if (endIf)
            {
                _vertexFormatDefine.AppendLine("#endif");
                _vertexShaderExtraCode.AppendLine("#endif");
                _fragmentFormatDefine.AppendLine("#endif");
                _fragmentShaderExtraCode.AppendLine("#endif");
            }
        }
    }

    #endregion

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

    private class ShaderVertexAttribute
    {
        public VertexDataFormatAttributeType AttributeType;
        public string AttributeName;
        public bool Optional;

        // Assigned programatically
        public int AttributeLocation;
        public string DataType = string.Empty;

        public string OptionalIfDef = string.Empty;
        public string OptionalDefine = string.Empty;

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