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

    private MeshEntity _entity = null!;

    [DontSerialize]
    public MeshEntityMetaState RenderState { get; private set; } = null!;

    public MeshComponent(MeshReference? entity)
    {
        _entityOwner.SetOnChangeCallback(static (_, component) => (component as MeshComponent)?.OnEntityChanged(), this);
        _entityOwner.Set(entity ?? MeshReference.Invalid, true);
    }

    protected MeshComponent()
    {

    }

    public virtual Coroutine? Init(GameObject obj)
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

        _currentAnimation = null;
        _animationTime = 0;

        // Reset the animation (or set it to the one set before the entity was loaded).
        // This will also set the default bone matrices.
        // This will also calculate bounds
        SetAnimation(_initSetAnimation);
        _initSetAnimation = null;

        RenderState.UpdateAnimationRigBones(_currentAnimation, 0);

        Object.InvalidateModelMatrix();
    }

    #endregion

    #region Animation and Bones

    private SkeletalAnimation? _currentAnimation;
    private float _animationTime;
    private string? _initSetAnimation;

    public string GetCurrentAnimation()
    {
        return _currentAnimation?.Name ?? string.Empty;
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

    public virtual void SetAnimation(string? name, bool forceIfMissing = false)
    {
        if (!_componentInitialized)
        {
            _initSetAnimation = name;
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
        if (animInstance == null && name != null && !forceIfMissing)
            return;

        _currentAnimation = animInstance;
        _animationTime = 0; // Reset time

        //CacheVerticesForCollision();

        _entity.GetBounds(null, out Sphere baseSphere, out Cube baseCube);
        _boundingSphereBase = baseSphere;
        _boundingCubeBase = baseCube;
        Object.InvalidateModelMatrix();

        Assert(_boundingSphereBase.Radius != 0, "Entity bounds is 0 - no vertices?");
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
        // Update current animation
        if (_currentAnimation != null && _entity != null && RenderState != null)
        {
            _animationTime += dt;

            float duration = _currentAnimation.Duration;
            RenderState.UpdateAnimationRigBones(_currentAnimation, _animationTime % duration);
            if (_animationTime > duration) _animationTime -= duration;
        }
    }

    public virtual void Render(Renderer r)
    {
        r.MeshEntityRenderer.EnsureAssetsLoaded();

        r.MeshEntityRenderer.StartScene(r);
        r.MeshEntityRenderer.SubmitObjectForRendering(this, _entity, RenderState);
        r.MeshEntityRenderer.EndScene(r, LightModel.DefaultLightModel);

        // Temp normal visualization code
        // ignore :)
        //int maxNormal = 1000;
        //var firstMesh = _entity.Meshes[0];
        //var verts = firstMesh.Vertices;
        //var vertData = firstMesh.ExtraVertexData;
        //for (int i = 0; i < verts.Length; i++)
        //{
        //    Graphics.Data.VertexData vert = verts[i];
        //    Graphics.Data.VertexDataMesh3DExtra extraData = vertData[i];
        //    Vector3 norm = extraData.Normal;
        //    c.RenderLine(vert.Vertex, vert.Vertex + norm * 2.5f, Color.Red, 0.05f);

        //    if (i > maxNormal) break;
        //}
    }

    public void DebugDrawSkeleton(Renderer c)
    {
        SkeletonAnimRigNode[]? rig = _entity?.AnimationRig;
        if (rig == null) return;
        if (RenderState == null) return;

        c.SetDepthTest(false);

        CylinderMeshGenerator coneMeshGenerator = new CylinderMeshGenerator
        {
            RadiusTop = 0,
            RadiusBottom = 0.05f,
            Sides = 4
        };
        List<Mesh> visualizationMeshes = new List<Mesh>();
        MeshMaterial skeletonVisualizationMaterial = new MeshMaterial()
        {
            Name = "Skeleton Visualization Material",
            DiffuseColor = Color.PrettyPink
        };

        for (int i = 0; i < rig.Length; i++)
        {
            SkeletonAnimRigNode rigNode = rig[i];
            Matrix4x4 nodeMatrix = RenderState.GetMatrixForAnimationRigNode(i);
            Vector3 bonePos = Vector3.Transform(Vector3.Zero, nodeMatrix);

            Vector3 parentBonePos = Vector3.Zero;
            int parent = rigNode.ParentIdx;
            if (parent == -1) continue;

            Matrix4x4 parentMatrix = RenderState.GetMatrixForAnimationRigNode(parent);
            parentBonePos = Vector3.Transform(Vector3.Zero, parentMatrix);

            float height = Vector3.Distance(parentBonePos, bonePos);
            if (height == 0) continue;
            coneMeshGenerator.Height = height * Object.ScaleZ;

            // Look at params
            Vector3 conePos = parentBonePos;
            Vector3 lookTowards = bonePos;
            Vector3 meshDefaultLook = Vector3.UnitZ;

            // Look at
            Vector3 dir = Vector3.Normalize(lookTowards - conePos);
            Vector3 rotationAxis = Vector3.Cross(meshDefaultLook, dir);
            float rotationAngle = MathF.Acos(Vector3.Dot(meshDefaultLook, dir) / meshDefaultLook.Length() / dir.Length());
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, rotationAngle);

            Matrix4x4 boneMeshMatrix = Object.GetModelMatrixScale().Inverted() * rotationMatrix * Matrix4x4.CreateTranslation(conePos);
            Mesh coneMesh = coneMeshGenerator.GenerateMesh().TransformMeshVertices(boneMeshMatrix);
            coneMesh.Material = skeletonVisualizationMaterial;
            visualizationMeshes.Add(coneMesh);
        }

        c.PushModelMatrix(Object.GetModelMatrix());
        for (var i = 0; i < visualizationMeshes.Count; i++)
        {
            Mesh mesh = visualizationMeshes[i];
            mesh.Render(c);
        }
        c.PopModelMatrix();

        c.SetUseViewMatrix(false);
        for (int i = 0; i < rig.Length; i++)
        {
            SkeletonAnimRigNode rigNode = rig[i];
            Matrix4x4 nodeMatrix = RenderState.GetMatrixForAnimationRigNode(i);
            Vector3 bonePos = Vector3.Transform(Vector3.Zero, nodeMatrix);

            Vector3 parentBonePos = Vector3.Zero;
            int parent = rigNode.ParentIdx;
            if (parent == -1) continue;

            Matrix4x4 parentMatrix = RenderState.GetMatrixForAnimationRigNode(parent);
            parentBonePos = Vector3.Transform(Vector3.Zero, parentMatrix);

            // Look at params
            Vector3 conePos = parentBonePos;
            Vector3 lookTowards = bonePos;
            Vector3 meshDefaultLook = Vector3.UnitZ;

            // Look at
            Vector3 dir = Vector3.Normalize(lookTowards - conePos);
            Vector3 rotationAxis = Vector3.Cross(meshDefaultLook, dir);
            float rotationAngle = MathF.Acos(Vector3.Dot(meshDefaultLook, dir) / meshDefaultLook.Length() / dir.Length());
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, rotationAngle);

            Matrix4x4 boneMeshMatrix = Object.GetModelMatrixScale().Inverted() * rotationMatrix * Matrix4x4.CreateTranslation(conePos);
            Vector3 screenBonePos = c.Camera.WorldToScreen(Vector3.Transform(Vector3.Zero, boneMeshMatrix * Object.GetModelMatrix())).ToVec3();
            Vector3 screenBonePosLabel = screenBonePos + new Vector3((i % 10) * 10, (i % 10) * 10, 0);
            c.RenderString(screenBonePosLabel, rigNode.Name, 15);
            c.RenderLine(screenBonePos, screenBonePosLabel, Color.White * 0.5f, 1f);
        }

        c.SetUseViewMatrix(true);
        c.SetDepthTest(true);
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
