#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Game.World;
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
    public ObjectFlags? CustomObjectFlags; // used for the render flags, maybe split them?

    private MeshEntity _entity;
    private Matrix4x4[][] _boneMatricesPerMesh;
    private int[][] _skinToRigMappingPerMesh;

    protected const int MAX_BONES = 200; // Must match number in MeshShader.vert

    public MeshEntityMetaState(MeshEntity entity)
    {
        _entity = entity;
        RenderMesh = new bool[entity.Meshes.Length];

        _boneMatricesPerMesh = new Matrix4x4[entity.Meshes.Length][];
        _skinToRigMappingPerMesh = new int[entity.Meshes.Length][];

        // Build a mapping of bone indices in the skin to bone indices in the rig.
        // This will also filter out bones that are not used in the skin.
        bool skinned = entity.AnimationRigOne.Length > 0;
        SkeletonAnimRigNode[] flatAnimationRig = entity.AnimationRigOne;
        for (int meshIdx = 0; meshIdx < entity.Meshes.Length; meshIdx++)
        {
            Mesh mesh = entity.Meshes[meshIdx];
            RenderMesh[meshIdx] = true;

            Matrix4x4[] matrices;
            int[] skinToRigMapping;
            if (skinned && mesh.BoneData != null)
            {
                int largestBoneIdUsed = 0;
                for (int j = 0; j < mesh.BoneData.Length; j++)
                {
                    Graphics.Data.Mesh3DVertexDataBones data = mesh.BoneData[j];
                    Vector4 boneIds = data.BoneIds;
                    for (int b = 0; b < 4; b++)
                    {
                        int jointRef = (int) boneIds[b];
                        if (jointRef > largestBoneIdUsed) largestBoneIdUsed = jointRef;
                    }
                }

                if (largestBoneIdUsed > MAX_BONES)
                {
                    Engine.Log.Error($"Entity {_entity.Name}'s mesh {mesh.Name} has too many bones ({largestBoneIdUsed} > {MAX_BONES}).", "3D");
                }

                largestBoneIdUsed++; // Include this bone
                matrices = new System.Numerics.Matrix4x4[largestBoneIdUsed];

                skinToRigMapping = new int[largestBoneIdUsed];
                MeshBone[]? bones = mesh.Bones;
                AssertNotNull(bones);
                for (int boneInSkinIdx = 0; boneInSkinIdx < largestBoneIdUsed; boneInSkinIdx++)
                {
                    MeshBone boneInSkin = bones[boneInSkinIdx];

                    int boneInRig = 0;
                    for (int i = 0; i < flatAnimationRig.Length; i++)
                    {
                        SkeletonAnimRigNode rigNode = flatAnimationRig[i];
                        if (rigNode.Name == boneInSkin.Name)
                        {
                            boneInRig = i;
                            break;
                        }
                    }

                    skinToRigMapping[boneInSkinIdx] = boneInRig;
                }
            }
            else
            {
                matrices = [Matrix4x4.Identity];
                skinToRigMapping = Array.Empty<int>();
            }
            _boneMatricesPerMesh[meshIdx] = matrices;
            _skinToRigMappingPerMesh[meshIdx] = skinToRigMapping;
        }
    }

    public Matrix4x4[] GetBoneMatricesForMesh(int meshIdx)
    {
        if (meshIdx >= _boneMatricesPerMesh.Length) return Array.Empty<Matrix4x4>();
        return _boneMatricesPerMesh[meshIdx];
    }

    public void UpdateMeshMatrices(Matrix4x4[] rigMatrices)
    {
        for (int meshIdx = 0; meshIdx < _boneMatricesPerMesh.Length; meshIdx++)
        {
            Matrix4x4[] boneMatricesForMesh = _boneMatricesPerMesh[meshIdx];
            int[] mappingForMesh = _skinToRigMappingPerMesh[meshIdx];

            var mesh = _entity.Meshes[meshIdx];
            var meshBones = mesh.Bones;

            for (int b = 0; b < boneMatricesForMesh.Length; b++)
            {
                int rigBoneId = mappingForMesh[b];
                MeshBone meshBone = meshBones[b];
                Matrix4x4 matrix = rigMatrices[rigBoneId];
                boneMatricesForMesh[b] = meshBone.OffsetMatrix * matrix;
            }
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
        _shaderParameters ??= new();

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