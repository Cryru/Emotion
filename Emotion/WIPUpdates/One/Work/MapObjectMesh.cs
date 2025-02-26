#nullable enable

using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World3D;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;

namespace Emotion.WIPUpdates.One.Work;

public class MapObjectMesh : MapObject
{
    [DontSerialize]
    public MeshEntity? MeshEntity
    {
        get => _entity;
    }

    protected MeshEntity? _entity;

    [DontSerialize]
    public MeshEntityMetaState? RenderState;

    public string? EntityFilename;
    protected AssetHandle<MeshAsset>? _assetHandle;

    public MapObjectMesh(string? entityFile)
    {
        EntityFilename = entityFile;
    }

    public MapObjectMesh(MeshEntity? entity)
    {
        SetEntity(entity ?? Cube.GetEntity());
    }

    // serialization constructor
    protected MapObjectMesh()
    {

    }

    public override void Init()
    {
        base.Init();

        if (EntityFilename != null)
            SetEntity(EntityFilename);
    }

    #region Set Entity

    public void SetEntity(string assetPath)
    {
        UnloadOldAssetHandle();
        EntityFilename = assetPath;
        _assetHandle = Engine.AssetLoader.ONE_Get<MeshAsset>(assetPath, this);
        _assetHandle.OnAssetLoaded += OnEntityAssetChanged;
    }

    protected void OnEntityAssetChanged(MeshAsset asset)
    {
        if (asset == null) return;
        OnSetEntity(asset.Entity);
    }

    public void SetEntity(MeshEntity entity)
    {
        UnloadOldAssetHandle();
        OnSetEntity(entity);
    }

    protected void UnloadOldAssetHandle()
    {
        EntityFilename = null;
        if (_assetHandle != null)
        {
            _assetHandle.OnAssetLoaded -= OnEntityAssetChanged;
            Engine.AssetLoader.RemoveReferenceFromAssetHandle(_assetHandle, this);
            _assetHandle = null;
        }
    }

    protected void OnSetEntity(MeshEntity? entity)
    {
        RenderState = null;

        _boneMatricesPerMesh = null;
        _currentAnimation = null;
        _time = 0;

        _entity = entity;

        if (_entity != null)
        {
            RenderState = new MeshEntityMetaState(_entity);
            if (_entity.Meshes != null)
            {
                CacheBoneMatrices();
            }
            else
            {
                _entity.GetBounds(null, out Sphere baseSphere, out Cube baseCube);
                _boundingSphereBase = baseSphere;
                _boundingCubeBase = baseCube;
            }
        }
        else
        {
            _boundingSphereBase = new Sphere();
            _boundingCubeBase = new Cube();
        }

        InvalidateModelMatrix();
    }

    #endregion

    #region Transform

    /// <summary>
    /// Sphere that encompasses the whole object.
    /// If the mesh is animated the sphere encompasses all keyframes of the animation.
    /// </summary>
    public Sphere BoundingSphere;

    /// <summary>
    /// Axis aligned cube that encompasses the whole object.
    /// If the mesh is animated the AABB encompasses all keyframes of the animation.
    /// </summary>
    public Cube Bounds3D;

    public override Rectangle Bounds2D
    {
        get
        {
            Cube boundingCube = GetBoundingCube();

            Rectangle r = new Rectangle();
            r.Size = boundingCube.HalfExtents.ToVec2() * 2f;
            r.Center = boundingCube.Origin.ToVec2();
            return r;
        }
        set
        {
            Position2D = value.Center; // ?
            Assert(false, "Settings Bounds2D on a 3D object? Not sure what should happen");
        }
    }

    protected Sphere _boundingSphere;
    protected Sphere _boundingSphereBase;
    protected bool _dirtyBoundingSphere = true;

    public Sphere GetBoundingSphere()
    {
        if (_dirtyBoundingSphere)
        {
            _boundingSphere = _boundingSphereBase.Transform(GetModelMatrix());
            _dirtyBoundingSphere = false;
        }

        return _boundingSphere;
    }

    protected Cube _boundingCube;
    protected Cube _boundingCubeBase;
    protected bool _dirtyBoundingCube = true;

    public Cube GetBoundingCube()
    {
        if (_dirtyBoundingCube)
        {
            _boundingCube = _boundingCubeBase.Transform(GetModelMatrix());
            _dirtyBoundingCube = false;
        }

        return _boundingCube;
    }

    protected override void InvalidateModelMatrix()
    {
        base.InvalidateModelMatrix();
        _dirtyBoundingSphere = true;
        _dirtyBoundingCube = true;
    }

    protected override Matrix4x4 UpdateModelMatrix()
    {
        bool ignoreRotation = false;

        float entityScale = 1f;
        Matrix4x4 entityLocalTransform = Matrix4x4.Identity;

        if (_entity != null && RenderState != null)
        {
            entityScale = _entity.Scale * RenderState.Scale;
            entityLocalTransform = _entity.LocalTransform;
        }

        _translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
        _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Z, _rotation.Y, _rotation.X);
        _scaleMatrix = Matrix4x4.CreateScale(_sizeX * entityScale, _sizeY * entityScale, _sizeZ * entityScale);

        Matrix4x4 rotMatrix = ignoreRotation ? Matrix4x4.Identity : _rotationMatrix;
        return _scaleMatrix * rotMatrix * entityLocalTransform * _translationMatrix;
    }

    #endregion

    #region Animation and Bones

    protected const int MAX_BONES = 200; // Must match number in SkeletalAnim.vert
    protected Matrix4x4[][]? _boneMatricesPerMesh;
    private SkeletalAnimation? _currentAnimation;
    private float _time;
    private string? _initSetAnimation;

    protected void CacheBoneMatrices()
    {
        AssertNotNull(_entity);
        AssertNotNull(_entity.Meshes);

        // Create bone matrices for the meshes (if they have bones)
        _boneMatricesPerMesh = new Matrix4x4[_entity.Meshes.Length][];

        for (var i = 0; i < _entity.Meshes.Length; i++)
        {
            Mesh mesh = _entity.Meshes[i];
            var boneCount = 1; // idx 0 is identity
            if (mesh.Bones != null)
            {
                boneCount += mesh.Bones.Length;
                if (boneCount > MAX_BONES)
                {
                    Engine.Log.Error($"Entity {_entity.Name}'s mesh {mesh.Name} has too many bones ({boneCount} > {MAX_BONES}).", "3D");
                    boneCount = MAX_BONES;
                }
            }

            var boneMats = new Matrix4x4[boneCount];
            boneMats[0] = Matrix4x4.Identity;
            _boneMatricesPerMesh[i] = boneMats;
        }

        // Reset the animation.
        // This will also set the default bone matrices.
        // This will also calculate bounds (if missing - applicable for non em3 entities).
        // This will also calculate the vertices collisions.
        SetAnimation(_initSetAnimation);
        _initSetAnimation = null;
    }

    public Matrix4x4[] GetBoneMatricesForMesh(int meshIdx)
    {
        if (_boneMatricesPerMesh == null) return Array.Empty<Matrix4x4>();
        if (meshIdx >= _boneMatricesPerMesh.Length) return Array.Empty<Matrix4x4>();
        return _boneMatricesPerMesh[meshIdx];
    }

    public string GetCurrentAnimation()
    {
        return _currentAnimation?.Name ?? string.Empty;
    }

    public virtual void SetAnimation(string? name)
    {
        if (!Initialized)
        {
            _initSetAnimation = name;
            return;
        }

        AssertNotNull(_entity);
        AssertNotNull(_entity.Meshes);

        // Try to find the animation instance.
        SkeletalAnimation? animInstance = null;
        if (_entity.Animations != null)
        {
            for (var i = 0; i < _entity.Animations.Length; i++)
            {
                SkeletalAnimation anim = _entity.Animations[i];
                if (anim.Name == name) animInstance = anim;
            }
        }

        _currentAnimation = animInstance;
        _time = 0; // Reset time

        // todo: add some way for the entity to calculate and hold a collision mesh.
        _entity.CalculateBoneMatrices(_currentAnimation, _boneMatricesPerMesh, 0);
        //CacheVerticesForCollision();

        _entity.GetBounds(null, out Sphere baseSphere, out Cube baseCube);
        _boundingSphereBase = baseSphere;
        _boundingCubeBase = baseCube;

        Assert(_boundingSphereBase.Radius != 0, "Bounding radius is not 0");
    }

    #endregion

    public override void Update(float dt)
    {
        _time += dt;
        _entity?.CalculateBoneMatrices(_currentAnimation, _boneMatricesPerMesh, _time % _currentAnimation?.Duration ?? 0);

        base.Update(dt);
    }

    public override void Render(RenderComposer c)
    {
        if (_entity != null && RenderState != null)
        {
            c.MeshEntityRenderer.EnsureAssetsLoaded();

            c.MeshEntityRenderer.StartScene(c);
            c.MeshEntityRenderer.SubmitObjectForRendering(this, _entity, RenderState);
            c.MeshEntityRenderer.EndScene(c, LightModel.DefaultLightModel);
        }

        base.Render(c);
    }

    public void DebugDrawSkeleton(RenderComposer c)
    {
        SkeletonAnimRigRoot? rig = _entity?.AnimationRig;
        if (rig == null) return;

        var coneMeshGenerator = new CylinderMeshGenerator
        {
            RadiusTop = 0,
            RadiusBottom = 1.25f,
            Sides = 4
        };
        var visualizationMeshes = new List<Mesh>();

        void DrawSkeleton(SkeletonAnimRigNode node, Matrix4x4 parentMatrix, Vector3 parentPos)
        {
            Matrix4x4 currentMatrix = node.LocalTransform;
            if (_currentAnimation != null)
            {
                if (node.DontAnimate)
                {
                    currentMatrix = Matrix4x4.Identity;
                }
                else
                {
                    SkeletonAnimChannel? channel = _currentAnimation.GetMeshAnimBone(node.Name);
                    if (channel != null)
                        currentMatrix = channel.GetMatrixAtTimestamp(_time % _currentAnimation.Duration);
                }
            }

            Matrix4x4 matrix = currentMatrix * parentMatrix;
            Vector3 bonePos = Vector3.Transform(Vector3.Zero, matrix);

            if (parentPos != Vector3.Zero)
            {
                float height = Vector3.Distance(parentPos, bonePos);
                coneMeshGenerator.Height = height * _sizeZ;

                // Look at params
                Vector3 conePos = parentPos;
                Vector3 lookTowards = bonePos;
                Vector3 meshDefaultLook = Vector3.UnitZ;

                // Look at
                Vector3 dir = Vector3.Normalize(lookTowards - conePos);
                Vector3 rotationAxis = Vector3.Cross(meshDefaultLook, dir);
                float rotationAngle = MathF.Acos(Vector3.Dot(meshDefaultLook, dir) / meshDefaultLook.Length() / dir.Length());
                var rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, rotationAngle);

                Mesh coneMesh = coneMeshGenerator.GenerateMesh().TransformMeshVertices(
                    _scaleMatrix.Inverted() *
                    rotationMatrix *
                    Matrix4x4.CreateTranslation(conePos)
                ).ColorMeshVertices(Color.PrettyPink);
                visualizationMeshes.Add(coneMesh);
            }

            SkeletonAnimRigNode[]? children = node.Children;
            if (children == null) return;
            for (var i = 0; i < children.Length; i++)
            {
                SkeletonAnimRigNode child = children[i];
                DrawSkeleton(child, matrix, bonePos);
            }
        }

        DrawSkeleton(rig, Matrix4x4.Identity, Vector3.Zero);

        c.PushModelMatrix(GetModelMatrix());
        for (var i = 0; i < visualizationMeshes.Count; i++)
        {
            Mesh mesh = visualizationMeshes[i];
            mesh.Render(c);
        }

        c.PopModelMatrix();
    }

    #region Helpers

    public void RotateToFacePoint(Vector3 pt)
    {
        Vector3 forward = _entity?.Forward ?? Vector3.UnitX;

        Vector3 direction = Vector3.Normalize(pt - Position);
        float angle = MathF.Atan2(direction.Y, direction.X) + MathF.Atan2(forward.Y, forward.X);
        if (float.IsNaN(angle)) angle = 0;

        Vector3 rotation = Vector3.Zero;
        rotation.Z = angle;
        Rotation = rotation;
    }

    #endregion
}
