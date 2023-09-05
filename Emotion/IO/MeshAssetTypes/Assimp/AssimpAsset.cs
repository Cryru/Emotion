#region Using

using System.IO;
using Emotion.Common.Threading;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.Utility;
using Silk.NET.Assimp;
using AssContext = Silk.NET.Assimp.Assimp;
using AssTexture = Silk.NET.Assimp.Texture;
using AssMesh = Silk.NET.Assimp.Mesh;
using File = Silk.NET.Assimp.File;
using Mesh = Emotion.Graphics.ThreeDee.Mesh;
using Texture = Emotion.Graphics.Objects.Texture;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.Assimp
{
	/// <summary>
	/// A 3D entity loaded using Assimp. Note that this isn't optimized and is to be used in development builds only.
	/// The data will be converted to the Em3 format at runtime.
	/// </summary>
	public class AssimpAsset : Asset
	{
		public MeshEntity? Entity { get; protected set; }

		private static AssContext _assContext = AssContext.GetApi();
		private ReadOnlyMemory<byte> _thisData;

		private PostProcessSteps _postProcFlags = PostProcessSteps.Triangulate |
		                                          PostProcessSteps.JoinIdenticalVertices |
		                                          PostProcessSteps.FlipUVs |
		                                          PostProcessSteps.SortByPrimitiveType |
		                                          PostProcessSteps.OptimizeGraph |
		                                          PostProcessSteps.OptimizeMeshes;

		protected override unsafe void CreateInternal(ReadOnlyMemory<byte> data)
		{
			_thisData = data;

			// Initialize virtual file system.
			var thisStream = new AssimpStream("this");
			thisStream.AddMemory(_thisData);
			_loadedFiles.Add(thisStream);

			//_assContext.EnableVerboseLogging(1);
			//_assContext.AttachLogStream(_assContext.GetPredefinedLogStream(DefaultLogStream.Stdout, ""));

			var customIO = new FileIO
			{
				OpenProc = PfnFileOpenProc.From(OpenFileCallback),
				CloseProc = PfnFileCloseProc.From(CloseFileCallback)
			};
			
			Scene* scene = _assContext.ImportFileEx("this", (uint) _postProcFlags, ref customIO);
			if ((IntPtr) scene == IntPtr.Zero)
			{
				Engine.Log.Error(_assContext.GetErrorStringS(), "Assimp");
				return;
			}

			var isYUp = false;
			if (scene->MMetaData != null)
			{
				int? upAxis = GetMetadataInt(scene->MMetaData, "UpAxis");
				if (upAxis == 1) isYUp = true;
			}

			// Process materials, animations, and meshes.
			var materials = new List<MeshMaterial>();
			ProcessMaterials(scene, materials);

			var animations = new List<SkeletalAnimation>();
			ProcessAnimations(scene, animations);

			// Add animations from a _Animations folder.
			string myFolder = AssetLoader.GetDirectoryName(Name);
			string animFolder = AssetLoader.GetFilePathNoExtension(Name) + "_Animations/";
			string[] assetsInFolder = Engine.AssetLoader.GetAssetsInFolder(animFolder);
			for (var i = 0; i < assetsInFolder.Length; i++)
			{
				string asset = assetsInFolder[i];
				Scene* otherAssetScene = _assContext.ImportFileEx(asset.Replace(myFolder, ""), (uint) _postProcFlags, ref customIO);
				if (otherAssetScene != null) ProcessAnimations(otherAssetScene, animations);
			}

			var meshes = new List<Mesh>();

			Node* rootNode = scene->MRootNode;
			SkeletonAnimRigNode? animRigRoot = WalkNodesForSkeleton( rootNode);
			SkeletonAnimRigRoot? animRigAsRoot = animRigRoot != null ? SkeletonAnimRigRoot.PromoteNode(animRigRoot) : null;

			WalkNodesForMeshes(scene, rootNode, meshes, materials);

			// Convert to right handed Z is up.
			if (animRigAsRoot != null && (Name.Contains(".gltf") || isYUp))
				animRigAsRoot.LocalTransform *= Matrix4x4.CreateRotationX(90 * Maths.DEG2_RAD);

			Entity = new MeshEntity
			{
				Name = Name,
				Meshes = meshes.ToArray(),
				Animations = animations.ToArray(),
				AnimationRig = animRigAsRoot
			};

			// Properties
			float scaleF = GetMetadataFloat(rootNode->MMetaData, "UnitScaleFactor") ?? 100f;
			Entity.Scale = scaleF;

			// Clear virtual file system
			for (var i = 0; i < _loadedFiles.Count; i++)
			{
				AssimpStream file = _loadedFiles[i];
				file.Dispose();
			}

			_loadedFiles.Clear();
			_thisData = null;
		}

		#region IO

		private List<AssimpStream> _loadedFiles = new();

		private unsafe File* OpenFileCallback(FileIO* arg0, byte* arg1, byte* arg2)
		{
			string readMode = NativeHelpers.StringFromPtr((IntPtr) arg2);
			if (readMode != "rb")
			{
				Engine.Log.Error("Only read-binary file mode is supported.", "Assimp");
				return null;
			}

			string fileName = NativeHelpers.StringFromPtr((IntPtr) arg1);
			for (var i = 0; i < _loadedFiles.Count; i++)
			{
				AssimpStream alreadyOpenFile = _loadedFiles[i];
				if (alreadyOpenFile.Name == fileName) return (File*) alreadyOpenFile.Memory;
			}

			string assetPath = AssetLoader.GetDirectoryName(Name);
			fileName = AssetLoader.GetNonRelativePath(assetPath, fileName);
			var byteAsset = Engine.AssetLoader.Get<OtherAsset>(fileName, false);
			if (byteAsset == null) return null;

			var assimpStream = new AssimpStream(fileName);
			assimpStream.AddMemory(byteAsset.Content);
			_loadedFiles.Add(assimpStream);
			return (File*) assimpStream.Memory;
		}

		private unsafe void CloseFileCallback(FileIO* arg0, File* arg1)
		{
			// When Assimp wants to close a file we'll keep it loaded
			// but reset its position instead as it can be requested again and
			// we want to reduce IO. At the end of the asset parsing all loaded
			// files will be properly unloaded.
			var filePtr = (IntPtr) arg1;
			for (var i = 0; i < _loadedFiles.Count; i++)
			{
				AssimpStream file = _loadedFiles[i];
				if (file.Memory == filePtr)
				{
					file.Position = 0;
					return;
				}
			}
		}

		#endregion

		#region Materials

		private unsafe void ProcessMaterials(Scene* scene, List<MeshMaterial> list)
		{
			var embeddedTextures = new List<Texture>();
			for (var i = 0; i < scene->MNumTextures; i++)
			{
				AssTexture* assTexture = scene->MTextures[i];

				if (assTexture->MHeight == 0) // compressed
				{
					// Data in embedded png
					var dataAsByte = new ReadOnlySpan<byte>(assTexture->PcData, (int) assTexture->MWidth);
					byte[] dataManaged = dataAsByte.ToArray();
					var embeddedTexture = new TextureAsset();
					embeddedTexture.Create(dataManaged);
					embeddedTextures.Add(embeddedTexture.Texture);
				}
				else
				{
					// Texture data is ARGB8888
					var dataAsByte = new ReadOnlySpan<byte>(assTexture->PcData, (int) (assTexture->MWidth * assTexture->MHeight * 4));

					byte[] dataManaged = dataAsByte.ToArray();
					Texture? embeddedTexture = Texture.NonGLThreadInitialize(new Vector2(assTexture->MWidth, assTexture->MHeight));
					GLThread.ExecuteGLThreadAsync(() =>
					{
						Texture.NonGLThreadInitializedCreatePointer(embeddedTexture);
						embeddedTexture.Upload(embeddedTexture.Size, dataManaged);
					});
					embeddedTextures.Add(embeddedTexture);
				}
			}

			for (var i = 0; i < scene->MNumMaterials; i++)
			{
				Material* material = scene->MMaterials[i];
				string materialName = GetMaterialString(material, AssContext.DefaultMaterialName);
				Color diffColor = GetMaterialColor(material, AssContext.MaterialColorDiffuseBase);

				Texture? diffuseTexture = null;
				bool hasDiffuseTexture = GetMaterialTexture(material, TextureType.Diffuse, out uint idx, out string? diffuseTextureName);
				if (hasDiffuseTexture)
				{
					bool embeddedTexture = embeddedTextures.Count > idx;
					if (embeddedTexture)
					{
						diffuseTextureName = $"EmbeddedTexture{idx}";
						diffuseTexture = embeddedTextures[(int) idx];
					}
					else if (!string.IsNullOrEmpty(diffuseTextureName))
					{
						string assetPath = AssetLoader.GetDirectoryName(Name);
						assetPath = AssetLoader.GetNonRelativePath(assetPath, diffuseTextureName);
						var textureAsset = Engine.AssetLoader.Get<TextureAsset>(assetPath, false);
						diffuseTexture = textureAsset?.Texture;
					}
				}

				var emotionMaterial = new MeshMaterial
				{
					Name = materialName,
					DiffuseColor = diffColor,
					DiffuseTextureName = diffuseTextureName,
					DiffuseTexture = diffuseTexture
				};

				list.Add(emotionMaterial);
			}
		}

		private static unsafe string GetMaterialString(Material* material, string key)
		{
			var str = new AssimpString();
			_assContext.GetMaterialString(material, key, 0, 0, ref str);
			return str.AsString;
		}

		private static unsafe Color GetMaterialColor(Material* material, string key)
		{
			for (var i = 0; i < material->MNumProperties; i++)
			{
				MaterialProperty* prop = material->MProperties[i];
				if (prop->MKey == key && prop->MType == PropertyTypeInfo.Float)
				{
					byte* rawValue = prop->MData;

					if (prop->MDataLength == sizeof(Vector4)) // RGBA
					{
						var v4Span = new ReadOnlySpan<Vector4>(rawValue, 1);
						Vector4 v4 = v4Span[0];
						return new Color(v4);
					}

					if (prop->MDataLength == sizeof(Vector3)) // RGB
					{
						var v3Span = new ReadOnlySpan<Vector3>(rawValue, 1);
						Vector3 v3 = v3Span[0];
						return new Color(v3);
					}
				}
			}

			return Color.White;
		}

		private static unsafe bool GetMaterialTexture(Material* material, TextureType key, out uint idx, out string? texturePath)
		{
			idx = 0;
			texturePath = null;

			var path = new AssimpString();
			var mapping = TextureMapping.UV;
			Return result = _assContext.GetMaterialTexture(material, key, idx, ref path, ref mapping,
				(uint*) 0, (float*) 0, (TextureOp*) 0, (TextureMapMode*) 0, (uint*) 0);
			if (result == Return.Success)
			{
				texturePath = path.AsString;
				return true;
			}

			return false;
		}

		#endregion

		#region Animations

		protected unsafe SkeletonAnimRigNode? WalkNodesForSkeleton(Node* n)
		{
			if ((IntPtr) n == IntPtr.Zero) return null;

			string nodeName = n->MName.AsString;

			var myNode = new SkeletonAnimRigNode
			{
				Name = nodeName,
				LocalTransform = Matrix4x4.Transpose(n->MTransformation),
				Children = new SkeletonAnimRigNode[n->MNumChildren],
				DontAnimate = nodeName.Contains("$AssimpFbx$")
			};

			AssertNotNull(myNode.Children);
			for (var i = 0; i < n->MNumChildren; i++)
			{
				Node* child = n->MChildren[i];
				SkeletonAnimRigNode? childRigNode = WalkNodesForSkeleton(child);
				if (childRigNode == null) continue;
				myNode.Children[i] = childRigNode;
			}

			return myNode;
		}

		private unsafe void ProcessAnimations(Scene* scene, List<SkeletalAnimation> list)
		{
			var unnamedAnimations = 0;
			for (var i = 0; i < scene->MNumAnimations; i++)
			{
				Animation* anim = scene->MAnimations[i];

				var channels = new SkeletonAnimChannel[anim->MNumChannels];
				string animName = anim->MName.AsString;
				var emotionAnim = new SkeletalAnimation
				{
					Name = string.IsNullOrEmpty(animName) ? $"UnnamedAnimation{unnamedAnimations++}" : animName,
					AnimChannels = channels
				};

				// wtf is happening here, idk
				var speedFactor = (float) (anim->MTicksPerSecond == 1 ? 1f / 1000f : anim->MTicksPerSecond == 1000 ? 1 : anim->MTicksPerSecond);
				Assert(!float.IsNaN(speedFactor));
				emotionAnim.Duration = (float) (anim->MDuration / speedFactor);

				list.Add(emotionAnim);

				for (var j = 0; j < channels.Length; j++)
				{
					NodeAnim* channel = anim->MChannels[j];

					var bone = new SkeletonAnimChannel
					{
						Name = channel->MNodeName.AsString,
						Positions = new MeshAnimBoneTranslation[channel->MNumPositionKeys],
						Rotations = new MeshAnimBoneRotation[channel->MNumRotationKeys],
						Scales = new MeshAnimBoneScale[channel->MNumScalingKeys]
					};

					for (var k = 0; k < channel->MNumPositionKeys; k++)
					{
						VectorKey val = channel->MPositionKeys[k];
						ref MeshAnimBoneTranslation translation = ref bone.Positions[k];
						translation.Position = val.MValue;
						translation.Timestamp = (float) (val.MTime / speedFactor);
					}

					for (var k = 0; k < channel->MNumRotationKeys; k++)
					{
						QuatKey val = channel->MRotationKeys[k];
						ref MeshAnimBoneRotation rotation = ref bone.Rotations[k];
						rotation.Rotation = val.MValue.AsQuaternion;
						rotation.Timestamp = (float) (val.MTime / speedFactor);
					}

					for (var k = 0; k < channel->MNumScalingKeys; k++)
					{
						VectorKey val = channel->MScalingKeys[k];
						ref MeshAnimBoneScale scale = ref bone.Scales[k];
						scale.Scale = val.MValue;
						scale.Timestamp = (float) (val.MTime / speedFactor);
					}

					channels[j] = bone;
				}
			}
		}

		#endregion

		#region Meshes

		protected unsafe void WalkNodesForMeshes(Scene* scene, Node* n, List<Mesh> list, List<MeshMaterial> materials)
		{
			if ((IntPtr) n == IntPtr.Zero) return;

			for (var i = 0; i < n->MNumMeshes; i++)
			{
				uint meshIdx = n->MMeshes[i];
				AssMesh* mesh = scene->MMeshes[meshIdx];
				Mesh emotionMesh = ProcessMesh(mesh, materials);
				list.Add(emotionMesh);
			}

			for (var i = 0; i < n->MNumChildren; i++)
			{
				Node* child = n->MChildren[i];
				WalkNodesForMeshes(scene, child, list, materials);
			}
		}

		protected unsafe Mesh ProcessMesh(AssMesh* m, List<MeshMaterial> materials)
		{
			var newMesh = new Mesh
			{
				Name = m->MName.AsString,
				Material = materials[(int) m->MMaterialIndex]
			};

			// Collect indices
			uint indicesCount = 0;
			for (var i = 0; i < m->MNumFaces; i++)
			{
				ref Face face = ref m->MFaces[i];
				indicesCount += face.MNumIndices;
			}

			// dynamically change type based on vertex count?
			// is it realistic to have meshes with 65k+ vertices?
			var emotionIndices = new ushort[indicesCount];
			var emoIdx = 0;
			for (var p = 0; p < m->MNumFaces; p++)
			{
				ref Face face = ref m->MFaces[p];
				if ((IntPtr) face.MIndices == IntPtr.Zero) continue;

				for (var j = 0; j < face.MNumIndices; j++)
				{
					emotionIndices[emoIdx] = (ushort) face.MIndices[j];
					emoIdx++;
				}
			}

			newMesh.Indices = emotionIndices;

			// Copy vertices (todo: separate path for boneless)
			var vertices = new VertexDataWithBones[m->MNumVertices];
			for (var i = 0; i < m->MNumVertices; i++)
			{
				// todo: check if uvs exist, vert colors, normals
				ref Vector3 assVertex = ref m->MVertices[i];

				var uv = new Vector2(0, 0);
				if ((IntPtr) m->MNumUVComponents != IntPtr.Zero && m->MNumUVComponents[0] >= 2)
				{
					Vector3 uv3 = m->MTextureCoords[0][i];
					uv = new Vector2(uv3.X, uv3.Y);
				}

				vertices[i] = new VertexDataWithBones
				{
					Vertex = assVertex,
					UV = uv,

					BoneIds = new Vector4(0, 0, 0, 0),
					BoneWeights = new Vector4(1, 0, 0, 0)
				};
			}

			newMesh.VerticesWithBones = vertices;

			if (m->MNumBones == 0) return newMesh;

			var boneToIndex = new Dictionary<string, int>();
			var bones = new MeshBone[m->MNumBones];
			newMesh.Bones = bones;

			for (var i = 0; i < m->MNumBones; i++)
			{
				Bone* bone = m->MBones[i];
				var emBone = new MeshBone
				{
					Name = bone->MName.AsString,
					OffsetMatrix = Matrix4x4.Transpose(bone->MOffsetMatrix)
				};
				bones[i] = emBone;

				// Check if this bone has an id assigned.
				if (!boneToIndex.TryGetValue(bone->MName.AsString, out int boneIndex))
				{
					// 0 is identity
					boneIndex = boneToIndex.Count + 1;
					boneToIndex.Add(bone->MName.AsString, boneIndex);
				}

				emBone.BoneIndex = boneIndex;

				for (var j = 0; j < bone->MNumWeights; j++)
				{
					ref VertexWeight boneDef = ref bone->MWeights[j];

					if (boneDef.MVertexId > vertices.Length - 1) continue;
					if (boneDef.MWeight == 0) continue;

					ref VertexDataWithBones vertex = ref vertices[boneDef.MVertexId];

					// Todo: better way of doing this
					var found = false;
					for (var dim = 0; dim < 4; dim++)
					{
						if (vertex.BoneIds[dim] != 0) continue;

						vertex.BoneIds[dim] = boneIndex;
						vertex.BoneWeights[dim] = boneDef.MWeight;
						found = true;
						break;
					}

					// If no free bone weight replace the lowest weight if it is lower than this one.
					if (!found)
					{
						Engine.Log.Warning($"Bone {bone->MName.AsString} affects more than 4 vertices in mesh {m->MName.AsString}.", "Assimp", true);

						var lowestWeight = float.MaxValue;
						int lowestWeightIdx = -1;

						for (var dim = 0; dim < 4; dim++)
						{	
							float thisWeight = vertex.BoneWeights[dim];
							if (thisWeight < lowestWeight && thisWeight != 0)
							{
								lowestWeight = thisWeight;
								lowestWeightIdx = dim;
							}
						}

						if (lowestWeight < boneDef.MWeight)
						{
							vertex.BoneIds[lowestWeightIdx] = boneIndex;
							vertex.BoneWeights[lowestWeightIdx] = boneDef.MWeight;
						}
					}
				}
			}

			// Normalize bone weights to 1.
			for (var i = 0; i < vertices.Length; i++)
			{
                // The code below is basically
                // float vecLength = vertex.BoneWeights.Length();
                // float ratio = 1f / vecLength;
                // vertex.BoneWeights = vertex.BoneWeights * ratio;
				// But written out with doubles due to some weights being so tiny.

                ref VertexDataWithBones vertex = ref vertices[i];
				double vecLength = 0;
				for (int c = 0; c < 4; c++)
				{
					double weight = vertex.BoneWeights[c];
					if (weight == 0) continue;
					vecLength += weight * weight;
                }
				vecLength = Math.Sqrt(vecLength);

				double ratio = 1.0 / vecLength;
                for (int c = 0; c < 4; c++)
                {
                    double weight = vertex.BoneWeights[c];
                    if (weight == 0) continue;

					weight = weight * ratio;

					float weightFloat = (float)weight;
					if (weightFloat == 0) weightFloat = Maths.EPSILON;
					vertex.BoneWeights[c] = weightFloat;
                }
            }

			return newMesh;
		}

		#endregion

		private unsafe float? GetMetadataFloat(Metadata* meta, string key)
		{
			if ((IntPtr) meta == IntPtr.Zero) return null;

			for (var i = 0; i < meta->MNumProperties; i++)
			{
				AssimpString k = meta->MKeys[i];
				if (k.AsString == key && meta->MValues->MType == MetadataType.Float)
					return *(float*) meta->MValues->MData;
			}

			return null;
		}

		private unsafe int? GetMetadataInt(Metadata* meta, string key)
		{
			if ((IntPtr) meta == IntPtr.Zero) return null;

			for (var i = 0; i < meta->MNumProperties; i++)
			{
				AssimpString k = meta->MKeys[i];
				if (k.AsString == key && meta->MValues->MType == MetadataType.Int32)
					return *(int*) meta->MValues->MData;
			}

			return null;
		}

		protected override void DisposeInternal()
		{
		}

		public unsafe void ExportAs(Scene* scene, string formatId, string name)
		{
			ExportDataBlob* blob = _assContext.ExportSceneToBlob(scene, formatId, (uint) _postProcFlags);
			var str = new MemoryStream();
			using (var writer = new BinaryWriter(str))
			{
				while (true)
				{
					if (blob == null) return;

					bool hasNext = blob->Next != null;

					writer.Write(blob->Name.AsString);
					writer.Write(blob->Size);
					var data = new Span<byte>(blob->Data, (int) blob->Size);
					writer.Write(data);
					writer.Write(hasNext);

					if (hasNext)
					{
						blob = blob->Next;
						continue;
					}

					break;
				}
			}

			Engine.AssetLoader.Save(str.ToArray(), name, false);
		}
	}
}