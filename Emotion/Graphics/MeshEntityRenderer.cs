#nullable enable

#region Using

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
using Silk.NET.Maths;


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
				Gl.CullFace(_renderingShadowMap ? CullFaceMode.Front : CullFaceMode.Back);
				Gl.FrontFace(FrontFaceDirection.Ccw);
			}

			// Slope scale depth bias
			if (_renderingShadowMap)
			{
				Gl.Enable(EnableCap.PolygonOffsetFill);
				Gl.PolygonOffset(1f, 1f);
			}
			else
			{
				Gl.Disable(EnableCap.PolygonOffsetFill);
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
		private Matrix4x4 _renderingShadowMapCurrentLightViewProj;
		private string[]? cascadePlaneFarZUniformNames;
		private string[]? cascadeLightProjUniformNames;
		private static float _shadowMapResolution = 1024;

		private FrameBuffer? pingPongBlur1;
		private FrameBuffer? pingPongBlur2;
		private ShaderAsset? _blurShader;

		private class ShadowCascade
		{
			public FrameBuffer Buffer;
			public float NearZ;
			public float FarZ;
			public Matrix4x4 LightViewProj;

			public ShadowCascade(float nearZ, float farZ)
			{
				var resolution = new Vector2(_shadowMapResolution);
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
				new ShadowCascade(0.1f, 200f),
				new ShadowCascade(200f, 800f),
				new ShadowCascade(800f, 4000f)
			};
			Assert(_shadowCascades.Length == _shadowCascadeCount);

			cascadePlaneFarZUniformNames = new string[_shadowCascadeCount];
			cascadeLightProjUniformNames = new string[_shadowCascadeCount];

			for (var i = 0; i < _shadowCascadeCount; i++)
			{
				cascadePlaneFarZUniformNames[i] = $"cascadePlaneFarZ[{i}]";
				cascadeLightProjUniformNames[i] = $"cascadeLightProj[{i}]";
			}

			pingPongBlur1 = new FrameBuffer(new Vector2(_shadowMapResolution / 2f)).WithDepth(true);
			pingPongBlur2 = new FrameBuffer(new Vector2(_shadowMapResolution / 2f)).WithDepth(true);

			_blurShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BlurShadowMap.xml");

			_initializedShadowMapObjects = true;
		}

		private void DownscaleAndBlurShadowMap(RenderComposer c, FrameBuffer shadowmap)
		{
			if (!_initializedShadowMapObjects || _blurShader == null) return;
			AssertNotNull(pingPongBlur1);
			AssertNotNull(pingPongBlur2);

			c.SetUseViewMatrix(false);
			c.SetShader(_blurShader.Shader);
			c.RenderToAndClear(pingPongBlur1);
			_blurShader.Shader.SetUniformVector2("direction", new Vector2(1f, 0));
			c.RenderSprite(Vector3.Zero, pingPongBlur1.Size, shadowmap.DepthStencilAttachment);
			c.RenderTo(null);
			c.RenderToAndClear(shadowmap);
			_blurShader.Shader.SetUniformVector2("direction", new Vector2(0, 1f));
			c.RenderSprite(Vector3.Zero, shadowmap.Size, pingPongBlur1.DepthStencilAttachment);
			//c.RenderTo(null);
			//c.SetShader(null);

			//c.RenderToAndClear(shadowmap);
			//c.RenderSprite(Vector3.Zero, shadowmap.Size, pingPongBlur2.DepthStencilAttachment);
			c.RenderTo(null);
			c.SetShader(null);
			c.SetUseViewMatrix(true);
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
			//Span<Vector3> corners = stackalloc Vector3[8];
			Span<Vector3> corners = new Vector3[8];
			GetFrustumCornersWorldSpace(corners, cameraView * cameraProjection, out Vector3 center);

			// Furthest two points in the frustum tell us how big it is, and the radius is /2f
			float radius = (corners[0] - corners[6]).Length() / 2f;

			// The resolution of the buffer devided by the covered space of the frustum (radius * 2f)
			// gives us how many texels cover a world space unit.
			float texelsPerUnit = cascade.Buffer.DepthStencilAttachment.Size.X / (radius * 2f);

			Matrix4x4 scaleMat = Matrix4x4.CreateScale(texelsPerUnit);

			Vector3 baseLookAt = model.SunDirection;
			Matrix4x4 lookatCorrected = Matrix4x4.CreateLookAt(Vector3.Zero, baseLookAt, RenderComposer.Up);
			lookatCorrected = scaleMat * lookatCorrected;
			Matrix4x4 lookAtInv = lookatCorrected.Inverted();

			// Round the frustum center to texels and then return it back to world space.
			center = Vector3.Transform(center, lookatCorrected);
			center.X = (float)MathF.Floor(center.X);
			center.Y = (float)Math.Floor(center.Y);
			center = Vector3.Transform(center, lookAtInv);

			// The light view matrix looks at the center of the frustum.
			Vector3 eye = center + (model.SunDirection * radius * 2f);
			var lightView = Matrix4x4.CreateLookAt(eye, center, Vector3.UnitZ);

			// The projection of the light encompasses the frustum in a square.
			var lightProjection = Matrix4x4.CreateOrthographicOffCenter(-radius, radius, -radius, radius, -radius * 6, radius * 6);

			cascade.LightViewProj = lightView * lightProjection;
			_renderingShadowMapCurrentLightViewProj = cascade.LightViewProj;
			_renderingShadowMap = true;
		}

		private static void GetFrustumCornersWorldSpace(Span<Vector3> frustumCorners, Matrix4x4 cameraViewProj, out Vector3 frustumCenter)
		{
			Matrix4x4 inv = cameraViewProj.Inverted();

			frustumCorners[0] = new Vector3(-1f, +1f, 0f);
			frustumCorners[1] = new Vector3(+1f, +1f, 0f);
			frustumCorners[2] = new Vector3(+1f, -1f, 0f);
			frustumCorners[3] = new Vector3(-1f, -1f, 0f);

			frustumCorners[4] = new Vector3(-1f, +1f, 1f);
			frustumCorners[5] = new Vector3(+1f, +1f, 1f);
			frustumCorners[6] = new Vector3(+1f, -1f, 1f);
			frustumCorners[7] = new Vector3(-1f, -1f, 1f);

			Vector3 center = Vector3.Zero;
			for (int i = 0; i < frustumCorners.Length; i++)
			{
				var corner = frustumCorners[i];
				Vector4 worldSpace = Vector4.Transform(new Vector4(corner, 1.0f), inv);
				Vector4 cartPt = worldSpace / worldSpace.W;

				Vector3 cartVec3 = cartPt.ToVec3();
				frustumCorners[i] = cartVec3;
				center += cartVec3;
			}

			//Vector4 center = Vector4.Zero;
			//var idx = 0;
			//for (var x = 0; x < 2; ++x)
			//{
			//	for (var y = 0; y < 2; ++y)
			//	{
			//		for (var z = 0; z < 2; ++z)
			//		{
			//			var ndcPos = new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f);
			//			Vector4 pt = Vector4.Transform(ndcPos, inv);
			//			Vector4 cartPt = pt / pt.W;
			//			frustumCorners[idx++] = cartPt;
			//			center += cartPt;
			//		}
			//	}
			//}

			frustumCenter = center / 8f;
		}

		public void EndRenderShadowMap(RenderComposer c)
		{
			if (!_renderingShadowMap) return;
			AssertNotNull(_shadowCascades);

			c.RenderTo(null);
			c.FlushRenderStream();

			for (int i = 0; i < _shadowCascades.Length; i++)
			{
				var cascade = _shadowCascades[i];
				DownscaleAndBlurShadowMap(c, cascade.Buffer);
			}

			_renderingShadowMap = false;
		}
	}
}