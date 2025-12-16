#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.IO;
using Emotion.Game.Systems.Animation.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;

#endregion

namespace Emotion.Game.World.ThreeDee;

/// <summary>
/// Holds runtime state information referring to a mesh entity.
/// This is information that only applies to the particular instance of the
/// entity is use by the object, but not pertaining to the object itself.
/// </summary>
[DontSerialize]
public class MeshEntityMetaState
{
    public Matrix4x4 ModelMatrix = Matrix4x4.Identity;

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

    private MeshEntity _entity;
    private Matrix4x4[] _boneMatricesForEntityRig;
    private Matrix4x4[][] _boneMatricesPerMesh;

    protected const int MAX_BONES = 200; // Must match number in MeshShader.vert

    public MeshEntityMetaState(MeshEntity entity)
    {
        _entity = entity;
        ModelMatrix = entity.LocalTransform;

        RenderMesh = new bool[entity.Meshes.Length];

        _boneMatricesForEntityRig = new Matrix4x4[entity.AnimationRig.Length];

        // todo: replace this with bone matrices per animation skin when we push them to a UBO
        _boneMatricesPerMesh = new Matrix4x4[entity.Meshes.Length][];

        // Build a mapping of which bones a mesh uses.
        for (int meshIdx = 0; meshIdx < entity.Meshes.Length; meshIdx++)
        {
            Mesh mesh = entity.Meshes[meshIdx];
            RenderMesh[meshIdx] = true;

            Matrix4x4[] matrices;
            if (mesh.BoneData != null)
            {
                int largestBoneIdUsed = 0;
                for (int j = 0; j < mesh.BoneData.Length; j++)
                {
                    Mesh3DVertexDataBones data = mesh.BoneData[j];
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

                // Assimp hack
                // Note: Models loaded by assimp have one skin per mesh and the skins are generally
                // optimized to only include the bones used by the mesh.
                // Should we do this for the GLTF ourselves?
                SkeletalAnimationSkin skin = _entity.AnimationSkins[mesh.AnimationSkin];
                if (skin.Joints.Length < largestBoneIdUsed) largestBoneIdUsed = skin.Joints.Length;

                matrices = new Matrix4x4[largestBoneIdUsed];
            }
            else
            {
                matrices = [Matrix4x4.Identity];
            }
            _boneMatricesPerMesh[meshIdx] = matrices;
        }
    }

    public Matrix4x4[] GetBoneMatricesForMesh(int meshIdx)
    {
        if (meshIdx >= _boneMatricesPerMesh.Length) return Array.Empty<Matrix4x4>();
        return _boneMatricesPerMesh[meshIdx];
    }

    public Matrix4x4 GetMatrixForAnimationRigNode(int nodeId)
    {
        return _boneMatricesForEntityRig[nodeId];
    }

    public void UpdateAnimationRigBones(SkeletalAnimation? animation, float timeStamp)
    {
        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        Matrix4x4[] animRigMatrices = _boneMatricesForEntityRig;
        if (animRig.Length != 0)
            Assert(animRigMatrices.Length == animRig.Length);

        for (var nodeIdx = 0; nodeIdx < animRig.Length; nodeIdx++)
        {
            SkeletonAnimRigNode node = animRig[nodeIdx];
            Matrix4x4 currentMatrix = node.LocalTransform;

            if (animation != null)
            {
                if (node.DontAnimate)
                {
                    currentMatrix = Matrix4x4.Identity;
                }
                else
                {
                    // note: not every bone is moved by the animation
                    SkeletonAnimChannel? channel = animation.GetAnimChannelForRigNode(nodeIdx);
                    if (channel != null)
                    {
                        Matrix4x4 animMatrix = channel.GetMatrixAtTimestamp(timeStamp);

                        // When before the start of the animation.
                        if (!animMatrix.IsIdentity) currentMatrix = animMatrix;
                    }
                }
            }

            Matrix4x4 parentMatrix = Matrix4x4.Identity;
            int parentIdx = node.ParentIdx;
            if (parentIdx != -1)
                parentMatrix = animRigMatrices[parentIdx];

            Matrix4x4 matrixForNode = currentMatrix * parentMatrix;
            animRigMatrices[nodeIdx] = matrixForNode;
        }

        if (_entity.AnimationSkins.Length > 0)
        {
            for (int meshIdx = 0; meshIdx < _boneMatricesPerMesh.Length; meshIdx++)
            {
                Mesh mesh = _entity.Meshes[meshIdx];
                int skinIdx = mesh.AnimationSkin;
                SkeletalAnimationSkin primarySkin = _entity.AnimationSkins[skinIdx];
                SkeletalAnimationSkinJoint[] joints = primarySkin.Joints;

                Matrix4x4[] boneMatricesForMesh = _boneMatricesPerMesh[meshIdx];
                for (int b = 0; b < boneMatricesForMesh.Length; b++)
                {
                    SkeletalAnimationSkinJoint joint = joints[b];
                    int rigBoneId = joint.RigNodeIdx;
                    Matrix4x4 matrix = animRigMatrices[rigBoneId];
                    boneMatricesForMesh[b] = joint.OffsetMatrix * matrix;
                }
            }
        }
    }

    public Task SetShader(string path)
    {
        // todo: wtf
        _shaderParameters = new();
        ShaderAsset = Engine.AssetLoader.Get<ShaderAsset>(path);
        return Task.CompletedTask;
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