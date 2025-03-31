#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Game.World3D;

public partial class GameObject3D : BaseGameObject
{
    /// <summary>
    /// The current visual of this object.
    /// </summary>
    [DontSerialize]
    public MeshEntity? Entity
    {
        get => _entity;
        set
        {
            lock (this)
            {
                _entity = value;
                EntityMetaState = new MeshEntityMetaState(value)
                {
                    Tint = Tint
                };
                OnSetEntity();
            }
        }
    }

    private MeshEntity? _entity;

    /// <summary>
    /// Entity related state for this particular object.
    /// </summary>
    public MeshEntityMetaState? EntityMetaState { get; private set; }

    /// <inheritdoc />
    public override Color Tint
    {
        get => base.Tint;
        set
        {
            base.Tint = value;
            if (EntityMetaState != null) EntityMetaState.Tint = value;
        }
    }

    /// <summary>
    /// The name of the current animation playing (if any).
    /// </summary>
    public string CurrentAnimation
    {
        get => _currentAnimation?.Name ?? "None";
    }

    /// <summary>
    /// Sphere that encompasses the whole object.
    /// If the mesh is animated the sphere encompasses all keyframes of the animation.
    /// </summary>
    public Sphere BoundingSphere
    {
        get => _bSphereBase.Transform(GetModelMatrix());
    }

    /// <summary>
    /// Axis aligned cube that encompasses the whole object.
    /// If the mesh is animated the AABB encompasses all keyframes of the animation.
    /// </summary>
    public Cube Bounds3D
    {
        get => _bCubeBase.Transform(GetModelMatrix());
    }

    /// <summary>
    /// Axis aligned cube that encompasses the whole object.
    /// </summary>
    public Cube GetBounds3DForAnimation(string? animationName, bool ignoreRotation = false)
    {
        if (Entity == null) return new Cube();
        Entity.GetBounds(animationName, out Sphere _, out Cube c);
        return c.Transform(GetModelMatrix(ignoreRotation));
    }

    protected Sphere _bSphereBase;
    protected Cube _bCubeBase;

    private SkeletalAnimation? _currentAnimation;
    private float _time;
    private Matrix4x4[][]? _boneMatricesPerMesh;

    private const int MAX_BONES = 126; // Must match number in SkeletalAnim.vert

    public GameObject3D(string name) : base(name)
    {
        Entity = Cube.GetEntity();
        EntityMetaState!.Scale = 10f;

        _sizeX = 1;
        _sizeY = 1;
        _sizeZ = 1;
        Resized();
        Moved();
        Rotated();
    }

    // Serialization constructor.
    protected GameObject3D()
    {
        Entity = Cube.GetEntity();
        EntityMetaState!.Scale = 10f;

        _sizeX = 1;
        _sizeY = 1;
        _sizeZ = 1;
        Resized();
        Moved();
        Rotated();
    }

    protected override void UpdateInternal(float dt)
    {
        lock (this)
        {
            _time += dt;
            //_entity?.CalculateBoneMatrices(_currentAnimation, _boneMatricesPerMesh, _time % _currentAnimation?.Duration ?? 0);
        }

        base.UpdateInternal(dt);
    }

    /// <inheritdoc />
    protected override void RenderInternal(RenderComposer c)
    {
        MeshEntityMetaState? metaState = EntityMetaState;
        if (metaState == null) return;

        // Rendered by the map as part of the scene.
        if (c.MeshEntityRenderer.IsGatheringObjectsForScene())
        {
            c.MeshEntityRenderer.SubmitObjectForRendering(this, metaState);
            return;
        }

        // Rendered by something else, such as UI
        MeshEntity? entity = _entity;
        Mesh[]? meshes = entity?.Meshes;
        if (entity == null || meshes == null) return;

        c.PushModelMatrix(GetModelMatrix());
        c.MeshEntityRenderer.RenderMeshEntityStandalone(entity, metaState, _boneMatricesPerMesh, Map is Map3D map3d ? map3d.LightModel : null, ObjectFlags);
        c.PopModelMatrix();
    }

    public Matrix4x4[]? GetBoneMatricesForMesh(int meshIdx)
    {
        if (_entity == null) return null;
        if (_entity.Meshes == null || _entity.Meshes.Length == 0) return null;
        if (_boneMatricesPerMesh == null) return null;
        if (meshIdx >= _boneMatricesPerMesh.Length) return null;

        return _boneMatricesPerMesh[meshIdx];
    }

    protected virtual void OnSetEntity()
    {
        if (_entity?.Meshes == null) return;

        // Create bone matrices for the meshes (if they have bones)
        _boneMatricesPerMesh = new Matrix4x4[_entity.Meshes.Length][];

        //for (var i = 0; i < _entity.Meshes.Length; i++)
        //{
        //    Mesh mesh = _entity.Meshes[i];
        //    var boneCount = 1; // idx 0 is identity
        //    if (mesh.Bones != null)
        //    {
        //        boneCount += mesh.Bones.Length;
        //        if (boneCount > MAX_BONES)
        //        {
        //            Engine.Log.Error($"Entity {_entity.Name}'s mesh {mesh.Name} has too many bones ({boneCount} > {MAX_BONES}).", "3D");
        //            boneCount = MAX_BONES;
        //        }
        //    }

        //    var boneMats = new Matrix4x4[boneCount];
        //    boneMats[0] = Matrix4x4.Identity;
        //    _boneMatricesPerMesh[i] = boneMats;
        //}

        _verticesCacheCollision = null;

        // Update unit scale.
        Resized();

        // Reset the animation.
        // This will also set the default bone matrices.
        // This will also calculate bounds (if missing - applicable for non em3 entities).
        // This will also calculate the vertices collisions.
        SetAnimation(null);
    }

    public virtual void SetAnimation(string? name)
    {
        MeshEntity? entity = _entity;
        if (entity?.Meshes == null)
        {
            _currentAnimation = null;
            _time = 0;
            return;
        }

        // Try to find the animation instance.
        SkeletalAnimation? animInstance = null;
        if (entity.Animations != null)
            for (var i = 0; i < entity.Animations.Length; i++)
            {
                SkeletalAnimation anim = entity.Animations[i];
                if (anim.Name == name) animInstance = anim;
            }

        _currentAnimation = animInstance;
        _time = 0;

        // todo: add some way for the entity to calculate and hold a collision mesh.
        //_entity?.CalculateBoneMatrices(_currentAnimation, _boneMatricesPerMesh, 0);
        CacheVerticesForCollision();
        _entity?.GetBounds(name, out _bSphereBase, out _bCubeBase);
        Assert(_bSphereBase.Radius != 0, "Bounding radius is not 0");
    }

    public virtual bool IsTransparent()
    {
        return Tint.A != 255;
    }

    #region Collision

    private Vector3[]?[]? _verticesCacheCollision;

    public void CacheVerticesForCollision(bool reuseMeshData = true)
    {
        //Mesh[]? meshes = _entity?.Meshes;
        //if (meshes == null) return;

        //if (!reuseMeshData) _verticesCacheCollision = null;
        //_verticesCacheCollision ??= new Vector3[meshes.Length][];
        //for (var m = 0; m < meshes.Length; m++)
        //{
        //    Mesh mesh = meshes[m];
        //    if (mesh.BoneData == null) continue;

        //    VertexData[] vertices = mesh.Vertices;
        //    Vector3[] thisMesh;
        //    if (_verticesCacheCollision[m] != null)
        //    {
        //        thisMesh = _verticesCacheCollision[m]!;
        //    }
        //    else
        //    {
        //        thisMesh = new Vector3[vertices.Length];
        //        _verticesCacheCollision[m] = thisMesh;
        //    }

        //    Mesh3DVertexDataBones[]? boneData = mesh.BoneData;
        //    if (boneData != null)
        //    {
        //        Matrix4x4[] bonesForThisMesh = _boneMatricesPerMesh![m];

        //        for (var vertexIdx = 0; vertexIdx < boneData.Length; vertexIdx++)
        //        {
        //            ref Mesh3DVertexDataBones vertexDataBones = ref boneData[vertexIdx];
        //            ref Vector3 vertex = ref vertices[vertexIdx].Vertex;

        //            Vector3 vertexTransformed = Vector3.Zero;
        //            for (var w = 0; w < 4; w++)
        //            {
        //                float boneId = vertexDataBones.BoneIds[w];
        //                float weight = vertexDataBones.BoneWeights[w];

        //                Matrix4x4 boneMat = bonesForThisMesh[(int) boneId];
        //                Vector3 thisWeightPos = Vector3.Transform(vertex, boneMat);
        //                vertexTransformed += thisWeightPos * weight;
        //            }

        //            thisMesh[vertexIdx] = vertexTransformed;
        //        }
        //    }
        //    else
        //    {
        //        for (var vertexIdx = 0; vertexIdx < vertices.Length; vertexIdx++)
        //        {
        //            thisMesh[vertexIdx] = vertices[vertexIdx].Vertex;
        //        }
        //    }
        //}
    }

    public void GetMeshTriangleForCollision(int meshIdx, int v1, int v2, int v3, out Vector3 vert1, out Vector3 vert2, out Vector3 vert3)
    {
        vert1 = Vector3.Zero;
        vert2 = Vector3.Zero;
        vert3 = Vector3.Zero;

        //Mesh[]? meshes = _entity?.Meshes;
        //if (meshes == null) return;

        //var mesh = meshes[meshIdx];
        //if (mesh.BoneData == null)
        //{
        //    var vertices = mesh.Vertices;
        //    vert1 = vertices[v1].Vertex;
        //    vert2 = vertices[v2].Vertex;
        //    vert3 = vertices[v3].Vertex;
        //    return;
        //}

        //Vector3[]? meshData = _verticesCacheCollision?[meshIdx];
        //if (meshData == null) return; // todo: maybe fallback to the vertices?

        //vert1 = meshData[v1];
        //vert2 = meshData[v2];
        //vert3 = meshData[v3];
    }

    #endregion

    public void RotateZToFacePoint(Vector3 pt)
    {
        Vector3 forward = Entity?.Forward ?? Vector3.UnitX;

        Vector3 direction = Vector3.Normalize(Position - pt);
        float angle = MathF.Atan2(direction.Y, direction.X) + MathF.Atan2(forward.Y, forward.X);

        Vector3 rotation = Rotation;
        rotation.Z = angle;
        Rotation = rotation;
    }
}