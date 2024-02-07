#nullable enable

#region Using

using Emotion;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Editor;
using Emotion.Editor.EditorHelpers;
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
using Emotion.UI;
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
    private Matrix4x4 _globalShadowMatrix;

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
        public static Vector2 FramebufferResolution = new Vector2(1024);

        public int CascadeId;
        public FrameBuffer? Buffer;

        public float FarClip;
        public float NearClip;

        public Matrix4x4 LightViewProj;
        public float UnitToTexelScale;

        public string ViewProjUniformName;
        public string UnitToTexelScaleUniformName;

        public ShadowCascadeData(int cascadeId)
        {
            CascadeId = cascadeId;
            GLThread.ExecuteGLThreadAsync(InitFrameBuffer);
            LightViewProj = Matrix4x4.Identity;

            ViewProjUniformName = $"cascadeLightProj[{cascadeId}]";
            UnitToTexelScaleUniformName = $"cascadeUnitToTexel[{cascadeId}]";
        }

        private void InitFrameBuffer()
        {
            // Create a cascade framebuffer to hold the VSM data.
            // A depth component is needed as some drivers will not output gl_FragCoord.z
            Buffer = new FrameBuffer(FramebufferResolution).WithColor(true, InternalFormat.Rg32F, PixelFormat.Rgba).WithDepth();
            Buffer.ColorAttachment.Smooth = true;

            Texture.EnsureBound(Buffer.ColorAttachment.Pointer);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_BORDER);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_BORDER);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, borderColor);
        }
    }

    private ShadowCascadeData[] _shadowCascades = null!;

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
        }
        else
        {
            _meshShader = ShaderFactory.DefaultProgram;
            _skinnedMeshShader = ShaderFactory.DefaultProgram;
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
        if (distanceToCameraMin < objBSphere.Radius) distanceToCameraMin = 10f;

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
                currentShader = _skinnedMeshShader;
            }
            else
            {
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

        CalculateShadowMapCascadeData(light);

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
                shader.SetUniformFloat(cascade.UnitToTexelScaleUniformName, cascade.UnitToTexelScale);
            }

            shader.SetUniformVector3("cameraPosition", c.Camera.Position);
            shader.SetUniformVector3("sunDirection", light.SunDirection);
            shader.SetUniformColor("ambientColor", receiveAmbient ? light.AmbientLightColor : Color.White);
            shader.SetUniformFloat("ambientLightStrength", receiveAmbient ? light.AmbientLightStrength : 1f);
            shader.SetUniformFloat("diffuseStrength", receiveAmbient ? light.DiffuseStrength : 0f);
            shader.SetUniformFloat("shadowOpacity", light.ShadowOpacity);
            shader.SetUniformMatrix4("globalShadowMatrix", _globalShadowMatrix);
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
            Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer, (uint)j);
        }

        if (light.Shadows)
        {
            for (int i = 0; i < _shadowCascades.Length; i++)
            {
                var cascade = _shadowCascades[i];
                if (cascade.Buffer == null) continue;

                _renderingShadowmap = i;
                c.RenderToAndClear(cascade.Buffer);
                //Gl.DrawBuffers(Gl.NONE);
                //Gl.ReadBuffer(Gl.NONE);
                RenderSceneFull(c);
                c.RenderTo(null);
                _renderingShadowmap = -1;
            }

            // Bind cascade textures.
            for (var j = reservedTextureSlots; j < reservedTextureSlots + _shadowCascades.Length; j++)
            {
                ShadowCascadeData cascade = _shadowCascades[j - reservedTextureSlots];
                Texture.EnsureBound(cascade.Buffer?.ColorAttachment.Pointer ?? Texture.EmptyWhiteTexture.Pointer, (uint)j);
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

            bool receiveAmbient = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveAmbient);
            bool receiveShadow = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveShadow);
            int lightMode = 0;
            if (!receiveAmbient) lightMode = 1;
            if (!receiveShadow) lightMode = 2;
            if (!receiveAmbient && !receiveShadow) lightMode = 3;
            currentShader.SetUniformInt("lightMode", lightMode);

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

                    bool receiveAmbient = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveAmbient);
                    bool receiveShadow = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveShadow);
                    int lightMode = 0;
                    if (!receiveAmbient) lightMode = 1;
                    if (!receiveShadow) lightMode = 2;
                    if (!receiveAmbient && !receiveShadow) lightMode = 3;
                    currentShader.SetUniformInt("lightMode", lightMode);

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
            //bool receiveShadow = light != null && !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveShadow);
            //bool receiveAmbient = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveAmbient);
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
                currentShader.SetUniformColor("ambientColor", light.AmbientLightColor);
                currentShader.SetUniformFloat("ambientLightStrength", light.AmbientLightStrength);
                currentShader.SetUniformFloat("diffuseStrength", light.DiffuseStrength);
                currentShader.SetUniformFloat("shadowOpacity", 0f); // No shadows outside of scene
            }
            else
            {
                currentShader.SetUniformVector3("sunDirection", LightModel.DefaultLightModel.SunDirection);
                currentShader.SetUniformColor("ambientColor", Color.White);
                currentShader.SetUniformFloat("ambientLightStrength", 1f);
                currentShader.SetUniformFloat("diffuseStrength", 0f);
                currentShader.SetUniformFloat("shadowOpacity", 0f);
            }

            bool receiveAmbient = !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveAmbient);
            bool receiveShadow = false;// !flags.EnumHasFlag(ObjectFlags.Map3DDontReceiveShadow);
            int lightMode = 0;
            if (!receiveAmbient) lightMode = 1;
            if (!receiveShadow) lightMode = 2;
            if (!receiveAmbient && !receiveShadow) lightMode = 3;
            currentShader.SetUniformInt("lightMode", lightMode);

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

    public class ShadowRenderingDebugInformation
    {
        public bool FreezeCamera;

        public float ShadowMapNear;
        public float ShadowMapFar;
        public Matrix4x4 GlobalShadowMatrix;

        public int CascadeInfo;

        [DontSerialize]
        [DontShowInEditor]
        public FrameBuffer CascadeTexture;

        public Vector2 CascadeX;
        public Vector2 CascadeY;
        public Vector2 CascadeZ;

        public Vector3[] CascadeFrustumCorners = new Vector3[8];
    }

    public ShadowRenderingDebugInformation ShadowDebugInfo = new();

    private void CalculateShadowMapCascadeData(LightModel light)
    {
        var c = Engine.Renderer;

        // Split the scene using "practical/weighted splits" - a combination of
        // a logarithmic and uniform distribution.
        var renderer = Engine.Renderer;
        var camera = renderer.Camera;

        if (_closestObjectDist == int.MaxValue || _closestObjectDist < camera.NearZ) _closestObjectDist = camera.NearZ;
        if (_furthestObjectDist == 0 || _furthestObjectDist > camera.FarZ) _furthestObjectDist = camera.FarZ;

        //Matrix4x4 cameraViewProjection = renderer.Camera.ViewMatrix * renderer.Camera.ProjectionMatrix;
        //Matrix4x4 cameraViewProjectionInv = cameraViewProjection.Inverted();

        //float cascadeResolution = ShadowCascadeData.FramebufferResolution.X;

        float nearPlane = _closestObjectDist;
        float farPlane = _furthestObjectDist;
        //float clipDist = farPlane - nearPlane;

        {
            ShadowDebugInfo.ShadowMapNear = nearPlane;
            ShadowDebugInfo.ShadowMapFar = farPlane;

            float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;
            var cam3D = c.Camera as Camera3D;
            float fov = cam3D.FieldOfView;
            var cameraDebugProj = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, nearPlane, farPlane);
            _globalShadowMatrix = cameraDebugProj;

            ShadowDebugInfo.GlobalShadowMatrix = (c.Camera.ViewMatrix * cameraDebugProj);
        }

        //float prevCascadeSplit = nearPlane;
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

            //float cascadeSplit = cascade.NearClip + (cascade.FarClip - cascade.NearClip) * (i + 1) / _shadowCascades.Length;
            //cascade.Split = cascadeSplit;

            CalculateShadowMapCascadeMatrix(c, cascade, light);

            //float cascadeWidth = cascadeSplit - prevCascadeSplit;
            //Vector4 scale = new Vector4(cascadeWidth / 2.0f, cascadeWidth / 2.0f, cascadeSplit - prevCascadeSplit, 1.0f);
            //cascade.Scale = Vector4.One;
            //cascade.Offset = (Maths.TransformCartesian(Vector3.Zero, cascade.LightViewProj) - Maths.TransformCartesian(Vector3.One, cascade.LightViewProj)).ToVec4();

            prevCascadeEnd = max;
            //prevCascadeSplit = cascadeSplit;
        }
    }

    private void CalculateShadowMapCascadeMatrix(RenderComposer c, ShadowCascadeData cascade, LightModel light)
    {
        float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;

        // Get camera frustum for the current cascade clip.
        var cam3D = c.Camera as Camera3D;
        float fov = cam3D.FieldOfView;
        var cameraProjection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, cascade.NearClip, cascade.FarClip);

        Matrix4x4 cameraView = c.Camera.ViewMatrix;
        var cameraViewProjInv = (cameraView * cameraProjection).Inverted();

        Span<Vector3> corners = new Vector3[8];
        CameraBase.GetCameraFrustum3D(corners, cameraViewProjInv);

        // Calculate the centroid of the view frustum slice
        Vector3 frustumCenter = Vector3.Zero;
        for (int i = 0; i < 8; ++i)
            frustumCenter = frustumCenter + corners[i];
        frustumCenter /= 8.0f;

        float radius = (corners[0] - corners[6]).Length() / 2f;
        radius = MathF.Ceiling(radius / 16.0f) * 16.0f;

        float texelPerUnit = ShadowCascadeData.FramebufferResolution.X / (radius * 2f);
        Matrix4x4 texelScalar = Matrix4x4.CreateScale(texelPerUnit, texelPerUnit, texelPerUnit);
        cascade.UnitToTexelScale = (1.0f / texelPerUnit);

        Vector3 baseLookAt = light.SunDirection;
        Matrix4x4 lookAt = Matrix4x4.CreateLookAt(Vector3.Zero, baseLookAt, RenderComposer.Up);
        lookAt = texelScalar * lookAt;
        Matrix4x4 lookAtInv = lookAt.Inverted();

        // Snap the frustum center to a texel size increment.
        frustumCenter = Vector3.Transform(frustumCenter, lookAt);
        frustumCenter = frustumCenter.Floor();
        frustumCenter = Vector3.Transform(frustumCenter, lookAtInv);

        // The light view matrix looks at the center of the frustum.
        Vector3 eye = frustumCenter + (light.SunDirection * radius * 2f);
        var lightView = Matrix4x4.CreateLookAt(eye, frustumCenter, Vector3.UnitZ);

        //float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        //float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        //for (var i = 0; i < corners.Length; i++)
        //{
        //    Vector3 v = corners[i];
        //    Vector3 trf = Maths.TransformCartesian(v, lightView);

        //    minX = Math.Min(minX, trf.X);
        //    maxX = Math.Max(maxX, trf.X);

        //    minY = Math.Min(minY, trf.Y);
        //    maxY = Math.Max(maxY, trf.Y);

        //    minZ = Math.Min(minZ, trf.Z);
        //    maxZ = Math.Max(maxZ, trf.Z);
        //}

        // The projection of the light encompasses the frustum in a square.
        var lightProjection = Matrix4x4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius * 6, radius * 6);
        cascade.LightViewProj = lightView * lightProjection;

        if (ShadowDebugInfo.CascadeInfo == cascade.CascadeId)
        {
            //ShadowDebugInfo.CascadeX = new Vector2(minX, maxX);
            //ShadowDebugInfo.CascadeY = new Vector2(minY, maxY);
            //ShadowDebugInfo.CascadeZ = new Vector2(minZ, maxZ);

            for (int i = 0; i < corners.Length; i++)
            {
                ShadowDebugInfo.CascadeFrustumCorners[i] = corners[i];
            }

            ShadowDebugInfo.CascadeTexture = cascade.Buffer;
        }
    }

    #endregion
}

public class ShadowDebugPanel : EditorPanel
{
    private UISolidColor _drawArea;
    
    public ShadowDebugPanel() : base("Shadow Debug")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        _contentParent.LayoutMode = LayoutMode.HorizontalList;

        var propsPanel = new GenericPropertiesEditorPanel(Engine.Renderer.MeshEntityRenderer.ShadowDebugInfo);
        propsPanel.PanelMode = PanelMode.Embedded;
        _contentParent.AddChild(propsPanel);

        _drawArea = new UISolidColor();
        _drawArea.WindowColor = new Color(32, 32, 32);
        _drawArea.MaxSize = new Vector2(200, 999);
        _contentParent.AddChild(_drawArea);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        var shadowDebug = Engine.Renderer.MeshEntityRenderer.ShadowDebugInfo;

        Vector3[] corners = new Vector3[8];
        CameraBase.GetCameraFrustum3D(corners, shadowDebug.GlobalShadowMatrix.Inverted());

        RenderFrustum(c, corners, Color.White);

        var shadowCascadeCorners = shadowDebug.CascadeFrustumCorners;
        RenderFrustum(c, shadowCascadeCorners, Color.Red);

        c.RenderSprite(Vector2.Zero, new Vector2(256), Color.White, shadowDebug.CascadeTexture.DepthStencilAttachment);
    }

    private void RenderFrustum(RenderComposer c, Vector3[] corners, Color col)
    {
        Vector2 worldMin = new Vector2(-1_000);
        Vector2 worldMax = new Vector2(1_000);

        for (int i = 0; i < corners.Length; i++)
        {
            var corner = corners[i];
            corner.X = Maths.Map(corner.X, worldMin.X, worldMax.X, _drawArea.X, _drawArea.Bounds.Right);
            corner.Y = Maths.Map(corner.Y, worldMin.Y, worldMax.Y, _drawArea.Y, _drawArea.Bounds.Bottom);
            corner.Z = Z;
            corners[i] = corner;
        }

        c.RenderLine(corners[0], corners[1], col);
        c.RenderLine(corners[1], corners[2], col);
        c.RenderLine(corners[2], corners[3], col);
        c.RenderLine(corners[3], corners[0], col);
                                             
        c.RenderLine(corners[4], corners[5], col);
        c.RenderLine(corners[5], corners[6], col);
        c.RenderLine(corners[6], corners[7], col);
        c.RenderLine(corners[7], corners[4], col);
                                             
        c.RenderLine(corners[0], corners[4], col);
        c.RenderLine(corners[1], corners[5], col);
        c.RenderLine(corners[2], corners[6], col);
        c.RenderLine(corners[3], corners[7], col);
    }
}