#nullable enable

#region Using

using Emotion.Game.ThreeDee;
using Emotion.Game.World2D;
using Emotion.Game.World3D;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
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

		private Dictionary<Mesh, GLRenderObjects> _meshToRenderObject = new();

		private Stack<GLRenderObjects> _renderObjects = new();
		private Stack<GLRenderObjects> _renderObjectsUsed = new();

		private Stack<GLRenderObjects> _renderObjectsBones = new();
		private Stack<GLRenderObjects> _renderObjectsBonesUsed = new();

		#endregion

		public void LoadAssets()
		{
			var meshShaderAsset = Engine.AssetLoader.Get<ShaderAsset>("Shaders/MeshShader.xml");
			ShaderAsset? skinnedMeshShader = meshShaderAsset?.GetShaderVariation("SKINNED");
			ShaderAsset? shadowMapShader = meshShaderAsset?.GetShaderVariation("SHADOW_MAP");
			ShaderAsset? skinnedShadowMapShader = meshShaderAsset?.GetShaderVariation("SKINNED_SHADOW_MAP");

			if (meshShaderAsset?.Shader == null || skinnedMeshShader?.Shader == null ||
			    shadowMapShader == null || skinnedShadowMapShader == null) return;

			_meshShader = meshShaderAsset.Shader;
			_skinnedMeshShader = skinnedMeshShader.Shader;
			_meshShaderShadowMap = shadowMapShader.Shader;
			_meshShaderShadowMapSkinned = skinnedShadowMapShader.Shader;
			Initialized = true;
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
					{
						currentShader = _meshShaderShadowMapSkinned;
					}
					else
					{
						currentShader = _skinnedMeshShader;
					}
				}
				else
				{
					if (_renderingShadowMap)
					{
						currentShader = _meshShaderShadowMap;
					}
					else
					{
						currentShader = _meshShader;
					}
				}
				Engine.Renderer.SetShader(currentShader);

				// Material colors
				currentShader.SetUniformColor("diffuseColor", obj.Material.DiffuseColor);
				currentShader.SetUniformColor("objectTint", metaState.Tint);

				// Lighting
				if (light != null)
				{
					currentShader.SetUniformVector3("sunDirection", light.SunDirection);
					currentShader.SetUniformColor("sunColor", receiveAmbient ? light.SunColor : Color.Black);
					currentShader.SetUniformColor("ambientColor", light.AmbientLightColor);
				}
				else
				{
					currentShader.SetUniformVector3("sunDirection", Vector3.Zero);
					currentShader.SetUniformColor("sunColor", receiveAmbient ? Color.White : Color.Black);
					currentShader.SetUniformColor("ambientColor", Color.White);
				}

				currentShader.SetUniformMatrix4("lightViewProj", _lightViewProj);

				// Upload bone matrices for skinned meshes (if not missing).
				if (skinnedMesh && boneMatricesPerMesh != null)
				{
					Matrix4x4[] boneMats = boneMatricesPerMesh[i];
					currentShader.SetUniformMatrix4("boneMatrices", boneMats, boneMats.Length);
				}

				// Bind textures.
				// 0 - Diffuse
				// 1 - ShadowMap
				currentShader.SetUniformInt("diffuseTexture", 0);

				Texture? diffuseTexture = obj.Material.DiffuseTexture;
				Texture.EnsureBound(diffuseTexture?.Pointer ?? Texture.EmptyWhiteTexture.Pointer, 0);

				currentShader.SetUniformInt("shadowMapTexture", 1);

				if (_renderingShadowMap || !receiveShadow)
					Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer, 1);
				else
					Texture.EnsureBound(_shadowDepth?.DepthStencilAttachment.Pointer ?? Texture.EmptyWhiteTexture.Pointer, 1);

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
				Gl.DrawElements(PrimitiveType.Triangles, obj.Indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
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

		private GLRenderObjects? GetFirstFreeRenderObject(Mesh mesh, bool withBones, out bool alreadyUploaded)
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
		private FrameBuffer? _shadowDepth;
		private Matrix4x4 _lightViewProj;

		private void InitializeShadowMapObjects()
		{
			_shadowDepth = new FrameBuffer(new Vector2(2048)).WithDepth(true);
			Texture.EnsureBound(_shadowDepth.DepthStencilAttachment.Pointer);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_BORDER);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_BORDER);

			float[] borderColor = {1.0f, 1.0f, 1.0f, 1.0f};
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, borderColor);

			_initializedShadowMapObjects = true;
		}

		public void StartRenderShadowMap(RenderComposer c, LightModel model)
		{
			if (!Initialized) return;
			AssertNotNull(_meshShaderShadowMap);
			AssertNotNull(_meshShaderShadowMapSkinned);

			c.FlushRenderStream();

			if (!_initializedShadowMapObjects) InitializeShadowMapObjects();

			float aspectRatio = c.CurrentTarget.Size.X / c.CurrentTarget.Size.Y;

			c.RenderToAndClear(_shadowDepth);
			Gl.DrawBuffers(Gl.NONE);
			Gl.ReadBuffer(Gl.NONE);

			// todo: cascades
			float nearClip = 1f;
			float farClip = 1000;

			// Get camera frustum for the current cascade clip.
			var cam3D = c.Camera as Camera3D;
			float fov = cam3D.FieldOfView;
			var cameraProjection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.DegreesToRadians(fov), aspectRatio, nearClip, farClip);
			Matrix4x4 cameraView = c.Camera.ViewMatrix;
			Vector4[] corners = GetFrustumCornersWorldSpace(cameraView * cameraProjection, out Vector3 center);

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

			_lightViewProj = lightView * lightProjection;

			_renderingShadowMap = true;
		}

		private static Vector4[] GetFrustumCornersWorldSpace(Matrix4x4 cameraViewProj, out Vector3 frustumCenter)
		{
			Matrix4x4 inv = cameraViewProj.Inverted();

			Vector4 center = Vector4.Zero;
			var frustumCorners = new Vector4[8];
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
			return frustumCorners;
		}

		public void EndRenderShadowMap(RenderComposer c)
		{
			if (!_renderingShadowMap) return;
			AssertNotNull(_shadowDepth);

			c.RenderTo(null);
			c.FlushRenderStream();
			_renderingShadowMap = false;

			//c.SetUseViewMatrix(false);
			//c.RenderSprite(Vector3.Zero, _shadowDepth.DepthStencilAttachment);
			//c.SetUseViewMatrix(true);
		}
	}
}