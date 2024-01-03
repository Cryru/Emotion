#nullable enable

#region Using

using Emotion;
using Emotion.Game.ThreeDee;
using Emotion.Game.World;
using Emotion.Game.World3D;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;
using System.Security.Cryptography;
using static Emotion.Game.World3D.Map3D;
using static Emotion.Graphics.Batches3D.MeshEntityStreamRenderer;

#endregion

namespace Emotion.Graphics.Batches3D
{
    public sealed class MeshEntityBatchRenderer
    {
        private bool _assetsLoaded = false;
        private ShaderProgram _meshShader = null!;
        private ShaderProgram _skinnedMeshShader = null!;
        private ShaderProgram _meshShaderShadowMap = null!;
        private ShaderProgram _meshShaderShadowMapSkinned = null!;

        #region Scene State

        private bool _inScene;

        private StructPool<MeshRenderBatch> _batchPool = new(16);
        private StructPool<RenderInstanceObjectData> _objectDataPool = new(32);
        private StructPool<RenderInstanceMeshData> _meshDataPool = new(64);

        private static HashSet<ShaderProgram> _shadersUsedLookup = new HashSet<ShaderProgram>();
        private static List<ShaderProgram> _shadersUsedList = new List<ShaderProgram>();

        private static MeshRenderBatch _dummyMesh = new MeshRenderBatch();

        private static Dictionary<int, int> _batchLookup = new(); // This will be used to index

        #endregion

        public void EnsureAssetsLoaded()
        {
            if (_assetsLoaded) return;

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

            _assetsLoaded = true;
        }

        public void StartScene(RenderComposer c)
        {
            c.FlushRenderStream();

            EnsureAssetsLoaded();

            _inScene = true;

            _batchLookup.Clear();
            _batchPool.Reset();
            _objectDataPool.Reset();
            _meshDataPool.Reset();

            _shadersUsedLookup.Clear();
            _shadersUsedList.Clear();
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

            var objModelMatrix = obj.GetModelMatrix();

            // Objects can contain multiple meshes and they will refer to this.
            ref RenderInstanceObjectData objectInstance = ref _objectDataPool.Get(out int indexOfThisObjectData);
            objectInstance.BackfaceCulling = entity.BackFaceCulling;
            objectInstance.ModelMatrix = objModelMatrix;
            objectInstance.MetaState = metaState;

            var objIsTransparent = obj.IsTransparent();
            RenderPass passMeantFor = objIsTransparent ? RenderPass.Transparent : RenderPass.Main;

            // Register all meshes in this entity.
            for (int m = 0; m < meshes.Length; m++)
            {
                // Don't render mesh!
                if (!metaState.RenderMesh[m]) continue;

                var mesh = meshes[m];

                // Decide on shader, this determines batch.
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

                ref RenderInstanceMeshData meshRegistration = ref _meshDataPool.Get(out int indexOfThisMeshData);
                meshRegistration.BoneData = obj.GetBoneMatricesForMesh(m);
                meshRegistration.ObjectRegistrationId = indexOfThisObjectData;
                meshRegistration.NextMesh = -1;

                // Try to batch this mesh draw data.
                int renderPairHash = HashCode.Combine(mesh, currentShader, passMeantFor);

                // Try to match this render config to an existing batch.
                ref MeshRenderBatch batch = ref _dummyMesh;
                if (_batchLookup.ContainsKey(renderPairHash))
                {
                    int key = _batchLookup[renderPairHash];
                    batch = ref _batchPool[key];

                    // Change the index the last instance points to to this one.
                    ref RenderInstanceMeshData lastInstance = ref _meshDataPool[batch.MeshDataLL_End];
                    lastInstance.NextMesh = indexOfThisMeshData;
                    batch.MeshDataLL_End = indexOfThisMeshData;
                }
                else // Create new batch!
                {
                    batch = ref _batchPool.Get(out int thisBatchIndex);
                    _batchLookup.Add(renderPairHash, thisBatchIndex);

                    batch.RenderPass = passMeantFor;
                    batch.Mesh = mesh;
                    batch.Shader = currentShader;
                    batch.MeshDataLL_Start = indexOfThisMeshData;
                    batch.MeshDataLL_End = indexOfThisMeshData;

                    // Assuming that the overwritten shader will require all meshes using it to upload state.
                    // AKA that a custom shader is custom for everyone that uses it, which is true unless the
                    // custom shader is a base mesh shader, but that's stupid.
                    batch.UploadMetaStateToShader = overwrittenShader;
                    if (!_shadersUsedLookup.Contains(currentShader))
                    {
                        _shadersUsedLookup.Add(currentShader);
                        _shadersUsedList.Add(currentShader);
                    }
                }
            }
        }

        public void EndScene(RenderComposer c, Map3D map)
        {
            _inScene = false;

            bool backfaceCulling = false; // todo: Move to render state.

            // Upload base state to all shaders that will be in use.
            var light = map.LightModel;
            bool receiveAmbient = true; // todo
            for (int i = 0; i < _shadersUsedList.Count; i++)
            {
                var shader = _shadersUsedList[i];
                c.SetShader(shader);

                shader.SetUniformInt("diffuseTexture", 0);
                shader.SetUniformInt("shadowMapTextureC1", 1);
                shader.SetUniformInt("shadowMapTextureC2", 2);
                shader.SetUniformInt("shadowMapTextureC3", 3);

                shader.SetUniformVector3("cameraPosition", c.Camera.Position);

                shader.SetUniformVector3("sunDirection", light.SunDirection);
                shader.SetUniformColor("ambientColor", receiveAmbient ? light.AmbientLightColor : Color.White);
                shader.SetUniformFloat("ambientLightStrength", receiveAmbient ? light.AmbientLightStrength : 1f);
                shader.SetUniformFloat("diffuseStrength", receiveAmbient ? light.DiffuseStrength : 0f);
                shader.SetUniformFloat("shadowOpacity", light.ShadowOpacity);
            }

            // Render main pass batches.
            for (int i = 0; i < _batchPool.Length; i++)
            {
                ref MeshRenderBatch batch = ref _batchPool[i];
                if (!batch.RenderPass.EnumHasFlag(RenderPass.Main)) continue;

                var mesh = batch.Mesh;

                ShaderProgram currentShader = batch.Shader;
                Engine.Renderer.SetShader(currentShader);
                currentShader.SetUniformColor("diffuseColor", mesh.Material.DiffuseColor);

                Texture? diffuseTexture = mesh.Material.DiffuseTexture;
                Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

                bool skinnedMesh = mesh.Bones != null;
                GLRenderObjects? renderObj = c.RenderStream.MeshRenderer.GetFirstFreeRenderObject(mesh, skinnedMesh, out bool alreadyUploaded);
                if (renderObj == null) // Impossible!
                {
                    Assert(false, "No render object?");
                    continue;
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

                int instanceIdx = batch.MeshDataLL_Start;
                while (instanceIdx != -1)
                {
                    ref RenderInstanceMeshData instance = ref _meshDataPool[instanceIdx];
                    ref RenderInstanceObjectData objectData = ref _objectDataPool[instance.ObjectRegistrationId];

                    c.PushModelMatrix(objectData.ModelMatrix);
                    currentShader.SetUniformColor("objectTint", objectData.MetaState.Tint);

                    if (batch.UploadMetaStateToShader)
                        objectData.MetaState.ApplyShaderUniforms(currentShader);

                    if (skinnedMesh && instance.BoneData != null)
                        currentShader.SetUniformMatrix4("boneMatrices", instance.BoneData, instance.BoneData.Length);

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
            }

            // Restore render state.
            Engine.Renderer.SetShader(null);
            if (backfaceCulling) Gl.Disable(EnableCap.CullFace);
        }

        public void RenderEntityStandalone(MeshEntity entity)
        {

        }
    }

    public sealed class MeshEntityStreamRenderer
    {
        public bool Initialized { get; private set; }

        private ShaderProgram? _meshShader;
        private ShaderProgram? _skinnedMeshShader;
        private ShaderProgram? _meshShaderShadowMap;
        private ShaderProgram? _meshShaderShadowMapSkinned;

        #region GL Objects

        // todo: these could be shared by multiple mesh entity stream renderers
        // implying that there would be more than one
        public class GLRenderObjects
        {
            public VertexBuffer VBO;
            public VertexBuffer VBOExtended;
            public VertexBuffer? VBOBones;

            public IndexBuffer IBO;
            public VertexArrayObject VAO;

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

        public void LoadAssets()
        {
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
        }

        public void RenderMeshEntity(
            MeshEntity entity,
            MeshEntityMetaState metaState,
            Matrix4x4[][]? boneMatricesPerMesh = null,
            LightModel? light = null,
            ObjectFlags flags = ObjectFlags.None
        )
        {
            if (!Initialized) return;
            AssertNotNull(_meshShader);
            AssertNotNull(_skinnedMeshShader);
            AssertNotNull(_meshShaderShadowMap);
            AssertNotNull(_meshShaderShadowMapSkinned);

            Mesh[]? meshes = entity.Meshes;
            if (meshes == null) return;

            // Shadow map pass - object doesn't throw shadow.
            if (_renderingShadowMap && flags.EnumHasFlag(ObjectFlags.Map3DDontThrowShadow)) return;

            Engine.Renderer.FlushRenderStream();

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
                    if (_renderingShadowMap)
                        currentShader = _meshShaderShadowMapSkinned;
                    else
                        currentShader = _skinnedMeshShader;
                }
                else
                {
                    if (_renderingShadowMap)
                        currentShader = _meshShaderShadowMap;
                    else
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

                if (_renderingShadowMap)
                    currentShader.SetUniformMatrix4("lightViewProj", _renderingShadowMapCurrentLightViewProj);

                currentShader.SetUniformVector3("cameraPosition", Engine.Renderer.Camera.Position);

                // Upload bone matrices for skinned meshes (if not missing).
                if (skinnedMesh && boneMatricesPerMesh != null)
                {
                    Matrix4x4[] boneMats = boneMatricesPerMesh[i];
                    currentShader.SetUniformMatrix4("boneMatrices", boneMats, boneMats.Length);
                }

                // Bind textures.
                // 0 - Diffuse
                // 1-2-3 - ShadowMap (based on cascades)
                // todo: convert cascades into array texture
                currentShader.SetUniformInt("diffuseTexture", 0);

                Texture? diffuseTexture = obj.Material.DiffuseTexture;
                Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

                currentShader.SetUniformInt("shadowMapTextureC1", 1);
                currentShader.SetUniformInt("shadowMapTextureC2", 2);
                currentShader.SetUniformInt("shadowMapTextureC3", 3);

                if (_renderingShadowMap || !receiveShadow)
                {
                    for (var j = 0; j < _shadowCascadeCount; j++)
                    {
                        Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer, (uint)(j + 1));
                    }
                }
                else if (_initializedShadowMapObjects)
                {
                    AssertNotNull(_shadowCascades);
                    AssertNotNull(cascadePlaneFarZUniformNames);
                    AssertNotNull(cascadeLightProjUniformNames);

                    for (var j = 0; j < _shadowCascades.Length; j++)
                    {
                        ShadowCascade cascade = _shadowCascades[j];
                        uint bufferPointer = cascade.Buffer.DepthStencilAttachment?.Pointer ?? Texture.EmptyWhiteTexture.Pointer;
                        Texture.EnsureBound(bufferPointer, (uint)(j + 1));

                        currentShader.SetUniformFloat(cascadePlaneFarZUniformNames[j], cascade.FarZ);
                        currentShader.SetUniformMatrix4(cascadeLightProjUniformNames[j], cascade.LightViewProj);
                    }
                }

                // Upload geometry
                GLRenderObjects? renderObj = GetFirstFreeRenderObject(obj, skinnedMesh, out bool alreadyUploaded);
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

        public void DoTasks()
        {
            // Funnel used objects back into the usable pool.
            while (_renderObjectsUsed.Count > 0) _renderObjects.Push(_renderObjectsUsed.Pop());
            while (_renderObjectsBonesUsed.Count > 0) _renderObjectsBones.Push(_renderObjectsBonesUsed.Pop());

            _meshToRenderObject.Clear();
        }

        private GLRenderObjects CreateRenderObject(bool withBones)
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

        public GLRenderObjects? GetFirstFreeRenderObject(Mesh mesh, bool withBones, out bool alreadyUploaded)
        {
            alreadyUploaded = false;

            // If this entity was already rendered this frame, we can reuse its render object.
            if (_meshToRenderObject.ContainsKey(mesh))
            {
                alreadyUploaded = true;
                return _meshToRenderObject[mesh];
            }

            Stack<GLRenderObjects> stack = withBones ? _renderObjectsBones : _renderObjects;
            if (stack.Count == 0) CreateRenderObject(withBones);

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

        private bool _initializedShadowMapObjects;
        private bool _renderingShadowMap;
        private Matrix4x4 _renderingShadowMapCurrentLightViewProj;
        private string[]? cascadePlaneFarZUniformNames;
        private string[]? cascadeLightProjUniformNames;

        private class ShadowCascade
        {
            public FrameBuffer Buffer;
            public float NearZ;
            public float FarZ;
            public Matrix4x4 LightViewProj;

            public ShadowCascade(float nearZ, float farZ)
            {
                var resolution = new Vector2(2048);
                NearZ = nearZ;
                FarZ = farZ;

                Buffer = new FrameBuffer(resolution).WithDepth(true);
                Texture.EnsureBound(Buffer.DepthStencilAttachment.Pointer);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_BORDER);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_BORDER);

                float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, borderColor);
            }
        }

        private int _shadowCascadeCount = 3;
        private ShadowCascade[]? _shadowCascades;

        private void InitializeShadowMapObjects()
        {
            _shadowCascades = new ShadowCascade[3]
            {
                new ShadowCascade(10f, 300f),
                new ShadowCascade(300f, 500f),
                new ShadowCascade(500f, 1000f)
            };
            Assert(_shadowCascades.Length == _shadowCascadeCount);

            cascadePlaneFarZUniformNames = new string[_shadowCascadeCount];
            cascadeLightProjUniformNames = new string[_shadowCascadeCount];

            for (var i = 0; i < _shadowCascadeCount; i++)
            {
                cascadePlaneFarZUniformNames[i] = $"cascadePlaneFarZ[{i}]";
                cascadeLightProjUniformNames[i] = $"cascadeLightProj[{i}]";
            }

            _initializedShadowMapObjects = true;
        }

        public int GetShadowMapCascadeCount()
        {
            if (!Initialized) return 0;
            if (!_initializedShadowMapObjects) InitializeShadowMapObjects();
            AssertNotNull(_shadowCascades);
            return _shadowCascades.Length;
        }

        public void StartRenderShadowMap(int cascIdx, RenderComposer c, LightModel model)
        {
            if (!Initialized) return;
            AssertNotNull(_meshShaderShadowMap);
            AssertNotNull(_meshShaderShadowMapSkinned);

            c.FlushRenderStream();

            if (!_initializedShadowMapObjects) InitializeShadowMapObjects();
            AssertNotNull(_shadowCascades);

            float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;

            ShadowCascade cascade = _shadowCascades[cascIdx];
            c.RenderToAndClear(cascade.Buffer);
            Gl.DrawBuffers(Gl.NONE);
            Gl.ReadBuffer(Gl.NONE);

            float nearClip = cascade.NearZ;
            float farClip = cascade.FarZ;

            // Get camera frustum for the current cascade clip.
            var cam3D = c.Camera as Camera3D;
            float fov = cam3D.FieldOfView;
            var cameraProjection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, nearClip, farClip);
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

            // Fudge the depth range of the shadow map.
            var zMult = 10f;
            if (minZ < 0)
                minZ *= zMult;
            else
                minZ /= zMult;

            if (maxZ < 0)
                maxZ /= zMult;
            else
                maxZ *= zMult;

            // The projection of the light encompasses the frustum in a square.
            var lightProjection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);

            cascade.LightViewProj = lightView * lightProjection;
            _renderingShadowMapCurrentLightViewProj = cascade.LightViewProj;
            _renderingShadowMap = true;
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

        public void EndRenderShadowMap(RenderComposer c)
        {
            if (!_renderingShadowMap) return;
            AssertNotNull(_shadowCascades);

            c.RenderTo(null);
            c.FlushRenderStream();
            _renderingShadowMap = false;
        }
    }
}