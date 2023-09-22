#nullable enable

#region Using

using Emotion.Game.ThreeDee;
using Emotion.Game.World3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
	public sealed class MeshEntityStreamRenderer
	{
		public bool Initialized { get; private set; }

		private ShaderProgram? _meshShader;
		private ShaderProgram? _skinnedMeshShader;

		#region GL Objects

		// todo: these could be shared by multiple mesh entity stream renderers
		// implying that there would be more than one
		private class GLRenderObjects
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

		private Stack<GLRenderObjects> _renderObjects = new();
		private Stack<GLRenderObjects> _renderObjectsUsed = new();

		private Stack<GLRenderObjects> _renderObjectsBones = new();
		private Stack<GLRenderObjects> _renderObjectsBonesUsed = new();

		#endregion

		public void LoadAssets()
		{
			var meshShaderAsset = Engine.AssetLoader.Get<ShaderAsset>("Shaders/MeshShader.xml");
			ShaderAsset? skinnedMeshShader = meshShaderAsset?.GetShaderVariation("SKINNED");

			if (meshShaderAsset?.Shader == null || skinnedMeshShader?.Shader == null) return;

			_meshShader = meshShaderAsset.Shader;
			_skinnedMeshShader = skinnedMeshShader.Shader;
			Initialized = true;
		}

		public void RenderMeshEntity(MeshEntity entity, MeshEntityMetaState metaState, Matrix4x4[][]? boneMatricesPerMesh = null, LightModel? light = null)
		{
			if (!Initialized) return;
			AssertNotNull(_meshShader);
			AssertNotNull(_skinnedMeshShader);

			Mesh[]? meshes = entity.Meshes;
			if (meshes == null) return;

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

				bool skinnedMesh = obj.Bones != null;
				ShaderProgram currentShader = shaderOverride ?? (skinnedMesh ? _skinnedMeshShader : _meshShader);
				Engine.Renderer.SetShader(currentShader);

				currentShader.SetUniformColor("diffuseColor", obj.Material.DiffuseColor);
				currentShader.SetUniformColor("objectTint", metaState.Tint);

				if (light == null || metaState.IgnoreLightModel)
				{
					currentShader.SetUniformColor("ambientColor", Color.White);
				}
				else
				{
					LightModel lightModel = light;

					currentShader.SetUniformVector3("sunDirection", lightModel.SunDirection);
					currentShader.SetUniformColor("sunColor", lightModel.SunColor);
					currentShader.SetUniformColor("ambientColor", lightModel.AmbientLightColor);
				}

				if (skinnedMesh && boneMatricesPerMesh != null)
				{
					Matrix4x4[] boneMats = boneMatricesPerMesh[i];
					currentShader.SetUniformMatrix4("boneMatrices", boneMats, boneMats.Length);
				}

				GLRenderObjects? renderObj = GetFirstFreeRenderObject(skinnedMesh);
				if (renderObj == null) // Impossible!
				{
					Assert(false, "RenderStream had no render object to flush with.");
					return;
				}

				renderObj.VBO.UploadPartial(obj.Vertices);
				renderObj.VBOExtended.UploadPartial(obj.ExtraVertexData);

				if (skinnedMesh)
				{
					AssertNotNull(renderObj.VBOBones);
					renderObj.VBOBones.UploadPartial(obj.BoneData);
				}

				renderObj.IBO.UploadPartial(obj.Indices);

				Texture? texture = obj.Material.DiffuseTexture;
				Texture.EnsureBound(texture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer);

				VertexArrayObject.EnsureBound(renderObj.VAO);
				IndexBuffer.EnsureBound(renderObj.IBO.Pointer);

				Gl.DrawElements(PrimitiveType.Triangles, obj.Indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
			}

			if (entity.BackFaceCulling) Gl.Disable(EnableCap.CullFace);
			Engine.Renderer.SetShader(null);
		}

		public void DoTasks()
		{
			// Funnel used objects back into the usable pool.
			while (_renderObjectsUsed.Count > 0) _renderObjects.Push(_renderObjectsUsed.Pop());
			while (_renderObjectsBonesUsed.Count > 0) _renderObjectsBones.Push(_renderObjectsBonesUsed.Pop());
		}

		private GLRenderObjects CreateRenderObject(bool withBones)
		{
			var vbo = new VertexBuffer((uint) (ushort.MaxValue * VertexData.SizeInBytes), BufferUsage.StreamDraw);
			var vboExt = new VertexBuffer((uint) (ushort.MaxValue * VertexDataMesh3DExtra.SizeInBytes), BufferUsage.StreamDraw);
			VertexBuffer? vboBones = null;
			if (withBones) vboBones = new VertexBuffer((uint) (ushort.MaxValue * Mesh3DVertexDataBones.SizeInBytes), BufferUsage.StreamDraw);

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

		private GLRenderObjects? GetFirstFreeRenderObject(bool withBones)
		{
			Stack<GLRenderObjects> stack = withBones ? _renderObjectsBones : _renderObjects;
			if (stack.Count == 0) CreateRenderObject(withBones);

			if (stack.Count > 0)
			{
				GLRenderObjects obj = stack.Pop();
				Stack<GLRenderObjects> usedStack = withBones ? _renderObjectsBonesUsed : _renderObjectsUsed;
				usedStack.Push(obj);
				return obj;
			}

			Assert(false, "No free render object for stream!?");
			return null;
		}
	}
}