#nullable enable

using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Memory;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using OpenGL;

namespace Emotion.Graphics.Renderer3D;

public sealed class MeshEntityRenderer
{
    private Queue<MeshEntityRendererScene> _scenes = new Queue<MeshEntityRendererScene>();
    private MeshEntityRendererScene? _currentScene = null;

    public void StartScene(LightConfig? light)
    {
        MeshEntityRendererScene renderScene = MeshEntityRendererScene.Shared.Get();
        if (light != null)
            renderScene.SetLightConfig(light);

        _scenes.Enqueue(renderScene);
        _currentScene = renderScene;
    }

    public void AddToCurrentScene(Renderer r, MeshEntityMetaState entityState)
    {
        AssertNotNull(_currentScene); // Scene not started?
        if (_currentScene == null) return;

        Engine.Renderer.FlushRenderStream();

        LightConfig? light = _currentScene.Light;

        RenderState prevState = r.CurrentState;

        MeshEntity entity = entityState.Entity;
        Matrix4x4 modelMatrix = entityState.ModelMatrix;

        r.PushModelMatrix(modelMatrix);
        for (int i = 0; i < entity.Meshes.Length; i++)
        {
            Mesh mesh = entity.Meshes[i];
            PerMeshState perMeshState = entityState.PerMeshState[i];
            if (!perMeshState.RenderMesh) continue;
            MeshMaterial material = mesh.Material;

            // Determine render state
            ShaderProgram currentShader;
            {
                // todo: This is a clusterfuck
                RenderState state = material.State; // Depends on material?
                state.ShaderGroupDefinition = new ShaderGroupDefinition(mesh.VertexFormat, _currentScene.Definitions); // Depends on mesh + _current scene?
                state.ViewMatrix = prevState.ViewMatrix; // Depends on current state
                r.SetState(state);
                currentShader = r.CurrentShader;
            }

            // Material properties
            {
                // todo: this should really be texture or color - not both
                Texture texture = Texture.EmptyWhiteTexture;
                if (material.DiffuseTexture != null) texture = material.DiffuseTexture;
                Texture.EnsureBound(texture.Pointer);

                currentShader.SetUniformColor("diffuseColor", entityState.Tint != Color.White ? entityState.Tint : material.DiffuseColor);
            }

            // Render Scene Properties
            {
                if (light != null)
                {
                    currentShader.SetUniformVector3("cameraPosition", r.Camera.Position);
                    currentShader.SetUniformVector3("sunDirection", Vector3.Normalize(light.SunDirection));
                    currentShader.SetUniformColor("ambientColor", light.AmbientLightColor);
                    currentShader.SetUniformFloat("ambientLightStrength", light.AmbientLightStrength);
                    currentShader.SetUniformFloat("diffuseStrength", light.DiffuseStrength);
                    currentShader.SetUniformFloat("shadowOpacity", light.ShadowOpacity);
                }
            }

            // EntityState
            {
                Matrix4x4[]? boneMatrices = entityState.GetBoneMatricesForMesh(i);
                if (boneMatrices != null)
                    currentShader.SetUniformMatrix4("boneMatrices", boneMatrices, boneMatrices.Length);
            }

            // Render
            {
                GPUVertexMemory gpuMem = GPUMemoryAllocator.AllocateBuffer(mesh.VertexFormat);
                VertexArrayObject.EnsureBound(gpuMem.VAO);
                gpuMem.VBO.Upload(mesh.VertexAllocation);

                IndexBuffer ibo = GPUMemoryAllocator.RentIndexBuffer(mesh.Indices.Length);
                IndexBuffer.EnsureBound(ibo.Pointer);
                ibo.Upload(mesh.Indices);

                Gl.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedShort, nint.Zero);

                GPUMemoryAllocator.ReturnIndexBuffer(ibo);
                GPUMemoryAllocator.FreeBuffer(gpuMem);
            }
        }
        r.PopModelMatrix();
        r.SetShader(null);

        r.SetState(prevState);
    }

    public void EndScene()
    {
        MeshEntityRendererScene old = _scenes.Dequeue();
        _scenes.TryPeek(out MeshEntityRendererScene? newTop);
        _currentScene = newTop;

        MeshEntityRendererScene.Shared.Return(old);
        Engine.Renderer.FlushRenderStream();
    }

    private class MeshEntityRendererScene
    {
        public static ObjectPool<MeshEntityRendererScene> Shared = new ObjectPool<MeshEntityRendererScene>(resetMethod: static (p) => p.Reset());

        public List<string> Definitions = new List<string>();
        public LightConfig? Light { get; set; }

        public MeshEntityRendererScene()
        {

        }

        public void SetLightConfig(LightConfig light)
        {
            Light = light;
            Definitions.Add("LIGHT_ENABLED");
        }

        public void Reset()
        {
            Definitions = new List<string>();
            Light = null;
        }
    }
}
