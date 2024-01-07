#nullable enable

#region Using

using Emotion;
using Emotion.Common.Threading;
using Emotion.Game.ThreeDee;
using Emotion.Game.World;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches3D;

public sealed class MeshEntityBatchRenderer
{
    private bool _assetsLoaded = false;
    private Comparison<RenderInstanceMeshDataTransparent> _objectComparison = null!;
    private ShaderProgram _meshShader = null!;
    private ShaderProgram _skinnedMeshShader = null!;
    private ShaderProgram _meshShaderShadowMap = null!;
    private ShaderProgram _meshShaderShadowMapSkinned = null!;

    #region Scene State

    // STATE DESCRIPTION
    // ==========================
    // For Each RenderPass:
    // | - MeshRenderPipelineStateBatch (per shader used) via _mainPassShaderGroups
    //     |
    //     (RenderBatchLL_Start)
    //     MeshRenderMeshBatch (per mesh used)
    //     |
    //     (MeshDataLL_Start)
    //     RenderInstanceMesData (per each instance of that mesh in the scene) via _meshDataPool
    //
    // References Resources:
    //      > RenderInstanceObjectData (per each object, referenced by all meshes in it) via _objectDataPool
    //      > Entries in _shadersUsedList (per shader used in the scene) checked via _shadersUsedLookup
    // 

    private bool _inScene;

    private StructArenaAllocator<MeshRenderPipelineStateGroup> _mainPassShaderGroups = new StructArenaAllocator<MeshRenderPipelineStateGroup>(4); // Main
    private StructArenaAllocator<MeshRenderMeshBatch> _renderBatchPool = new(16);
    private StructArenaAllocator<RenderInstanceObjectData> _objectDataPool = new(32);
    private StructArenaAllocator<RenderInstanceMeshData> _meshDataPool = new(64);
    private StructArenaAllocator<RenderInstanceMeshDataTransparent> _meshDataPoolTransparent = new(16);

    private static HashSet<ShaderProgram> _shadersUsedLookup = new HashSet<ShaderProgram>();
    private static List<ShaderProgram> _shadersUsedList = new List<ShaderProgram>();

    private static HashSet<Mesh> _meshesUsedLookup = new HashSet<Mesh>();
    private static List<Mesh> _meshesUsedList = new List<Mesh>();

    // This will be used to index a member of _batchesPerRenderPass based on the shader hash to get the specific group.
    private static Dictionary<int, int> _pipelineBatchLookup = new();
    private static Dictionary<int, int> _meshShaderGroupBatchLookup = new(); // This will be used to index

    private float _furthestObjectDist;
    private float _closestObjectDist;
    private BaseGameObject? _furthestObject;
    private BaseGameObject? _closestObject;

    private int _renderingShadowmap;

    #endregion

    #region GL Data Buffers

    public class GLRenderObjects
    {
        public VertexBuffer VBO;
        public VertexBuffer VBOExtended;
        public VertexBuffer? VBOBones;

        public IndexBuffer IBO;
        public VertexArrayObject VAO;

        //public int LastUsedFramesAgo;

        public GLRenderObjects(VertexBuffer vbo, VertexBuffer vboExt, VertexBuffer? vboBone, IndexBuffer ibo, VertexArrayObject vao)
        {
            VBO = vbo;
            VBOExtended = vboExt;
            VBOBones = vboBone;
            IBO = ibo;
            VAO = vao;
        }
    }

    private Dictionary<Mesh, GLRenderObjects> _meshToRenderObject = new();

    private Stack<GLRenderObjects> _renderObjects = new();
    private Stack<GLRenderObjects> _renderObjectsUsed = new();

    private Stack<GLRenderObjects> _renderObjectsBones = new();
    private Stack<GLRenderObjects> _renderObjectsBonesUsed = new();

    #endregion

    #region Shadows

    public class ShadowCascadeData
    {
        public Vector2 FramebufferResolution = new Vector2(2048);

        public int CascadeId;
        public FrameBuffer? Buffer;
        public Matrix4x4 DEBUG_View;
        public Matrix4x4 LightViewProj;

        public float FarClip;
        public float NearClip;

        public string ViewProjUniformName;
        public string FarZUnifornName;

        public ShadowCascadeData(int cascadeId)
        {
            CascadeId = cascadeId;
            GLThread.ExecuteGLThreadAsync(InitFrameBuffer);
            LightViewProj = Matrix4x4.Identity;
            ViewProjUniformName = $"cascadeLightProj[{cascadeId}]";
            FarZUnifornName = $"cascadePlaneFarZ[{cascadeId}]";
        }

        private void InitFrameBuffer()
        {
            Buffer = new FrameBuffer(FramebufferResolution).WithDepth(true);
            Texture.EnsureBound(Buffer.DepthStencilAttachment.Pointer);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_BORDER);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_BORDER);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, borderColor);
        }
    }

    private ShadowCascadeData[] _shadowCascades = null!;
    private Matrix4x4 _currentCascadeRenderingViewProj = Matrix4x4.Identity;

    #endregion

    private int ObjectSort(RenderInstanceMeshDataTransparent a, RenderInstanceMeshDataTransparent b)
    {
        var objectA = _objectDataPool[a.ObjectRegistrationId];
        var objectB = _objectDataPool[b.ObjectRegistrationId];
        return MathF.Sign(objectA.DistanceToCamera - objectB.DistanceToCamera);
    }

    public void EnsureAssetsLoaded()
    {
        if (_assetsLoaded) return;

        _objectComparison = ObjectSort; // Prevent delegate allocation

        var meshShaderAsset = Engine.AssetLoader.Get<ShaderAsset>("Shaders/MeshShader.xml");
        if (meshShaderAsset != null)
        {
            _meshShader = meshShaderAsset.Shader;
            _skinnedMeshShader = meshShaderAsset.GetShaderVariation("SKINNED").Shader;
            _meshShaderShadowMap = meshShaderAsset.GetShaderVariation("SHADOW_MAP").Shader;
            _meshShaderShadowMapSkinned = meshShaderAsset.GetShaderVariation("SKINNED_SHADOW_MAP").Shader;
        }
        else
        {
            _meshShader = ShaderFactory.DefaultProgram;
            _skinnedMeshShader = ShaderFactory.DefaultProgram;
            _meshShaderShadowMap = ShaderFactory.DefaultProgram;
            _meshShaderShadowMapSkinned = ShaderFactory.DefaultProgram;
        }

        _shadowCascades = new ShadowCascadeData[4]
        {
            new ShadowCascadeData(0),
            new ShadowCascadeData(1),
            new ShadowCascadeData(2),
            new ShadowCascadeData(3),
        };

        _assetsLoaded = true;
    }

    public void StartScene(RenderComposer c)
    {
        c.FlushRenderStream();

        EnsureAssetsLoaded();

        _inScene = true;
        _pipelineBatchLookup.Clear();
        _mainPassShaderGroups.Reset();

        _shadersUsedLookup.Clear();
        _shadersUsedList.Clear();

        _meshesUsedLookup.Clear();
        _meshesUsedList.Clear();

        _meshShaderGroupBatchLookup.Clear();
        _renderBatchPool.Reset();

        _objectDataPool.Reset();
        _meshDataPool.Reset();
        _meshDataPoolTransparent.Reset();

        _furthestObjectDist = 0;
        _closestObjectDist = int.MaxValue;
        _renderingShadowmap = -1;
    }

    public bool IsGatheringObjectsForScene()
    {
        return _inScene;
    }

    // flatten the hierarchy
    public void SubmitObjectForRendering(GameObject3D obj)
    {
        if (!_inScene) return;

        // No visual representation
        var entity = obj.Entity;
        if (entity == null) return;

        // Invalid
        var metaState = obj.EntityMetaState;
        if (metaState == null) return;

        // No meshes to render
        var meshes = entity.Meshes;
        if (meshes == null) return;

        Matrix4x4 objModelMatrix = obj.GetModelMatrix();

        CameraBase camera = Engine.Renderer.Camera;
        Sphere objBSphere = obj.BoundingSphere;
        float distanceToCamera = Vector3.Distance(camera.Position, objBSphere.Origin);

        // Record the furthest and closest object distances.
        float distanceToCameraMax = distanceToCamera + objBSphere.Radius;
        float distanceToCameraMin = distanceToCamera - objBSphere.Radius;
        if (distanceToCameraMax > _furthestObjectDist)
        {
            _furthestObjectDist = distanceToCameraMax;
            _furthestObject = obj;
        }
        if (distanceToCameraMin < _closestObjectDist)
        {
            _closestObjectDist = distanceToCameraMin;
            _closestObject = obj;
        }

        // Objects can contain multiple meshes and they will refer to this.
        ref RenderInstanceObjectData objectInstance = ref _objectDataPool.Allocate(out int indexOfThisObjectData);
        objectInstance.BackfaceCulling = entity.BackFaceCulling;
        objectInstance.ModelMatrix = objModelMatrix;
        objectInstance.MetaState = metaState;
        objectInstance.DistanceToCamera = distanceToCamera;
        objectInstance.Flags = obj.ObjectFlags;

        var objIsTransparent = obj.IsTransparent();

        // Register all meshes in this entity.
        for (int m = 0; m < meshes.Length; m++)
        {
            // Don't render mesh!
            if (!metaState.RenderMesh[m]) continue;

            var mesh = meshes[m];

            if (!_meshesUsedLookup.Contains(mesh))
            {
                _meshesUsedLookup.Add(mesh);
                _meshesUsedList.Add(mesh);
            }

            // Decide on the pipeline state for this mesh.
            bool renderingShadowMap = false;
            bool skinnedMesh = mesh.Bones != null;
            ShaderProgram currentShader;
            bool overwrittenShader = false;
            if (metaState.ShaderAsset != null)
            {
                currentShader = metaState.ShaderAsset.Shader;
                overwrittenShader = true;
            }
            else if (skinnedMesh)
            {
                if (renderingShadowMap)
                    currentShader = _meshShaderShadowMapSkinned;
                else
                    currentShader = _skinnedMeshShader;
            }
            else
            {
                if (renderingShadowMap)
                    currentShader = _meshShaderShadowMap;
                else
                    currentShader = _meshShader;
            }

            if (!_shadersUsedLookup.Contains(currentShader))
            {
                _shadersUsedLookup.Add(currentShader);
                _shadersUsedList.Add(currentShader);
            }

            // Add to render pass
            if (!objIsTransparent) // Opaque objects are grouped by pipeline state and then mesh usage
            {
                StructArenaAllocator<MeshRenderPipelineStateGroup> shaderGroupPool = _mainPassShaderGroups;

                // Get the pipeline group this mesh will use. (same pass, same pipe state)
                ref MeshRenderPipelineStateGroup pipelineGroup = ref shaderGroupPool[0];
                var shaderHash = currentShader.GetHashCode();
                if (_pipelineBatchLookup.ContainsKey(shaderHash))
                {
                    int key = _pipelineBatchLookup[shaderHash];
                    pipelineGroup = ref shaderGroupPool[key];
                }
                else  // create new
                {
                    pipelineGroup = ref shaderGroupPool.Allocate(out int thisShaderGroupIndex);
                    pipelineGroup.Shader = currentShader;
                    pipelineGroup.MeshRenderBatchLL_Start = -1;
                    pipelineGroup.MeshRenderBatchLL_End = -1;

                    // Assuming that the overwritten shader will require all meshes using it to upload state.
                    // AKA that a custom shader is custom for everyone that uses it, which is true unless the
                    // custom shader is a base mesh shader, but that's stupid.
                    pipelineGroup.UploadMetaStateToShader = overwrittenShader;

                    _pipelineBatchLookup.Add(shaderHash, thisShaderGroupIndex);
                }

                // Get the mesh batch for this pipeline state. (same mesh and same pipe state)
                ref MeshRenderMeshBatch meshBatch = ref _renderBatchPool[0];
                int meshShaderGroupHash = HashCode.Combine(mesh, shaderHash);
                if (_meshShaderGroupBatchLookup.ContainsKey(meshShaderGroupHash))
                {
                    int key = _meshShaderGroupBatchLookup[meshShaderGroupHash];
                    meshBatch = ref _renderBatchPool[key];
                }
                else // create new
                {
                    meshBatch = ref _renderBatchPool.Allocate(out int thisMeshGroupIndex);
                    meshBatch.Mesh = mesh;
                    meshBatch.NextRenderBatch = -1;
                    meshBatch.MeshInstanceLL_Start = -1;
                    meshBatch.MeshInstanceLL_End = -1;

                    _meshShaderGroupBatchLookup.Add(meshShaderGroupHash, thisMeshGroupIndex);

                    // Attach to the pipeline linked list
                    if (pipelineGroup.MeshRenderBatchLL_Start == -1) pipelineGroup.MeshRenderBatchLL_Start = thisMeshGroupIndex;
                    if (pipelineGroup.MeshRenderBatchLL_End == -1)
                    {
                        pipelineGroup.MeshRenderBatchLL_End = thisMeshGroupIndex;
                    }
                    else
                    {
                        ref var lastBatch = ref _renderBatchPool[pipelineGroup.MeshRenderBatchLL_End];
                        lastBatch.NextRenderBatch = thisMeshGroupIndex;
                        pipelineGroup.MeshRenderBatchLL_End = thisMeshGroupIndex;
                    }
                }

                // Create a instance of the mesh data. (unique props for this instance of the mesh)
                ref RenderInstanceMeshData instanceRegistration = ref _meshDataPool.Allocate(out int indexOfThisMeshData);
                instanceRegistration.BoneData = obj.GetBoneMatricesForMesh(m);
                instanceRegistration.ObjectRegistrationId = indexOfThisObjectData;
                instanceRegistration.NextMesh = -1;
                if (meshBatch.MeshInstanceLL_Start == -1) meshBatch.MeshInstanceLL_Start = indexOfThisMeshData;
                if (meshBatch.MeshInstanceLL_End == -1)
                {
                    meshBatch.MeshInstanceLL_End = indexOfThisMeshData;
                }
                else
                {
                    ref var lastBatch = ref _meshDataPool[meshBatch.MeshInstanceLL_End];
                    lastBatch.NextMesh = indexOfThisMeshData;
                    meshBatch.MeshInstanceLL_End = indexOfThisMeshData;
                }
            }
            else // Transparent objects are just inserted into a flat list to be sorted later. No batching.
            {
                ref RenderInstanceMeshDataTransparent instanceRegistration = ref _meshDataPoolTransparent.Allocate(out int _);
                instanceRegistration.Mesh = mesh;
                instanceRegistration.Shader = currentShader;
                instanceRegistration.BoneData = obj.GetBoneMatricesForMesh(m);
                instanceRegistration.ObjectRegistrationId = indexOfThisObjectData;
                instanceRegistration.UploadMetaStateToShader = overwrittenShader;
            }
        }
    }

    public void EndScene(RenderComposer c, Map3D map)
    {
        _inScene = false;

        var light = map.LightModel;

        // Split the scene using "practical/weighted splits" - a combination of
        // a logarithmic and uniform distribution.
        if (c.Camera is Camera3D cam3D)
        {
            if (_closestObjectDist == int.MaxValue || _closestObjectDist < cam3D.NearZ) _closestObjectDist = cam3D.NearZ;
            if (_furthestObjectDist == 0 || _furthestObjectDist > cam3D.FarZ) _furthestObjectDist = cam3D.FarZ;
        }

        float nearPlane = _closestObjectDist;
        float farPlane = _furthestObjectDist;
        float prevCascadeEnd = nearPlane;
        for (int i = 0; i < _shadowCascades.Length; i++)
        {
            float p = (i + 1) / (float)_shadowCascades.Length;
            float logSplit = nearPlane * MathF.Pow(farPlane / nearPlane, p);
            float uniformSplit = nearPlane + (farPlane - nearPlane) * p;

            var cascade = _shadowCascades[i];
            float max = Maths.Lerp(uniformSplit, logSplit, 0.5f); // Weighted average
            cascade.NearClip = prevCascadeEnd;
            cascade.FarClip = max;
            prevCascadeEnd = max;

            CalculateShadowMapCascadeMatrix(c, cascade, light);
        }

        // Upload base state to all shaders that will be in use.
        bool receiveAmbient = true; // todo
        for (int i = 0; i < _shadersUsedList.Count; i++)
        {
            ShaderProgram shader = _shadersUsedList[i];
            c.SetShader(shader);

            shader.SetUniformInt("diffuseTexture", 0);
            shader.SetUniformInt("shadowMapTextureC1", 1);
            shader.SetUniformInt("shadowMapTextureC2", 2);
            shader.SetUniformInt("shadowMapTextureC3", 3);
            shader.SetUniformInt("shadowMapTextureC4", 4);

            for (int cId = 0; cId < _shadowCascades.Length; cId++)
            {
                var cascade = _shadowCascades[cId];
                shader.SetUniformMatrix4(cascade.ViewProjUniformName, cascade.LightViewProj);
                shader.SetUniformFloat(cascade.FarZUnifornName, cascade.FarClip);
            }

            shader.SetUniformVector3("cameraPosition", c.Camera.Position);
            shader.SetUniformVector3("sunDirection", light.SunDirection);
            shader.SetUniformColor("ambientColor", receiveAmbient ? light.AmbientLightColor : Color.White);
            shader.SetUniformFloat("ambientLightStrength", receiveAmbient ? light.AmbientLightStrength : 1f);
            shader.SetUniformFloat("diffuseStrength", receiveAmbient ? light.DiffuseStrength : 0f);
            shader.SetUniformFloat("shadowOpacity", light.ShadowOpacity);
        }

        // Upload all meshes.
        for (int i = 0; i < _meshesUsedList.Count; i++)
        {
            var mesh = _meshesUsedList[i];
            bool skinnedMesh = mesh.Bones != null;
            GLRenderObjects? renderObj = GetMeshRenderObjectOrCreateNew(mesh, skinnedMesh, out bool alreadyUploaded);
            if (renderObj == null) // Impossible!
            {
                Assert(false, "No render object?");
                return;
            }

            if (!alreadyUploaded)
            {
                renderObj.VBO.UploadPartial(mesh.Vertices);
                renderObj.VBOExtended.UploadPartial(mesh.ExtraVertexData);

                if (skinnedMesh)
                {
                    AssertNotNull(renderObj.VBOBones);
                    renderObj.VBOBones.UploadPartial(mesh.BoneData);
                }

                renderObj.IBO.UploadPartial(mesh.Indices);
            }
        }

        // Bind cascade textures as none while rendering shadow maps.
        // 0 is diffuse texture
        const int reservedTextureSlots = 1;
        for (var j = reservedTextureSlots; j < reservedTextureSlots + _shadowCascades.Length; j++)
        {
            Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer, (uint) j);
        }

        if (light.Shadows)
        {
            for (int i = 0; i < _shadowCascades.Length; i++)
            {
                var cascade = _shadowCascades[i];
                if (cascade.Buffer == null) continue;

                _renderingShadowmap = i;
                c.RenderToAndClear(cascade.Buffer);
                Gl.DrawBuffers(Gl.NONE);
                Gl.ReadBuffer(Gl.NONE);
                RenderSceneFull(c);
                c.RenderTo(null);
                _renderingShadowmap = -1;
            }

            // Bind cascade textures.
            for (var j = reservedTextureSlots; j < reservedTextureSlots + _shadowCascades.Length; j++)
            {
                var cascade = _shadowCascades[j - reservedTextureSlots];
                Texture.EnsureBound(cascade.Buffer?.DepthStencilAttachment.Pointer ?? Texture.EmptyWhiteTexture.Pointer, (uint)j);
            }
        }

        // todo: different frustums for shadows would mean different pipeline groups etc.
        RenderSceneFull(c);
    }

    private void RenderSceneFull(RenderComposer c)
    {
        RenderMainPass(c, _mainPassShaderGroups);

        // Transparent objects are first sorted by distance from the camera, so they
        // can be drawn from furthest to closest and alpha blend with each other.
        //
        // Then they are drawn twice - first without writing to the color buffer
        // so they can populate the depth buffer with the highest depth, and then
        // drawn normally. This will prevent faces of one object from occluding other
        // faces in that same object, since they will be depth clipped.
        var transparentObjects = _meshDataPoolTransparent;
        var activeSlice = transparentObjects.GetActiveSlice();
        MemoryExtensions.Sort(activeSlice, _objectComparison);

        c.ToggleRenderColor(false);
        RenderTransparentObjects(c);
        c.ToggleRenderColor(true);
        RenderTransparentObjects(c);
    }

    private void RenderTransparentObjects(RenderComposer c)
    {
        if (_meshDataPoolTransparent.Length == 0) return;

        bool backfaceCulling = false; // todo: Move to render state.

        for (int i = 0; i < _meshDataPoolTransparent.Length; i++)
        {
            ref RenderInstanceMeshDataTransparent meshInstance = ref _meshDataPoolTransparent[i];

            var mesh = meshInstance.Mesh;
            var objectData = _objectDataPool[meshInstance.ObjectRegistrationId];

            ObjectFlags flags = objectData.Flags;
            if (_renderingShadowmap != -1 && flags.EnumHasFlag(ObjectFlags.Map3DDontThrowShadow)) return;

            var currentShader = meshInstance.Shader;

            Engine.Renderer.SetShader(currentShader);
            currentShader.SetUniformColor("diffuseColor", mesh.Material.DiffuseColor);

            Texture? diffuseTexture = mesh.Material.DiffuseTexture;
            Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

            bool skinnedMesh = mesh.Bones != null;
            GLRenderObjects? renderObj = GetMeshRenderObjectOrCreateNew(mesh, skinnedMesh, out bool _);
            AssertNotNull(renderObj);

            c.PushModelMatrix(objectData.ModelMatrix);
            currentShader.SetUniformColor("objectTint", objectData.MetaState.Tint);

            if (skinnedMesh && meshInstance.BoneData != null)
                currentShader.SetUniformMatrix4("boneMatrices", meshInstance.BoneData, meshInstance.BoneData.Length);

            if (meshInstance.UploadMetaStateToShader)
                objectData.MetaState.ApplyShaderUniforms(currentShader);

            // todo: render stream state
            bool changeBackcull = objectData.BackfaceCulling != backfaceCulling;
            if (changeBackcull)
            {
                if (objectData.BackfaceCulling)
                {
                    Gl.Enable(EnableCap.CullFace);
                    Gl.CullFace(CullFaceMode.Back);
                    Gl.FrontFace(FrontFaceDirection.Ccw);
                    backfaceCulling = true;
                }
                else
                {
                    Gl.Disable(EnableCap.CullFace);
                    backfaceCulling = false;
                }
            }

            // Render geometry
            VertexBuffer.EnsureBound(renderObj.VBO.Pointer);
            VertexBuffer.EnsureBound(renderObj.VBOExtended.Pointer);
            VertexArrayObject.EnsureBound(renderObj.VAO);
            IndexBuffer.EnsureBound(renderObj.IBO.Pointer);
            Gl.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedShort, nint.Zero);

            c.PopModelMatrix();
        }

        Engine.Renderer.SetShader(null);
        if (backfaceCulling) Gl.Disable(EnableCap.CullFace);
    }

    private void RenderMainPass(RenderComposer c, StructArenaAllocator<MeshRenderPipelineStateGroup> groupsInPass)
    {
        if (groupsInPass.Length == 0) return;

        bool backfaceCulling = false; // todo: Move to render state.
        for (int i = 0; i < groupsInPass.Length; i++) // for each pipeline
        {
            ref MeshRenderPipelineStateGroup pipelineState = ref groupsInPass[i];

            // Setup the pipeline.
            var currentShader = pipelineState.Shader;
            Engine.Renderer.SetShader(currentShader);

            currentShader.SetUniformInt("renderingShadowMap", _renderingShadowmap);

            // each mesh batch in this pipeline
            // these are parameters shared by all instances of that mesh in the scene
            int meshBatchIdx = pipelineState.MeshRenderBatchLL_Start;
            while (meshBatchIdx != -1)
            {
                ref MeshRenderMeshBatch batch = ref _renderBatchPool[meshBatchIdx];
                var mesh = batch.Mesh;

                currentShader.SetUniformColor("diffuseColor", mesh.Material.DiffuseColor);
                Texture? diffuseTexture = mesh.Material.DiffuseTexture;
                Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

                bool skinnedMesh = mesh.Bones != null;
                GLRenderObjects? renderObj = GetMeshRenderObjectOrCreateNew(mesh, skinnedMesh, out bool _);
                AssertNotNull(renderObj);

                // render each instance of the mesh
                // todo: instanced rendering
                int instanceIdx = batch.MeshInstanceLL_Start;
                while (instanceIdx != -1)
                {
                    ref RenderInstanceMeshData instance = ref _meshDataPool[instanceIdx];
                    ref RenderInstanceObjectData objectData = ref _objectDataPool[instance.ObjectRegistrationId];

                    ObjectFlags flags = objectData.Flags;
                    if (_renderingShadowmap != -1 && flags.EnumHasFlag(ObjectFlags.Map3DDontThrowShadow)) return;

                    c.PushModelMatrix(objectData.ModelMatrix);
                    currentShader.SetUniformColor("objectTint", objectData.MetaState.Tint);

                    if (skinnedMesh && instance.BoneData != null)
                        currentShader.SetUniformMatrix4("boneMatrices", instance.BoneData, instance.BoneData.Length);

                    if (pipelineState.UploadMetaStateToShader)
                        objectData.MetaState.ApplyShaderUniforms(currentShader);

                    // todo: render stream state
                    bool changeBackcull = objectData.BackfaceCulling != backfaceCulling;
                    if (changeBackcull)
                    {
                        if (objectData.BackfaceCulling)
                        {
                            Gl.Enable(EnableCap.CullFace);
                            Gl.CullFace(CullFaceMode.Back);
                            Gl.FrontFace(FrontFaceDirection.Ccw);
                            backfaceCulling = true;
                        }
                        else
                        {
                            Gl.Disable(EnableCap.CullFace);
                            backfaceCulling = false;
                        }
                    }

                    // Render geometry
                    VertexBuffer.EnsureBound(renderObj.VBO.Pointer);
                    VertexBuffer.EnsureBound(renderObj.VBOExtended.Pointer);
                    VertexArrayObject.EnsureBound(renderObj.VAO);
                    IndexBuffer.EnsureBound(renderObj.IBO.Pointer);
                    Gl.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedShort, nint.Zero);

                    c.PopModelMatrix();

                    instanceIdx = instance.NextMesh;
                }

                meshBatchIdx = batch.NextRenderBatch;
            }
        }

        // Restore render state.
        Engine.Renderer.SetShader(null);
        if (backfaceCulling) Gl.Disable(EnableCap.CullFace);
    }

    /// <summary>
    /// Meant to be used for rendering a one off entity, such as for UI or
    /// when not in a 3D map.
    /// </summary>
    public void RenderMeshEntityStandalone(
        MeshEntity entity,
        MeshEntityMetaState metaState,
        Matrix4x4[][]? boneMatricesPerMesh = null,
        LightModel? light = null,
        ObjectFlags flags = ObjectFlags.None
    )
    {
        Engine.Renderer.FlushRenderStream();

        EnsureAssetsLoaded();
        AssertNotNull(_meshShader);
        AssertNotNull(_skinnedMeshShader);
        AssertNotNull(_meshShaderShadowMap);
        AssertNotNull(_meshShaderShadowMapSkinned);

        Mesh[]? meshes = entity.Meshes;
        if (meshes == null) return;

        // Shadow map pass - object doesn't throw shadow.
        //if (_renderingShadowMap && flags.EnumHasFlag(ObjectFlags.Map3DDontThrowShadow)) return;

        if (entity.BackFaceCulling)
        {
            Gl.Enable(EnableCap.CullFace); // todo: render stream state
            Gl.CullFace(CullFaceMode.Back);
            Gl.FrontFace(FrontFaceDirection.Ccw);
        }

        ShaderProgram? shaderOverride = null;
        if (metaState.ShaderAsset != null)
        {
            shaderOverride = metaState.ShaderAsset.Shader;
            Engine.Renderer.SetShader(shaderOverride);
            metaState.ApplyShaderUniforms(shaderOverride);
        }

        for (var i = 0; i < meshes.Length; i++)
        {
            Mesh obj = meshes[i];
            if (!metaState.RenderMesh[i]) continue;

            if (obj.Vertices.Length != obj.ExtraVertexData.Length)
            {
                Assert(false, "Invalid mesh data.");
                continue;
            }

            // Decide which shader to use.
            bool receiveShadow = light != null && !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveShadow);
            bool receiveAmbient = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveAmbient);
            bool skinnedMesh = obj.Bones != null;
            ShaderProgram currentShader;
            if (shaderOverride != null)
            {
                currentShader = shaderOverride; // todo: can this handle the shadow map state?
            }
            else if (skinnedMesh)
            {
                currentShader = _skinnedMeshShader;
            }
            else
            {
                currentShader = _meshShader;
            }

            Engine.Renderer.SetShader(currentShader);

            // Material colors
            currentShader.SetUniformColor("diffuseColor", obj.Material.DiffuseColor);
            currentShader.SetUniformColor("objectTint", metaState.Tint);

            // Lighting
            if (light != null)
            {
                currentShader.SetUniformVector3("sunDirection", light.SunDirection);
                currentShader.SetUniformColor("ambientColor", receiveAmbient ? light.AmbientLightColor : Color.White);
                currentShader.SetUniformFloat("ambientLightStrength", receiveAmbient ? light.AmbientLightStrength : 1f);
                currentShader.SetUniformFloat("diffuseStrength", receiveAmbient ? light.DiffuseStrength : 0f);
                currentShader.SetUniformFloat("shadowOpacity", light.ShadowOpacity);
            }
            else
            {
                currentShader.SetUniformVector3("sunDirection", Vector3.Zero);
                currentShader.SetUniformColor("ambientColor", Color.White);
                currentShader.SetUniformFloat("ambientLightStrength", 1f);
                currentShader.SetUniformFloat("diffuseStrength", 0f);
            }

            //if (_renderingShadowMap)
            //    currentShader.SetUniformMatrix4("lightViewProj", _renderingShadowMapCurrentLightViewProj);

            currentShader.SetUniformVector3("cameraPosition", Engine.Renderer.Camera.Position);

            // Upload bone matrices for skinned meshes (if not missing).
            if (skinnedMesh && boneMatricesPerMesh != null)
            {
                Matrix4x4[] boneMats = boneMatricesPerMesh[i];
                currentShader.SetUniformMatrix4("boneMatrices", boneMats, boneMats.Length);
            }

            // Bind textures.
            // 0 - Diffuse
            // 1-2-3-4 - ShadowMap (based on cascades)
            // todo: convert cascades into array texture
            currentShader.SetUniformInt("diffuseTexture", 0);

            Texture? diffuseTexture = obj.Material.DiffuseTexture;
            Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

            currentShader.SetUniformInt("shadowMapTextureC1", 1);
            currentShader.SetUniformInt("shadowMapTextureC2", 2);
            currentShader.SetUniformInt("shadowMapTextureC3", 3);
            currentShader.SetUniformInt("shadowMapTextureC4", 4);

            // Cascade textures
            for (var j = 0; j < 4; j++)
            {
                Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer, (uint)(j + 1));
            }

            // Upload geometry
            GLRenderObjects? renderObj = GetMeshRenderObjectOrCreateNew(obj, skinnedMesh, out bool alreadyUploaded);
            if (renderObj == null) // Impossible!
            {
                Assert(false, "RenderStream had no render object to flush with.");
                return;
            }

            if (!alreadyUploaded)
            {
                renderObj.VBO.UploadPartial(obj.Vertices);
                renderObj.VBOExtended.UploadPartial(obj.ExtraVertexData);

                if (skinnedMesh)
                {
                    AssertNotNull(renderObj.VBOBones);
                    renderObj.VBOBones.UploadPartial(obj.BoneData);
                }

                renderObj.IBO.UploadPartial(obj.Indices);
            }

            // Render geometry
            VertexBuffer.EnsureBound(renderObj.VBO.Pointer);
            VertexBuffer.EnsureBound(renderObj.VBOExtended.Pointer);
            VertexArrayObject.EnsureBound(renderObj.VAO);
            IndexBuffer.EnsureBound(renderObj.IBO.Pointer);
            Gl.DrawElements(PrimitiveType.Triangles, obj.Indices.Length, DrawElementsType.UnsignedShort, nint.Zero);
        }

        if (entity.BackFaceCulling) Gl.Disable(EnableCap.CullFace);
        Engine.Renderer.SetShader();
    }

    #region GPU Memory Helpers

    private GLRenderObjects AllocateRenderObject(bool withBones)
    {
        var vbo = new VertexBuffer((uint)(ushort.MaxValue * VertexData.SizeInBytes), BufferUsage.StreamDraw);
        var vboExt = new VertexBuffer((uint)(ushort.MaxValue * VertexDataMesh3DExtra.SizeInBytes), BufferUsage.StreamDraw);
        VertexBuffer? vboBones = null;
        if (withBones) vboBones = new VertexBuffer((uint)(ushort.MaxValue * Mesh3DVertexDataBones.SizeInBytes), BufferUsage.StreamDraw);

        var ibo = new IndexBuffer(ushort.MaxValue * sizeof(ushort) * 3, BufferUsage.StreamDraw);

        var vao = new VertexArrayObjectTypeArg(typeof(VertexData), vbo, ibo);
        vao.AppendType(typeof(VertexDataMesh3DExtra), vboExt);
        if (withBones) vao.AppendType(typeof(Mesh3DVertexDataBones), vboBones);

        var objectsPair = new GLRenderObjects(vbo, vboExt, vboBones, ibo, vao);
        if (withBones)
            _renderObjectsBones.Push(objectsPair);
        else
            _renderObjects.Push(objectsPair);

        return objectsPair;
    }

    private GLRenderObjects? GetMeshRenderObjectOrCreateNew(Mesh mesh, bool withBones, out bool alreadyUploaded)
    {
        alreadyUploaded = false;

        // Mesh already uploaded!
        if (_meshToRenderObject.ContainsKey(mesh))
        {
            alreadyUploaded = true;
            return _meshToRenderObject[mesh];
        }

        Stack<GLRenderObjects> stack = withBones ? _renderObjectsBones : _renderObjects;
        if (stack.Count == 0) AllocateRenderObject(withBones);

        if (stack.Count > 0)
        {
            GLRenderObjects obj = stack.Pop();
            Stack<GLRenderObjects> usedStack = withBones ? _renderObjectsBonesUsed : _renderObjectsUsed;
            usedStack.Push(obj);

            _meshToRenderObject.Add(mesh, obj);
            return obj;
        }

        Assert(false, "No free render object for stream!?");
        return null;
    }

    public void DoTasks()
    {
        // Funnel used objects back into the usable pool.
        while (_renderObjectsUsed.Count > 0) _renderObjects.Push(_renderObjectsUsed.Pop());
        while (_renderObjectsBonesUsed.Count > 0) _renderObjectsBones.Push(_renderObjectsBonesUsed.Pop());

        _meshToRenderObject.Clear();
    }

    #endregion

    #region Shadow Helpers

    private void CalculateShadowMapCascadeMatrix(RenderComposer c, ShadowCascadeData cascade, LightModel model)
    {
        float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;

        // Get camera frustum for the current cascade clip.
        var cam3D = c.Camera as Camera3D;
        float fov = cam3D.FieldOfView;
        var cameraProjection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, cascade.NearClip, cascade.FarClip);
        Matrix4x4 cameraView = c.Camera.ViewMatrix;
        Span<Vector4> corners = stackalloc Vector4[8];
        GetFrustumCornersWorldSpace(corners, cameraView * cameraProjection, out Vector3 center);

        // The light view matrix looks at the center of the frustum.
        Vector3 eye = center + model.SunDirection;
        var lightView = Matrix4x4.CreateLookAt(eye, center, Vector3.UnitZ);

        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        for (var i = 0; i < corners.Length; i++)
        {
            Vector4 v = corners[i];
            Vector4 trf = Vector4.Transform(v, lightView);

            minX = Math.Min(minX, trf.X);
            maxX = Math.Max(maxX, trf.X);

            minY = Math.Min(minY, trf.Y);
            maxY = Math.Max(maxY, trf.Y);

            minZ = Math.Min(minZ, trf.Z);
            maxZ = Math.Max(maxZ, trf.Z);
        }

        // The projection of the light encompasses the frustum in a square.
        var lightProjection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);
        cascade.LightViewProj = lightView * lightProjection;
        cascade.DEBUG_View = lightView;

        // Apply the scale/offset matrix, which transforms from [-1,1]
        // post-projection space to [0,1] UV space
        Matrix4x4 texScaleBias = new Matrix4x4(
            0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, -0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.0f, 1.0f
        );
        Matrix4x4 texScaleBiasInv = texScaleBias.Inverted();

        //// Calculate the position of the lower corner of the cascade partition, in the UV space
        //// of the first cascade partition
        //Matrix4x4 invCascadeMat = Matrix4x4.Multiply(Matrix4x4.Multiply(texScaleBiasInv, invProj), invView);
        //Vector3 cascadeCorner = Vector3.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), invCascadeMat).XYZ();
        //cascadeCorner = Vector3.Transform(new Vector4(cascadeCorner, 1.0f), GlobalShadowMatrix).XYZ();

        //// Do the same for the upper corner
        //Vector3 otherCorner = Vector3.Transform(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), invCascadeMat).XYZ();
        //otherCorner = Vector3.Transform(new Vector4(otherCorner, 1.0f), GlobalShadowMatrix).XYZ();

        //// Calculate the scale and offset
        //Vector3 cascadeScale = new Vector3(1.0f) / (otherCorner - cascadeCorner);
        //CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
        //CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
    }

    private static void GetFrustumCornersWorldSpace(Span<Vector4> frustumCorners, Matrix4x4 cameraViewProj, out Vector3 frustumCenter)
    {
        Matrix4x4 inv = cameraViewProj.Inverted();

        Vector4 center = Vector4.Zero;
        var idx = 0;
        for (var x = 0; x < 2; ++x)
        {
            for (var y = 0; y < 2; ++y)
            {
                for (var z = 0; z < 2; ++z)
                {
                    var ndcPos = new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f);
                    Vector4 pt = Vector4.Transform(ndcPos, inv);
                    Vector4 cartPt = pt / pt.W;
                    frustumCorners[idx++] = cartPt;
                    center += cartPt;
                }
            }
        }

        frustumCenter = (center / 8).ToVec3();
    }

    #endregion
}

//public sealed class MeshEntityStreamRenderer
//{


//    public void LoadAssets()
//    {
//var meshShaderAsset = Engine.AssetLoader.Get<ShaderAsset>("Shaders/MeshShader.xml");
//ShaderAsset? skinnedMeshShader = meshShaderAsset?.GetShaderVariation("SKINNED");
//ShaderAsset? shadowMapShader = meshShaderAsset?.GetShaderVariation("SHADOW_MAP");
//ShaderAsset? skinnedShadowMapShader = meshShaderAsset?.GetShaderVariation("SKINNED_SHADOW_MAP");

//if (meshShaderAsset?.Shader == null || skinnedMeshShader?.Shader == null ||
//    shadowMapShader == null || skinnedShadowMapShader == null) return;

//_meshShader = meshShaderAsset.Shader;
//_skinnedMeshShader = skinnedMeshShader.Shader;
//_meshShaderShadowMap = shadowMapShader.Shader;
//_meshShaderShadowMapSkinned = skinnedShadowMapShader.Shader;
//Initialized = true;
//}

//private bool _initializedShadowMapObjects;
//private bool _renderingShadowMap;
//private Matrix4x4 _renderingShadowMapCurrentLightViewProj;
//private string[]? cascadePlaneFarZUniformNames;
//private string[]? cascadeLightProjUniformNames;

//private class ShadowCascade
//{
//    public FrameBuffer Buffer;
//    public float NearZ;
//    public float FarZ;
//    public Matrix4x4 LightViewProj;

//    public ShadowCascade(float nearZ, float farZ)
//    {
//        var resolution = new Vector2(2048);
//        NearZ = nearZ;
//        FarZ = farZ;

//        Buffer = new FrameBuffer(resolution).WithDepth(true);
//        Texture.EnsureBound(Buffer.DepthStencilAttachment.Pointer);
//        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_BORDER);
//        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_BORDER);

//        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
//        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, borderColor);
//    }
//}

//private int _shadowCascadeCount = 3;
//private ShadowCascade[]? _shadowCascades;

//private void InitializeShadowMapObjects()
//{
//    _shadowCascades = new ShadowCascade[3]
//    {
//        new ShadowCascade(10f, 300f),
//        new ShadowCascade(300f, 500f),
//        new ShadowCascade(500f, 1000f)
//    };
//    Assert(_shadowCascades.Length == _shadowCascadeCount);

//    cascadePlaneFarZUniformNames = new string[_shadowCascadeCount];
//    cascadeLightProjUniformNames = new string[_shadowCascadeCount];

//    for (var i = 0; i < _shadowCascadeCount; i++)
//    {
//        cascadePlaneFarZUniformNames[i] = $"cascadePlaneFarZ[{i}]";
//        cascadeLightProjUniformNames[i] = $"cascadeLightProj[{i}]";
//    }

//    _initializedShadowMapObjects = true;
//}

//public int GetShadowMapCascadeCount()
//{
//    if (!Initialized) return 0;
//    if (!_initializedShadowMapObjects) InitializeShadowMapObjects();
//    AssertNotNull(_shadowCascades);
//    return _shadowCascades.Length;
//}

//public void StartRenderShadowMap(int cascIdx, RenderComposer c, LightModel model)
//{
//    if (!Initialized) return;
//    AssertNotNull(_meshShaderShadowMap);
//    AssertNotNull(_meshShaderShadowMapSkinned);

//    c.FlushRenderStream();

//    if (!_initializedShadowMapObjects) InitializeShadowMapObjects();
//    AssertNotNull(_shadowCascades);

//    float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;

//    ShadowCascade cascade = _shadowCascades[cascIdx];
//    c.RenderToAndClear(cascade.Buffer);
//    Gl.DrawBuffers(Gl.NONE);
//    Gl.ReadBuffer(Gl.NONE);

//    float nearClip = cascade.NearZ;
//    float farClip = cascade.FarZ;

//    // Get camera frustum for the current cascade clip.
//    var cam3D = c.Camera as Camera3D;
//    float fov = cam3D.FieldOfView;
//    var cameraProjection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, nearClip, farClip);
//    Matrix4x4 cameraView = c.Camera.ViewMatrix;
//    Span<Vector4> corners = stackalloc Vector4[8];
//    GetFrustumCornersWorldSpace(corners, cameraView * cameraProjection, out Vector3 center);

//    // The light view matrix looks at the center of the frustum.
//    Vector3 eye = center + model.SunDirection;
//    var lightView = Matrix4x4.CreateLookAt(eye, center, Vector3.UnitZ);

//    float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
//    float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

//    for (var i = 0; i < corners.Length; i++)
//    {
//        Vector4 v = corners[i];
//        Vector4 trf = Vector4.Transform(v, lightView);

//        minX = Math.Min(minX, trf.X);
//        maxX = Math.Max(maxX, trf.X);

//        minY = Math.Min(minY, trf.Y);
//        maxY = Math.Max(maxY, trf.Y);

//        minZ = Math.Min(minZ, trf.Z);
//        maxZ = Math.Max(maxZ, trf.Z);
//    }

//    // Fudge the depth range of the shadow map.
//    var zMult = 10f;
//    if (minZ < 0)
//        minZ *= zMult;
//    else
//        minZ /= zMult;

//    if (maxZ < 0)
//        maxZ /= zMult;
//    else
//        maxZ *= zMult;

//    // The projection of the light encompasses the frustum in a square.
//    var lightProjection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);

//    cascade.LightViewProj = lightView * lightProjection;
//    _renderingShadowMapCurrentLightViewProj = cascade.LightViewProj;
//    _renderingShadowMap = true;
//}



//public void EndRenderShadowMap(RenderComposer c)
//{
//    if (!_renderingShadowMap) return;
//    AssertNotNull(_shadowCascades);

//    c.RenderTo(null);
//    c.FlushRenderStream();
//    _renderingShadowMap = false;
//}
//}