#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.Animation.ThreeDee;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.Standard.MeshGenerators;

namespace Emotion.Game.World.Components;

public class MeshComponent : IGameObjectComponent, IGameObjectTransformProvider, IRenderableComponent, IUpdateableComponent
{
    public GameObject Object { get; private set; } = null!;
    private bool _componentInitialized = false;

    public MeshEntity Entity { get => _entity; }
    private AssetOwner<MeshAsset, MeshEntity> _entityOwner = new();

    private MeshEntity _entity = Cube.GetEntity();

    [DontSerialize]
    public MeshEntityMetaState RenderState { get; private set; } = null!;

    public MeshComponent(MeshReference? entity)
    {
        _entityOwner.SetOnChangeCallback(static (_, component) => component.OnEntityChanged(), this);
        _entityOwner.Set(entity ?? MeshReference.Invalid, true);
    }

    protected MeshComponent()
    {

    }

    public virtual IRoutineWaiter? Init(GameObject obj)
    {
        Object = obj;
        return _entityOwner.GetCurrentLoading();
    }

    public virtual void Done(GameObject obj)
    {
        _entityOwner.Done();
    }

    #region Entity Setting

    public Coroutine? SetEntity(MeshReference entity)
    {
        return _entityOwner.Set(entity);
    }

    protected virtual void OnEntityChanged()
    {
        MeshEntity? entity = _entityOwner.GetCurrentObject();
        _entity = entity ?? Cube.GetEntity();
        _componentInitialized = true;

        if (Object.Name == GameObject.DEFAULT_OBJECT_NAME)
            Object.Name = _entity.Name;

        RenderState = new MeshEntityMetaState(_entity);

        // Reset the animation (or set it to the one set before the entity was loaded).
        // This will also set the default bone matrices.
        // This will also calculate bounds
        SetAnimation(_initSetAnimation, 0, true, _initSetAnimationCrossfade);
        _initSetAnimation = null;
        _initSetAnimationCrossfade = 0f;

        RenderState.UpdateBoneMatrices(0);

        _entity.GetBounds(null, out Sphere baseSphere, out Cube baseCube);
        _boundingSphereBase = baseSphere;
        _boundingCubeBase = baseCube;
        Object.InvalidateModelMatrix();

        Assert(_boundingSphereBase.Radius != 0, "Entity bounds is 0 - no vertices?");

        Object.InvalidateModelMatrix();
    }

    #endregion

    #region Animation and Bones

    private string? _initSetAnimation;
    private float _initSetAnimationCrossfade;

    public string GetCurrentAnimation()
    {
        return RenderState.GetAnimationLayerName(0);
    }

    public float GetCurrentAnimationDuration()
    {
        return RenderState.GetAnimationLayerFactor(0);
    }

    public float GetCurrentAnimationFactor()
    {
        return RenderState.GetAnimationLayerFactor(0);
    }

    public bool HasAnimation(string name)
    {
        if (_entity.Animations != null)
        {
            foreach (SkeletalAnimation animation in _entity.Animations)
            {
                if (animation.Name == name)
                    return true;
            }
        }
        return false;
    }

    public virtual void SetAnimation(string? name, int layerIdx = 0, bool looping = true, float crossfadeMS = 0f)
    {
        if (!_componentInitialized)
        {
            _initSetAnimation = name;
            _initSetAnimationCrossfade = crossfadeMS;
            return;
        }

        AssertNotNull(_entity.Meshes);

        // Try to find the animation instance.
        // todo: case insensitive?
        SkeletalAnimation? animInstance = null;
        if (name != null && _entity.Animations != null)
        {
            for (var i = 0; i < _entity.Animations.Length; i++)
            {
                SkeletalAnimation anim = _entity.Animations[i];
                if (anim.Name == name) animInstance = anim;
            }
        }
        if (animInstance == null && name != null)
            return;

        // Snapshot current state
        if (crossfadeMS != 0)
            RenderState.AddCrossfadeSnapshot(crossfadeMS);

        // Start new playback state
        if (looping)
            RenderState.SetAnimationLayerLooping(layerIdx, animInstance);
        else
            RenderState.SetAnimationLayerRunOnce(layerIdx, animInstance, crossfadeMS);
    }

    #endregion

    #region Model Matrix

    public Vector3 GetForwardModelSpace()
    {
        return _entity.Forward;
    }

    public void OnModelMatrixInvalidated()
    {
        _dirtyBoundingSphere = true;
        _dirtyBoundingCube = true;
    }

    public Matrix4x4 CalculateModelMatrix(GameObject obj, out Matrix4x4 scaleMatrix, out Matrix4x4 rotationMatrix, out Matrix4x4 translationMatrix)
    {
        if (RenderState == null)
        {
            // Not initialized yet - happens in race conidtions :/
            scaleMatrix = Matrix4x4.Identity;
            rotationMatrix = Matrix4x4.Identity;
            translationMatrix = Matrix4x4.Identity;
            return Matrix4x4.Identity;
        }

        float entityScale = _entity.Scale * RenderState.Scale;
        Matrix4x4 entityLocalTransform = _entity.LocalTransform;

        Vector3 objScale = obj.Scale3D;
        scaleMatrix = Matrix4x4.CreateScale(objScale.X * entityScale, objScale.Y * entityScale, objScale.Z * entityScale);

        Vector3 objRotation = obj.Rotation;
        rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(objRotation.Y, objRotation.X, objRotation.Z);

        translationMatrix = Matrix4x4.CreateTranslation(obj.Position3D);

        Matrix4x4 newCalculatedModelMatrix = entityLocalTransform * scaleMatrix * rotationMatrix * translationMatrix;
        RenderState.ModelMatrix = newCalculatedModelMatrix;

        return newCalculatedModelMatrix;
    }

    public Rectangle GetBoundingRect(GameObject obj)
    {
        Cube boundingCube = GetBoundingCube(obj);

        Rectangle r = new Rectangle();
        r.Size = boundingCube.HalfExtents.ToVec2() * 2f;
        r.Center = boundingCube.Origin.ToVec2();
        return r;
    }

    protected Sphere _boundingSphere;
    protected Sphere _boundingSphereBase;
    protected bool _dirtyBoundingSphere = true;

    public Sphere GetBoundingSphere(GameObject obj)
    {
        if (_dirtyBoundingSphere)
        {
            _boundingSphere = _boundingSphereBase.Transform(obj.GetModelMatrix());
            _dirtyBoundingSphere = false;
        }

        return _boundingSphere;
    }

    protected Cube _boundingCube;
    protected Cube _boundingCubeBase;
    protected bool _dirtyBoundingCube = true;

    public Cube GetBoundingCube(GameObject obj)
    {
        if (_dirtyBoundingCube)
        {
            _boundingCube = _boundingCubeBase.Transform(obj.GetModelMatrix());
            _dirtyBoundingCube = false;
        }

        return _boundingCube;
    }

    #endregion

    public void Update(float dt)
    {
        if (_entity == null || RenderState == null) return;
        RenderState.UpdateBoneMatrices(dt);
    }

    public virtual void Render(Renderer r)
    {
        r.MeshEntityRenderer.AddToCurrentScene(r, RenderState);
    }

    #region Collision

    public bool IntersectRay(Ray3D ray,
        out Mesh? collidedMesh,
        out Vector3 collisionPoint,
        out Vector3 normal,
        out int triangleIndex,
        bool closestMesh = false
    )
    {
        collidedMesh = null;
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        // Optimize mesh collider check with a sphere collider first.
        Sphere boundSphere = GetBoundingSphere(Object);
        if (!ray.IntersectWithSphere(boundSphere, out Vector3 _, out Vector3 _)) return false;

        Mesh[] meshes = Entity.Meshes;
        if (meshes == null) return false;

        float closestDist = float.MaxValue;
        for (var i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            if (!IntersectRayWithMesh(ray, mesh, out Vector3 meshCollisionPoint, out Vector3 collisionNormal, out int collisionTriIndex)) continue;

            if (closestMesh)
            {
                float distance = Vector3.DistanceSquared(ray.Start, collisionPoint);
                if (closestDist == float.MaxValue || distance < closestDist)
                {
                    closestDist = distance;
                    collidedMesh = mesh;

                    collisionPoint = meshCollisionPoint;
                    normal = collisionNormal;
                    triangleIndex = collisionTriIndex;
                }
            }
            else
            {
                collisionPoint = meshCollisionPoint;
                normal = collisionNormal;
                triangleIndex = collisionTriIndex;

                collidedMesh = mesh;
                break;
            }
        }

        return collidedMesh != null;
    }

    private bool IntersectRayWithMesh(Ray3D ray, Mesh mesh, out Vector3 collisionPoint, out Vector3 collisionNormal, out int collisionTriIndex)
    {
        collisionPoint = Vector3.Zero;
        collisionNormal = Vector3.Zero;
        collisionTriIndex = -1;

        if (mesh.BoneData != null)
        {
            return IntersectRayWithMesh_Animation(ray, mesh, out collisionPoint, out collisionNormal, out collisionTriIndex);
        }

        return IntersectRayWithMesh_NoAnimation(ray, mesh, out collisionPoint, out collisionNormal, out collisionTriIndex);
    }

    private bool IntersectRayWithMesh_Animation(Ray3D ray, Mesh mesh, out Vector3 collisionPoint, out Vector3 collisionNormal, out int collisionTriIndex)
    {
        collisionPoint = Vector3.Zero;
        collisionNormal = Vector3.Zero;
        collisionTriIndex = -1;

        int meshIdx = Entity.Meshes.IndexOf(mesh);
        Matrix4x4[] boneMatrices = RenderState.GetBoneMatricesForMesh(meshIdx);

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        ushort[] meshIndices = mesh.Indices;
        VertexData[] verts = mesh.Vertices;
        Mesh3DVertexDataBones[]? boneData = mesh.BoneData;
        AssertNotNull(boneData);

        Matrix4x4 modelMatrixInverse = RenderState.ModelMatrix.Inverted();

        // Tranform the ray to the inverse of the model matrix so we don't have to transform
        // each vertex of the mesh.
        Ray3D transformedRay = new Ray3D(
            Vector3.Transform(ray.Start, modelMatrixInverse),
            Vector3.TransformNormal(ray.Direction, modelMatrixInverse)
        );
        static Vector3 CalcVertexAnimatedPos(Vector3 vert, Mesh3DVertexDataBones boneData, Matrix4x4[] boneMatrices)
        {
            Matrix4x4 matTotal = Matrix4x4.Identity;
            for (int i = 0; i < 4; i++)
            {
                float bone = boneData.BoneIds[i];
                Matrix4x4 mat = boneMatrices[(int)bone];
                float weight = boneData.BoneWeights[i];

                if (i == 0)
                    matTotal = mat * weight;
                else
                    matTotal = matTotal + mat * weight;
            }

            return Vector3.Transform(vert, matTotal);
        }

        for (var i = 0; i < meshIndices.Length; i += 3)
        {
            ushort idx1 = meshIndices[i];
            ushort idx2 = meshIndices[i + 1];
            ushort idx3 = meshIndices[i + 2];

            Vector3 v1 = verts[idx1].Vertex;
            Vector3 v2 = verts[idx2].Vertex;
            Vector3 v3 = verts[idx3].Vertex;

            Mesh3DVertexDataBones boneData1 = boneData[idx1];
            v1 = CalcVertexAnimatedPos(v1, boneData1, boneMatrices);

            Mesh3DVertexDataBones boneData2 = boneData[idx2];
            v2 = CalcVertexAnimatedPos(v2, boneData2, boneMatrices);

            Mesh3DVertexDataBones boneData3 = boneData[idx3];
            v3 = CalcVertexAnimatedPos(v3, boneData3, boneMatrices);

            Triangle tri = new Triangle(v1, v2, v3);
            if (!transformedRay.IntersectsTriangle(tri, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                collisionNormal = tri.Normal;
                collisionTriIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = ray.Start + ray.Direction * closestDistance;

        return intersectionFound;
    }

    private bool IntersectRayWithMesh_NoAnimation(Ray3D ray, Mesh mesh, out Vector3 collisionPoint, out Vector3 collisionNormal, out int collisionTriIndex)
    {
        collisionPoint = Vector3.Zero;
        collisionNormal = Vector3.Zero;
        collisionTriIndex = -1;

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        ushort[] meshIndices = mesh.Indices;
        VertexData[] verts = mesh.Vertices;

        Matrix4x4 modelMatrixInverse = RenderState.ModelMatrix.Inverted();

        // Tranform the ray to the inverse of the model matrix so we don't have to transform
        // each vertex of the mesh.
        Ray3D transformedRay = new Ray3D(
            Vector3.Transform(ray.Start, modelMatrixInverse),
            Vector3.TransformNormal(ray.Direction, modelMatrixInverse)
        );

        for (var i = 0; i < meshIndices.Length; i += 3)
        {
            ushort idx1 = meshIndices[i];
            ushort idx2 = meshIndices[i + 1];
            ushort idx3 = meshIndices[i + 2];

            Vector3 v1 = verts[idx1].Vertex;
            Vector3 v2 = verts[idx2].Vertex;
            Vector3 v3 = verts[idx3].Vertex;

            Triangle tri = new Triangle(v1, v2, v3);
            if (!transformedRay.IntersectsTriangle(tri, out float t)) continue;

            if (t < closestDistance)
            {
                closestDistance = t;
                collisionNormal = tri.Normal;
                collisionTriIndex = i;
                intersectionFound = true;
            }
        }

        if (intersectionFound) collisionPoint = ray.Start + ray.Direction * closestDistance;

        return intersectionFound;
    }

    #endregion
}
