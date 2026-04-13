#nullable enable

#region Using

using Emotion.Game.Systems.Animation.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Game.World.ThreeDee;

public struct PerMeshState
{
    public bool RenderMesh;
    public MeshMaterial? OverrideMaterial;
}

/// <summary>
/// Holds runtime state information referring to a mesh entity.
/// This is information that only applies to the particular instance of the
/// entity is use by the object, but not pertaining to the object itself.
/// </summary>
[DontSerialize]
public class MeshEntityMetaState
{
    public MeshEntity Entity { get => _entity; }

    public Matrix4x4 ModelMatrix = Matrix4x4.Identity;

    public PerMeshState[] PerMeshState = Array.Empty<PerMeshState>();

    /// <summary>
    /// Additional scale for all dimensions of the entity.
    /// </summary>
    public float Scale = 1f;

    /// <summary>
    /// A color to tint all vertices of the entity in.
    /// Is multiplied by the material color.
    /// </summary>
    public Color Tint = Color.White;

    //public ShaderAsset? ShaderAsset { get; private set; }
    //private Dictionary<string, IMeshMaterialShaderParameter>? _shaderParameters;

    public RenderState? CustomRenderState;

    private MeshEntity _entity;
    private Matrix4x4[] _boneMatricesForEntityRig;
    private Matrix4x4[] _boneMatricesForEntityRigFinal;
    private Matrix4x4[][] _boneMatricesPerSkin;

    protected const int MAX_BONES = 200; // Must match number in MeshShader.vert

    public MeshEntityMetaState(MeshEntity entity)
    {
        _entity = entity;
        ModelMatrix = entity.LocalTransform;

        PerMeshState = new PerMeshState[entity.Meshes.Length];
        for (int i = 0; i < entity.Meshes.Length; i++)
        {
            PerMeshState[i].RenderMesh = true;
        }

        int rigJoints = entity.AnimationRig.Length;
        _boneMatricesForEntityRig = ArrayPool<Matrix4x4>.Shared.Rent(rigJoints);
        _boneMatricesForEntityRigFinal = ArrayPool<Matrix4x4>.Shared.Rent(rigJoints);

        int skinCount = _entity.AnimationSkins.Length;
        _boneMatricesPerSkin = new Matrix4x4[skinCount][];
        for (int i = 0; i < skinCount; i++)
        {
            SkeletalAnimationSkin skin = _entity.AnimationSkins[i];
            _boneMatricesPerSkin[i] = new Matrix4x4[skin.Joints.Length];
        }
    }

    #region Bones and Animation

    public Matrix4x4[] GetBoneMatricesForMesh(int meshIdx)
    {
        Mesh[] meshes = _entity.Meshes;
        if (meshIdx >= meshes.Length) return Array.Empty<Matrix4x4>();
        int skinIdx = meshes[meshIdx].AnimationSkin;
        if (skinIdx >= _boneMatricesPerSkin.Length) return Array.Empty<Matrix4x4>();

        return _boneMatricesPerSkin[skinIdx];
    }

    public Matrix4x4 GetMatrixForAnimationRigNode(int nodeId)
    {
        return _boneMatricesForEntityRig[nodeId];
    }

    public enum AnimationLayerType
    {
        None,
        LoopingAnimation,
        SinglePlayAnimation
    }

    private struct AnimationLayer
    {
        public AnimationLayerType Typ;
        public SkeletalAnimation Anim;
        public float Time;
        public float CrossfadeAtEnd;
    }

    private struct CrossfadeAnimationSnapshot
    {
        public bool Initialized;
        public bool Active;
        public Matrix4x4[] BoneMatrices;
        public float Time;
        public float LayerTimeout;
        public float Factor;
    }

    private struct AnimationLayerCustomJointTransform
    {
        public Matrix4x4 Matrix;

        public Matrix4x4 BlendFrom;
        public float BlendTime;
        public float BlendEnd;

        public readonly Matrix4x4 GetMatrix()
        {
            if (BlendEnd == 0) return Matrix;
            float factor = BlendTime / BlendEnd;
            if (factor >= 1f) return Matrix;
            return LerpBoneMatrix(BlendFrom, Matrix, factor);
        }
    }

    private List<AnimationLayer> _layers = new List<AnimationLayer>(2);
    private CrossfadeAnimationSnapshot _crossfadeSnapshot = new CrossfadeAnimationSnapshot();

    public void AddCrossfadeSnapshot(float time)
    {
        int entityRigLength = _entity.AnimationRig.Length;
        if (!_crossfadeSnapshot.Initialized)
        {
            _crossfadeSnapshot.BoneMatrices = ArrayPool<Matrix4x4>.Shared.Rent(entityRigLength);
            _crossfadeSnapshot.Initialized = true;
        }

        _crossfadeSnapshot.Time = 0;
        _crossfadeSnapshot.Factor = 0;
        _crossfadeSnapshot.LayerTimeout = time;
        _crossfadeSnapshot.Active = true;

        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        Array.Copy(_boneMatricesForEntityRig, _crossfadeSnapshot.BoneMatrices, entityRigLength);
    }

    public void SetAnimationLayerLooping(int index, SkeletalAnimation? animation)
    {
        if (animation == null) return;

        while (index >= _layers.Count)
        {
            _layers.Add(new AnimationLayer());
        }

        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);
        ref AnimationLayer layer = ref layerSpan[index];
        layer.Typ = AnimationLayerType.LoopingAnimation;
        layer.Anim = animation;
        layer.Time = 0;
    }

    public void SetAnimationLayerRunOnce(int index, SkeletalAnimation? animation, float crossfadeAtEnd)
    {
        if (animation == null || animation.Duration == 0) return;

        while (index >= _layers.Count)
        {
            _layers.Add(new AnimationLayer());
        }

        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);
        ref AnimationLayer layer = ref layerSpan[index];
        layer.Typ = AnimationLayerType.SinglePlayAnimation;
        layer.Anim = animation;
        layer.Time = 0;
        layer.CrossfadeAtEnd = crossfadeAtEnd;
    }

    public string GetAnimationLayerName(int index)
    {
        if (index >= _layers.Count) return string.Empty;

        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);
        ref AnimationLayer layer = ref layerSpan[index];
        return layer.Anim?.Name ?? string.Empty;
    }

    public float GetAnimationLayerDuration(int index)
    {
        if (index >= _layers.Count) return 0;

        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);
        ref AnimationLayer layer = ref layerSpan[index];
        if (layer.Anim == null) return 0;
        return layer.Anim.Duration;
    }

    public float GetAnimationLayerFactor(int index)
    {
        if (index >= _layers.Count) return 0;

        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);
        ref AnimationLayer layer = ref layerSpan[index];
        if (layer.Anim == null) return 0;
        return layer.Time / layer.Anim.Duration;
    }

    public void UpdateBoneMatrices(float dt)
    {
        Span<AnimationLayer> layerSpan = CollectionsMarshal.AsSpan(_layers);

        // Update the time of all active layers
        for (int i = 0; i < layerSpan.Length; i++)
        {
            ref AnimationLayer layer = ref layerSpan[i];
            if (layer.Typ == AnimationLayerType.None) continue;

            layer.Time += dt;
            if (layer.Typ == AnimationLayerType.SinglePlayAnimation)
            {
                SkeletalAnimation anim = layer.Anim;
                float duration = anim.Duration;
                layer.Time = Math.Min(layer.Time, duration);
            }
        }

        // Update crossfade
        if (_crossfadeSnapshot.Active)
        {
            _crossfadeSnapshot.Time += dt;
            _crossfadeSnapshot.Factor = Maths.Lerp(0, 1f, _crossfadeSnapshot.Time / _crossfadeSnapshot.LayerTimeout);
        }

        // Apply the top-most active layer
        bool anyAnimationApplied = false;
        for (int i = layerSpan.Length - 1; i >= 0; i--)
        {
            ref AnimationLayer layer = ref layerSpan[i];
            switch (layer.Typ)
            {
                case AnimationLayerType.None:
                    continue;
                case AnimationLayerType.LoopingAnimation:
                case AnimationLayerType.SinglePlayAnimation:
                    SkeletalAnimation anim = layer.Anim;
                    float duration = anim.Duration;
                    float time = layer.Time;
                    if (layer.Typ == AnimationLayerType.LoopingAnimation)
                    {
                        time = duration == 0 ? 0 : time % duration;
                    }
                    ApplyAnimationToRigMatrices(anim, time);
                    anyAnimationApplied = true;
                    break;
            }
            if (anyAnimationApplied) break;
        }

        // No animation - so just set the initial rig offsets.
        if (!anyAnimationApplied)
            ApplyDefaultRigMatrices();

        // Apply crossfading
        if (_crossfadeSnapshot.Active)
            LerpToEntityRigBones(_crossfadeSnapshot.BoneMatrices, _crossfadeSnapshot.Factor);

        // Apply custom bone transforms
        if (_customBoneTransforms != null && _customBoneTransforms.Count > 0)
        {
            SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
            Matrix4x4[] animRigMatrices = _boneMatricesForEntityRig;
            for (int nodeIdx = 0; nodeIdx < animRig.Length; nodeIdx++)
            {
                SkeletonAnimRigNode node = animRig[nodeIdx];
                int parent = node.ParentIdx;
                Matrix4x4 currentMatrix;
                Matrix4x4 parentMatrix;
                if (parent != -1)
                {
                    currentMatrix = _boneMatricesForEntityRig[nodeIdx] * _boneMatricesForEntityRig[parent].Inverted();
                    parentMatrix = _boneMatricesForEntityRigFinal[parent];
                }
                else
                {
                    currentMatrix = _boneMatricesForEntityRig[nodeIdx];
                    parentMatrix = Matrix4x4.Identity;
                }

                currentMatrix = currentMatrix * parentMatrix;

                if (_customBoneTransforms.TryGetValue(nodeIdx, out AnimationLayerCustomJointTransform customJoint))
                {
                    customJoint.BlendTime += dt;
                    _customBoneTransforms[nodeIdx] = customJoint;

                    Matrix4x4 customTrans;
                    float factor = customJoint.BlendTime / customJoint.BlendEnd;
                    if (factor < 1f)
                    {
                        customTrans = LerpBoneMatrix(customJoint.BlendFrom, customJoint.Matrix, factor);
                    }
                    else
                    {
                        customTrans = customJoint.Matrix;
                        if (customTrans.IsIdentity)
                            _customBoneTransforms.Remove(nodeIdx);
                    }

                    currentMatrix = customTrans * currentMatrix;
                }

                _boneMatricesForEntityRigFinal[nodeIdx] = currentMatrix;
            }
        }
        else
        {
            SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
            Array.Copy(_boneMatricesForEntityRig, _boneMatricesForEntityRigFinal, animRig.Length);
        }

        AssignSkinMatrices();

        // Cleanup single player layers
        for (int i = layerSpan.Length - 1; i >= 0; i--)
        {
            ref AnimationLayer layer = ref layerSpan[i];
            if (layer.Typ != AnimationLayerType.SinglePlayAnimation) continue;

            SkeletalAnimation anim = layer.Anim;
            float duration = anim.Duration;
            if (layer.Time == duration)
            {
                layer.Typ = AnimationLayerType.None; // This animation is done
                if (layer.CrossfadeAtEnd != 0)
                    AddCrossfadeSnapshot(layer.CrossfadeAtEnd);
            }
        }

        // Cleaup crossfade
        if (_crossfadeSnapshot.Active && _crossfadeSnapshot.Factor == 1f)
            _crossfadeSnapshot.Active = false;
    }

    private void LerpToEntityRigBones(Matrix4x4[] lerpFrom, float factor)
    {
        int entityRigLength = _entity.AnimationRig.Length;

        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        Matrix4x4[] animRigMatrices = _boneMatricesForEntityRig;

        for (int nodeIdx = 0; nodeIdx < entityRigLength; nodeIdx++)
        {
            SkeletonAnimRigNode node = animRig[nodeIdx];
            animRigMatrices[nodeIdx] = LerpBoneMatrix(lerpFrom[nodeIdx], _boneMatricesForEntityRig[nodeIdx], factor);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Matrix4x4 LerpBoneMatrix(Matrix4x4 from, Matrix4x4 to, float factor)
    {
        Matrix4x4.Decompose(from, out var scale1, out var rot1, out var pos1);
        Matrix4x4.Decompose(to, out var scale2, out var rot2, out var pos2);

        Vector3 blendedPos = Vector3.Lerp(pos1, pos2, factor);
        Quaternion blendedRot = Quaternion.Slerp(rot1, rot2, factor);
        Vector3 blendedScale = Vector3.Lerp(scale1, scale2, factor);

        return Matrix4x4.CreateScale(blendedScale)
            * Matrix4x4.CreateFromQuaternion(blendedRot)
            * Matrix4x4.CreateTranslation(blendedPos);
    }

    private void ApplyDefaultRigMatrices()
    {
        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        Matrix4x4[] animRigMatrices = _boneMatricesForEntityRig;
        for (var nodeIdx = 0; nodeIdx < animRig.Length; nodeIdx++)
        {
            SkeletonAnimRigNode node = animRig[nodeIdx];
            Matrix4x4 currentMatrix = node.LocalTransform;

            Matrix4x4 parentMatrix = Matrix4x4.Identity;
            int parentIdx = node.ParentIdx;
            if (parentIdx != -1)
                parentMatrix = animRigMatrices[parentIdx];

            animRigMatrices[nodeIdx] = currentMatrix * parentMatrix;
        }
    }

    private void ApplyAnimationToRigMatrices(SkeletalAnimation animation, float timeStamp)
    {
        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        Matrix4x4[] animRigMatrices = _boneMatricesForEntityRig;

        for (var nodeIdx = 0; nodeIdx < animRig.Length; nodeIdx++)
        {
            SkeletonAnimRigNode node = animRig[nodeIdx];
            Matrix4x4 currentMatrix = node.LocalTransform;

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

                    // When a channel is invalid or something it could return an identity matrix
                    if (!animMatrix.IsIdentity) currentMatrix = animMatrix;
                }
            }

            Matrix4x4 parentMatrix = Matrix4x4.Identity;
            int parentIdx = node.ParentIdx;
            if (parentIdx != -1)
                parentMatrix = animRigMatrices[parentIdx];

            animRigMatrices[nodeIdx] = currentMatrix * parentMatrix;
        }
    }

    private void AssignSkinMatrices()
    {
        int skinCount = _entity.AnimationSkins.Length;
        for (int i = 0; i < skinCount; i++)
        {
            SkeletalAnimationSkin skin = _entity.AnimationSkins[i];
            SkeletalAnimationSkinJoint[] joints = skin.Joints;

            Matrix4x4[] boneMatricesForSkin = _boneMatricesPerSkin[i];

            for (int j = 0; j < joints.Length; j++)
            {
                SkeletalAnimationSkinJoint joint = joints[j];
                int rigBoneId = joint.RigNodeIdx;
                Matrix4x4 matrix = _boneMatricesForEntityRigFinal[rigBoneId];
                boneMatricesForSkin[j] = joint.OffsetMatrix * matrix;
            }
        }
    }

    private Dictionary<int, AnimationLayerCustomJointTransform>? _customBoneTransforms;

    public void SetCustomTransformForJoint(string jointName, Matrix4x4 matrix, float blendTime = 0)
    {
        int rigIdx = -1;
        SkeletonAnimRigNode[] animRig = _entity.AnimationRig;
        for (int i = 0; i < animRig.Length; i++)
        {
            SkeletonAnimRigNode rigNode = animRig[i];
            if (rigNode.Name == jointName)
            {
                rigIdx = i;
                break;
            }
        }
        if (rigIdx == -1) return;

        _customBoneTransforms ??= new Dictionary<int, AnimationLayerCustomJointTransform>();

        bool exists = _customBoneTransforms.TryGetValue(rigIdx, out AnimationLayerCustomJointTransform previous);
        if (exists && previous.Matrix == matrix)
            return;

        Matrix4x4 oldMat = exists ? previous.GetMatrix() : Matrix4x4.Identity;
        _customBoneTransforms[rigIdx] = new AnimationLayerCustomJointTransform()
        {
            BlendEnd = blendTime,
            BlendTime = 0,
            BlendFrom = oldMat,
            Matrix = matrix
        };
    }

    #endregion

    //private void SetShaderParam<T>(string name, T value) where T : struct
    //{
    //    _shaderParameters ??= new();

    //    if (_shaderParameters.ContainsKey(name))
    //    {
    //        MeshMaterialShaderParameter<T>? param = _shaderParameters[name] as MeshMaterialShaderParameter<T>;
    //        if (param == null) return; // Parameters can't change types.
    //        param.Value = value;
    //    }
    //    else
    //    {
    //        IMeshMaterialShaderParameter? newParam = null;
    //        if (value is float floatVal)
    //            newParam = new MeshMaterialShaderParameterFloat(name, floatVal);
    //        else if (value is Vector2 vec2Val)
    //            newParam = new MeshMaterialShaderParameterVec2(name, vec2Val);
    //        else if (value is Vector3 vec3Val)
    //            newParam = new MeshMaterialShaderParameterVec3(name, vec3Val);
    //        else if (value is Vector4 vec4Val)
    //            newParam = new MeshMaterialShaderParameterVec4(name, vec4Val);
    //        else if (value is Color colorVal)
    //            newParam = new MeshMaterialShaderParameterColor(name, colorVal);
    //        if (newParam == null) return;
    //        _shaderParameters.Add(name, newParam);
    //    }
    //}

    //private void ApplyShaderUniforms(ShaderProgram program)
    //{
    //    if (_shaderParameters == null) return;
    //    foreach ((string paramName, IMeshMaterialShaderParameter param) in _shaderParameters)
    //    {
    //        param.Apply(program, paramName);
    //    }
    //}
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