#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using Assimp;
using Assimp.Configs;
using Emotion.Common;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Mesh = Emotion.Graphics.ThreeDee.Mesh;
using Quaternion = System.Numerics.Quaternion;

#endregion

namespace Emotion.PostBuildTool
{
	public class AssimpAsset : Asset
	{
		private static AssimpContext _assContext;

		public MeshEntity Entity;

		private Scene _scene;

		// Data to construct entity.
		private List<Mesh> _meshes;
		private Dictionary<string, int> _boneToIndex;
		private List<MeshMaterial> _materials;
		private List<SkeletalAnimation> _animations;

		protected override void CreateInternal(ReadOnlyMemory<byte> data)
		{
			PostProcessSteps postProcFlags = PostProcessSteps.Triangulate |
			                                 PostProcessSteps.FlipUVs |
			                                 PostProcessSteps.OptimizeGraph |
			                                 PostProcessSteps.OptimizeMeshes;

			_assContext ??= new AssimpContext();

			Scene scene;
			if (Name.Contains("gltf"))
			{
				_assContext.SetIOSystem(new AssimpAssetIOSystem());
				scene = _assContext.ImportFile(Name, postProcFlags);
			}
			else
			{
				var str = new ReadOnlyLinkedMemoryStream();
				str.AddMemory(data);
				_assContext.SetIOSystem(null);
				scene = _assContext.ImportFileFromStream(str, postProcFlags);
			}
			_scene = scene;

			var embeddedTextures = new List<Texture>();
			for (var i = 0; i < scene.TextureCount; i++)
			{
				EmbeddedTexture assTexture = scene.Textures[i];
				var embeddedTexture = new TextureAsset();
				embeddedTexture.Create(assTexture.CompressedData);
				embeddedTextures.Add(embeddedTexture.Texture);
			}

			_materials = new List<MeshMaterial>();
			for (var i = 0; i < scene.MaterialCount; i++)
			{
				Material material = scene.Materials[i];
				Color4D diffColor = material.ColorDiffuse;

				bool embeddedTexture = material.HasTextureDiffuse && embeddedTextures.Count > material.TextureDiffuse.TextureIndex;
				string textureName;
				Texture texture;
				if (embeddedTexture)
				{
					textureName = $"EmbeddedTexture{material.TextureDiffuse.TextureIndex}";
					texture = embeddedTextures[material.TextureDiffuse.TextureIndex];
				}
				else
				{
					textureName = material.TextureDiffuse.FilePath;
					string assetPath = AssetLoader.GetDirectoryName(Name);
					assetPath = AssetLoader.GetNonRelativePath(assetPath, textureName);
					var textureAsset = Engine.AssetLoader.Get<TextureAsset>(assetPath);
					texture = textureAsset?.Texture;
				}

				var emotionMaterial = new MeshMaterial
				{
					Name = material.Name,
					DiffuseColor = new Color(new Vector4(diffColor.R, diffColor.G, diffColor.B, diffColor.A)),
					DiffuseTextureName = textureName,
					DiffuseTexture = texture
				};

				_materials.Add(emotionMaterial);
			}

			_animations = new List<SkeletalAnimation>();
			ProcessAnimations(scene);

			_meshes = new List<Mesh>();
			Node rootNode = scene.RootNode;
			SkeletonAnimRigNode animRigRoot = ProcessNode(scene, rootNode);
			//animRigRoot.LocalTransform *= Matrix4x4.CreateRotationX(-90 * Maths.DEG2_RAD); // Convert to right handed Z is up.

			Entity = new MeshEntity
			{
				Name = Name,
				Meshes = _meshes.ToArray(),
				Animations = _animations.ToArray(),
				AnimationRig = animRigRoot
			};

			object scaleData = scene.RootNode.Metadata.GetValueOrDefault("UnitScaleFactor").Data;
			var scaleF = 1f;
			if (scaleData is float f) scaleF = f;
			Entity.Scale = scaleF;
		}

		private Matrix4x4 AssMatrixToEmoMatrix(Assimp.Matrix4x4 assMat)
		{
			return new Matrix4x4(
				assMat.A1, assMat.B1, assMat.C1, assMat.D1,
				assMat.A2, assMat.B2, assMat.C2, assMat.D2,
				assMat.A3, assMat.B3, assMat.C3, assMat.D3,
				assMat.A4, assMat.B4, assMat.C4, assMat.D4);
		}

		protected SkeletonAnimRigNode ProcessNode(Scene scene, Node n)
		{
			var myRigNode = new SkeletonAnimRigNode
			{
				Name = n.Name,
				LocalTransform = AssMatrixToEmoMatrix(n.Transform),
				Children = new SkeletonAnimRigNode[n.ChildCount]
			};

			for (var i = 0; i < n.MeshCount; i++)
			{
				int meshIdx = n.MeshIndices[i];
				ProcessMesh(scene.Meshes[meshIdx]);
			}

			for (var i = 0; i < n.Children.Count; i++)
			{
				Node child = n.Children[i];
				SkeletonAnimRigNode childRigNode = ProcessNode(scene, child);
				myRigNode.Children[i] = childRigNode;
			}

			return myRigNode;
		}

		protected void ProcessMesh(Assimp.Mesh m)
		{
			var newMesh = new Mesh
			{
				Name = m.Name,
				Material = _materials[m.MaterialIndex]
			};
			_meshes.Add(newMesh);

			uint[] indices = m.GetUnsignedIndices();
			var emotionIndices = new ushort[indices.Length];
			for (var i = 0; i < indices.Length; i++)
			{
				emotionIndices[i] = (ushort) indices[i];
			}

			newMesh.Indices = emotionIndices;

			var vertices = new VertexDataWithBones[m.VertexCount];
			for (var i = 0; i < m.VertexCount; i++)
			{
				// todo: check if uvs exist, vert colors, normals
				Vector3D vertex = m.Vertices[i];
				Vector3D uv = m.TextureCoordinateChannels[0][i];
				vertices[i] = new VertexDataWithBones
				{
					Vertex = new Vector3(vertex.X, vertex.Y, vertex.Z),
					UV = new Vector2(uv.X, uv.Y),

					BoneIds = new Vector4(0),
					BoneWeights = new Vector4(1, 0, 0, 0)
				};
			}

			newMesh.VerticesWithBones = vertices;

			if (!m.HasBones) return;

			_boneToIndex ??= new Dictionary<string, int>();
			var bones = new MeshBone[m.BoneCount];
			newMesh.Bones = bones;
			for (var i = 0; i < m.Bones.Count; i++)
			{
				Bone bone = m.Bones[i];
				var emBone = new MeshBone
				{
					Name = bone.Name,
					OffsetMatrix = AssMatrixToEmoMatrix(bone.OffsetMatrix)
				};
				bones[i] = emBone;

				// Check if this bone has an id assigned.
				if (!_boneToIndex.TryGetValue(bone.Name, out int boneIndex))
				{
					boneIndex = _boneToIndex.Count + 1;
					_boneToIndex.Add(bone.Name, boneIndex);
				}

				emBone.BoneIndex = boneIndex;

				for (var j = 0; j < bone.VertexWeightCount; j++)
				{
					VertexWeight boneDef = bone.VertexWeights[j];
					ref VertexDataWithBones vertex = ref vertices[boneDef.VertexID];

					// Todo: better way of doing this
					if (vertex.BoneIds.X == 0)
					{
						vertex.BoneIds.X = boneIndex;
						vertex.BoneWeights.X = boneDef.Weight;
					}
					else if (vertex.BoneIds.Y == 0)
					{
						vertex.BoneIds.Y = boneIndex;
						vertex.BoneWeights.Y = boneDef.Weight;
					}
					else if (vertex.BoneIds.Z == 0)
					{
						vertex.BoneIds.Z = boneIndex;
						vertex.BoneWeights.Z = boneDef.Weight;
					}
					else if (vertex.BoneIds.W == 0)
					{
						vertex.BoneIds.W = boneIndex;
						vertex.BoneWeights.W = boneDef.Weight;
					}
					else
					{
						Engine.Log.Warning($"Bone {bone.Name} affects more than 4 vertices in mesh {m.Name}.", "Assimp", true);
					}
				}
			}
		}

		protected void ProcessAnimations(Scene s)
		{
			for (var i = 0; i < s.AnimationCount; i++)
			{
				Animation anim = s.Animations[i];
				var channels = new SkeletonAnimChannel[anim.NodeAnimationChannelCount];
				var emotionAnim = new SkeletalAnimation
				{
					Name = anim.Name,
					Duration = (float) (anim.DurationInTicks * anim.TicksPerSecond * 1000),
					AnimChannels = channels
				};

				List<NodeAnimationChannel> boneChannels = anim.NodeAnimationChannels;
				for (var j = 0; j < boneChannels.Count; j++)
				{
					NodeAnimationChannel chan = boneChannels[j];

					var bone = new SkeletonAnimChannel
					{
						Name = chan.NodeName,
						Positions = new MeshAnimBoneTranslation[chan.PositionKeyCount],
						Rotations = new MeshAnimBoneRotation[chan.RotationKeyCount],
						Scales = new MeshAnimBoneScale[chan.ScalingKeyCount]
					};

					for (var k = 0; k < chan.PositionKeys.Count; k++)
					{
						VectorKey val = chan.PositionKeys[k];
						ref MeshAnimBoneTranslation translation = ref bone.Positions[k];
						translation.Position = new Vector3(val.Value.X, val.Value.Y, val.Value.Z);
						translation.Timestamp = (float) (val.Time * anim.TicksPerSecond * 1000);
					}

					for (var k = 0; k < chan.RotationKeys.Count; k++)
					{
						QuaternionKey val = chan.RotationKeys[k];
						ref MeshAnimBoneRotation rotation = ref bone.Rotations[k];
						rotation.Rotation = new Quaternion(val.Value.X, val.Value.Y, val.Value.Z, val.Value.W);
						rotation.Timestamp = (float) (val.Time * anim.TicksPerSecond * 1000);
					}

					for (var k = 0; k < chan.ScalingKeys.Count; k++)
					{
						VectorKey val = chan.ScalingKeys[k];
						ref MeshAnimBoneScale scale = ref bone.Scales[k];
						scale.Scale = new Vector3(val.Value.X, val.Value.Y, val.Value.Z);
						scale.Timestamp = (float) (val.Time * anim.TicksPerSecond * 1000);
					}

					channels[j] = bone;
				}

				_animations.Add(emotionAnim);
			}
		}

		protected override void DisposeInternal()
		{
		}

		public void ExportAs(string formatId, string name)
		{
			ExportDataBlob blob = _assContext.ExportToBlob(_scene, formatId);
			var str = new MemoryStream();
			blob.ToStream(str);
			Engine.AssetLoader.Save(str.ToArray(), name, false);
		}
	}
}