#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;

#endregion

namespace Emotion.Game.ThreeDee;

/// <summary>
/// Holds runtime state information referring to a mesh entity.
/// This is information that only applies to the particular instance of the
/// entity is use by the object, but not pertaining to the object itself.
/// </summary>
[DontSerialize]
public class MeshEntityMetaState
{
    /// <summary>
    /// Whether the mesh index should be rendered.
    /// </summary>
    public bool[] RenderMesh = Array.Empty<bool>();

    /// <summary>
    /// Additional scale for all dimensions of the entity.
    /// </summary>
    public float Scale = 1f;

    /// <summary>
    /// A color to tint all vertices of the entity in.
    /// Is multiplied by the material color.
    /// </summary>
    public Color Tint = Color.White;

    public ShaderAsset? ShaderAsset { get; private set; }
    private Dictionary<string, IMeshMaterialShaderParameter>? _shaderParameters;

    public RenderState? CustomRenderState;

    public MeshEntityMetaState(MeshEntity? entity)
    {
        if (entity?.Meshes == null) return;

        RenderMesh = new bool[entity.Meshes.Length];
        for (var i = 0; i < RenderMesh.Length; i++)
        {
            RenderMesh[i] = true;
        }
    }

    public async Task SetShader(string path)
    {
        _shaderParameters = new();
        ShaderAsset = await Engine.AssetLoader.GetAsync<ShaderAsset>(path);
    }

    public void SetShader(ShaderAsset asset)
    {
        _shaderParameters = new();
        ShaderAsset = asset;
    }

    public void SetShaderParam<T>(string name, T value) where T : struct
    {
        if (_shaderParameters == null) return;


        if (_shaderParameters.ContainsKey(name))
        {
            MeshMaterialShaderParameter<T>? param = _shaderParameters[name] as MeshMaterialShaderParameter<T>;
            if (param == null) return; // Parameters can't change types.
            param.Value = value;
        }
        else
        {
            IMeshMaterialShaderParameter? newParam = null;
            if (value is float floatVal)
                newParam = new MeshMaterialShaderParameterFloat(name, floatVal);
            else if (value is Vector2 vec2Val)
                newParam = new MeshMaterialShaderParameterVec2(name, vec2Val);
            else if (value is Vector3 vec3Val)
                newParam = new MeshMaterialShaderParameterVec3(name, vec3Val);
            else if (value is Vector4 vec4Val)
                newParam = new MeshMaterialShaderParameterVec4(name, vec4Val);
            else if (value is Color colorVal)
                newParam = new MeshMaterialShaderParameterColor(name, colorVal);
            if (newParam == null) return;
            _shaderParameters.Add(name, newParam);
        }
    }

    public void ApplyShaderUniforms(ShaderProgram program)
    {
        if (_shaderParameters == null) return;
        foreach ((string paramName, IMeshMaterialShaderParameter param) in _shaderParameters)
        {
            param.Apply(program, paramName);
        }
    }
}

#region Shader Uniform Types

public interface IMeshMaterialShaderParameter
{
    public void Apply(ShaderProgram program, string name);
}

public abstract class MeshMaterialShaderParameter<T> : IMeshMaterialShaderParameter where T : struct
{
    public string ParamName;
    public T Value;

    public MeshMaterialShaderParameter(string paramName, T val)
    {
        ParamName = paramName;
        Value = val;
    }

    public abstract void Apply(ShaderProgram program, string name);
}

public sealed class MeshMaterialShaderParameterFloat : MeshMaterialShaderParameter<float>
{
    public MeshMaterialShaderParameterFloat(string paramName, float val) : base(paramName, val)
    {
    }

    public override void Apply(ShaderProgram program, string name)
    {
        program.SetUniformFloat(name, Value);
    }
}

public sealed class MeshMaterialShaderParameterVec2 : MeshMaterialShaderParameter<Vector2>
{
    public MeshMaterialShaderParameterVec2(string paramName, Vector2 val) : base(paramName, val)
    {
    }

    public override void Apply(ShaderProgram program, string name)
    {
        program.SetUniformVector2(name, Value);
    }
}

public sealed class MeshMaterialShaderParameterVec3 : MeshMaterialShaderParameter<Vector3>
{
    public MeshMaterialShaderParameterVec3(string paramName, Vector3 val) : base(paramName, val)
    {
    }

    public override void Apply(ShaderProgram program, string name)
    {
        program.SetUniformVector3(name, Value);
    }
}

public sealed class MeshMaterialShaderParameterVec4 : MeshMaterialShaderParameter<Vector4>
{
    public MeshMaterialShaderParameterVec4(string paramName, Vector4 val) : base(paramName, val)
    {
    }

    public override void Apply(ShaderProgram program, string name)
    {
        program.SetUniformVector4(name, Value);
    }
}

public sealed class MeshMaterialShaderParameterColor : MeshMaterialShaderParameter<Color>
{
    public MeshMaterialShaderParameterColor(string paramName, Color val) : base(paramName, val)
    {
    }

    public override void Apply(ShaderProgram program, string name)
    {
        program.SetUniformColor(name, Value);
    }
}

#endregion