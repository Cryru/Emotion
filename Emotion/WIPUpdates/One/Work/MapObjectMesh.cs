#nullable enable

using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World3D;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;

namespace Emotion.WIPUpdates.One.Work;

public class MapObjectMesh : MapObject
{
    [DontSerialize]
    public MeshEntity MeshEntity
    {
        get => _entity;
    }
    protected MeshEntity _entity;

    [DontSerialize]
    public MeshEntityMetaState? RenderState;

    public SerializableAsset<MeshAsset>? EntityAsset;

    public MapObjectMesh()
    {
        SetEntity(Cube.GetEntity());
        AssertNotNull(_entity);
    }

    public MapObjectMesh(string entityFile) : this()
    {
        SetEntity(entityFile);
    }

    public MapObjectMesh(MeshEntity entity) : this()
    {
        SetEntity(entity);
    }

    public override void Init()
    {
        base.Init();
    }

    #region Set Entity

    public void SetEntity(string assetPath)
    {
        MeshAsset assetHandle = Engine.AssetLoader.ONE_Get<MeshAsset>(assetPath, this);
        SetEntity(assetHandle);
    }

    public void SetEntity(MeshAsset asset)
    {
        UnloadOldEntityAsset();
        EntityAsset = asset;
        asset.OnLoaded += OnEntityAssetChanged;

        // Loaded inline or already loaded.
        if (asset.Loaded)
        {
            AssertNotNull(asset.Entity);
            OnSetEntity(asset.Entity);
        }
    }

    protected void OnEntityAssetChanged(Asset asset)
    {
        if (asset is not MeshAsset meshAsset) return;
        if (meshAsset.Entity == null) return;
        OnSetEntity(meshAsset.Entity);
    }

    public void SetEntity(MeshEntity entity)
    {
        UnloadOldEntityAsset();
        OnSetEntity(entity);
    }

    protected void UnloadOldEntityAsset()
    {
        MeshAsset? oldHandle = EntityAsset?.Get();
        if (oldHandle != null)
        {
            oldHandle.OnLoaded -= OnEntityAssetChanged;
            Engine.AssetLoader.RemoveReferenceFromAsset(oldHandle, this);
            EntityAsset = null;
        }
    }

    protected void OnSetEntity(MeshEntity entity)
    {
        RenderState = null;

        _currentAnimation = null;
        _animationTime = 0;

        _entity = entity;

        if (_entity != null)
        {
            RenderState = new MeshEntityMetaState(_entity);

            // Reset the animation (or set it to the one set before the entity was loaded).
            // This will also set the default bone matrices.
            // This will also calculate bounds
            SetAnimation(_initSetAnimation);
            _initSetAnimation = null;

            RenderState.UpdateAnimationRigBones(_currentAnimation, 0);
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
    public Sphere BoundingSphere
    {
        get => GetBoundingSphere();
    }

    /// <summary>
    /// Axis aligned cube that encompasses the whole object.
    /// If the mesh is animated the AABB encompasses all keyframes of the animation.
    /// </summary>
    public override Cube BoundingCube
    {
        get => GetBoundingCube();
    }

    public override Rectangle BoundingRect
    {
        get
        {
            Cube boundingCube = GetBoundingCube();

            Rectangle r = new Rectangle();
            r.Size = boundingCube.HalfExtents.ToVec2() * 2f;
            r.Center = boundingCube.Origin.ToVec2();
            return r;
        }
    }

    protected Sphere _boundingSphere;
    protected Sphere _boundingSphereBase;
    protected bool _dirtyBoundingSphere = true;

    private Sphere GetBoundingSphere()
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

    private Cube GetBoundingCube()
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
        _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        _scaleMatrix = Matrix4x4.CreateScale(_scaleX * entityScale, _scaleY * entityScale, _scaleZ * entityScale);

        Matrix4x4 rotMatrix = ignoreRotation ? Matrix4x4.Identity : _rotationMatrix;
        return entityLocalTransform * _scaleMatrix * rotMatrix * _translationMatrix;
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
            for (var i = 0; i < _entity.Animations.Length; i++)
            {
                SkeletalAnimation anim = _entity.Animations[i];
                if (anim.Name == name) return true;
            }
        }
        return false;
    }

    public virtual void SetAnimation(string? name, bool forceIfMissing = false)
    {
        if (_entity == null)
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
        InvalidateModelMatrix();

        Assert(_boundingSphereBase.Radius != 0, "Entity bounds is 0 - no vertices?");
    }

    #endregion

    public override void Update(float dt)
    {
        // Update current animation
        if (_currentAnimation != null && _entity != null && RenderState != null)
        {
            _animationTime += dt;

            float duration = _currentAnimation.Duration;
            RenderState.UpdateAnimationRigBones(_currentAnimation, _animationTime % duration);
            if (_animationTime > duration) _animationTime -= duration;
        }

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

        base.Render(c);
    }

    public void DebugDrawSkeleton(RenderComposer c)
    {
        SkeletonAnimRigNode[]? rig = _entity?.AnimationRig;
        if (rig == null) return;
        if (RenderState == null) return;

        c.SetDepthTest(false);

        CylinderMeshGenerator coneMeshGenerator = new CylinderMeshGenerator
        {
            RadiusTop = 0,
            RadiusBottom = 1.25f,
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
            coneMeshGenerator.Height = height * _scaleZ;

            // Look at params
            Vector3 conePos = parentBonePos;
            Vector3 lookTowards = bonePos;
            Vector3 meshDefaultLook = Vector3.UnitZ;

            // Look at
            Vector3 dir = Vector3.Normalize(lookTowards - conePos);
            Vector3 rotationAxis = Vector3.Cross(meshDefaultLook, dir);
            float rotationAngle = MathF.Acos(Vector3.Dot(meshDefaultLook, dir) / meshDefaultLook.Length() / dir.Length());
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, rotationAngle);

            Mesh coneMesh = coneMeshGenerator.GenerateMesh().TransformMeshVertices(
                   _scaleMatrix.Inverted() *
                   rotationMatrix *
                   Matrix4x4.CreateTranslation(conePos)
               );
            coneMesh.Material = skeletonVisualizationMaterial;
            visualizationMeshes.Add(coneMesh);
        }

        c.PushModelMatrix(GetModelMatrix());
        for (var i = 0; i < visualizationMeshes.Count; i++)
        {
            Mesh mesh = visualizationMeshes[i];
            mesh.Render(c);
        }
        c.PopModelMatrix();

        c.SetDepthTest(true);
    }

    #region Helpers

    public void RotateToFacePoint(Vector3 pt)
    {
        Vector3 forward = _entity?.Forward ?? Vector3.UnitX;

        Vector3 direction = Vector3.Normalize(pt - Position);
        float angle = MathF.Atan2(direction.Y, direction.X) + MathF.Atan2(forward.Y, forward.X);
        if (float.IsNaN(angle)) angle = 0;

        Vector3 rotation = Rotation;
        rotation.Z = angle;
        Rotation = rotation;
    }

    #endregion
}
