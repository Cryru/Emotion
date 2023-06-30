#region Using

using System.IO;
using Emotion.Common.Threading;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.IO.MeshAssetTypes
{
    /// <summary>
    /// An asset containing a MeshEntity serialized in binary format.
    /// This file can contain meshes, animations, textures, and materials that
    /// together make up a 3D model. The file extension is .em3 and the format is
    /// Emotion engine specific.
    /// </summary>
    public class EmotionMeshAsset : Asset
    {
        public MeshEntity Entity { get; set; }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            Entity = EntityFromByteArray(data);
        }

        protected override void DisposeInternal()
        {
            Entity = null;
            // todo: maybe clean up entity textures or w/e?
        }

        public static MeshEntity EntityFromByteArray(ReadOnlyMemory<byte> byteArray)
        {
            var entity = new MeshEntity();

            var memoryStream = new ReadOnlyMemoryStream(byteArray);
            var reader = new BinaryReader(memoryStream);

            char check = reader.ReadChar();
            char check2 = reader.ReadChar();
            char check3 = reader.ReadChar();

            Debug.Assert(check == 'E' && check2 == 'M' && check3 == '3');

            byte version = reader.ReadByte();
            if (version != 1) return null;

            entity.Name = reader.ReadString();
            entity.Scale = reader.ReadSingle();

            var materialMap = new Dictionary<string, MeshMaterial>();
            var textureMap = new Dictionary<string, Texture>();

            int meshesCount = reader.ReadInt32();
            var meshArray = new Mesh[meshesCount];
            entity.Meshes = meshArray;
            for (var i = 0; i < meshesCount; i++)
            {
                var newMesh = new Mesh
                {
                    Name = reader.ReadString()
                };

                string materialName = reader.ReadString();
                if (materialMap.TryGetValue(materialName, out MeshMaterial mat))
                {
                    newMesh.Material = mat;
                }
                else
                {
                    mat = new MeshMaterial
                    {
                        Name = materialName,
                        DiffuseColor = new Color(reader.ReadUInt32())
                    };

                    bool hasDiffuseTexture = reader.ReadBoolean();
                    if (hasDiffuseTexture)
                    {
                        string textureName = reader.ReadString();
                        mat.DiffuseTextureName = textureName;

                        if (textureMap.TryGetValue(textureName, out Texture diffuseTexture))
                        {
                            mat.DiffuseTexture = diffuseTexture;
                        }
                        else
                        {
                            int textureByteLength = reader.ReadInt32();
                            float width = reader.ReadSingle();
                            float height = reader.ReadSingle();
                            int textureFormat = reader.ReadInt32();
                            Texture t = Texture.NonGLThreadInitialize(new Vector2(width, height));
                            mat.DiffuseTexture = t;

                            byte[] data = reader.ReadBytes(textureByteLength);
                            GLThread.ExecuteGLThreadAsync(() =>
                            {
                                Texture.NonGLThreadInitializedCreatePointer(t);
                                t.Upload(t.Size, data, (PixelFormat) textureFormat);
                            });

                            textureMap.Add(textureName, t);
                        }
                    }

                    newMesh.Material = mat;
                    materialMap.Add(materialName, mat);
                }

                int vertexFormat = reader.ReadByte();
                int length = reader.ReadInt32();
                if (vertexFormat == 0) // Normal vertices
                {
                    var vertices = new VertexData[length];

                    for (var j = 0; j < length; j++)
                    {
                        ref VertexData vert = ref vertices[j];

                        vert.Vertex = ReadVector3(reader);
                        vert.UV = ReadVector2(reader);
                        vert.Color = reader.ReadUInt32();
                    }

                    newMesh.Vertices = vertices;
                }
                else // Vertices with bone data
                {
                    var vertices = new VertexDataWithBones[length];

                    for (var j = 0; j < length; j++)
                    {
                        ref VertexDataWithBones vert = ref vertices[j];

                        vert.Vertex = ReadVector3(reader);
                        vert.UV = ReadVector2(reader);
                        vert.BoneIds = ReadVector4(reader);
                        vert.BoneWeights = ReadVector4(reader);
                    }

                    newMesh.VerticesWithBones = vertices;
                }

                int indicesLength = reader.ReadInt32();
                var indices = new ushort[indicesLength];
                for (var j = 0; j < indicesLength; j++)
                {
                    indices[j] = reader.ReadUInt16();
                }

                newMesh.Indices = indices;

                int bonesLength = reader.ReadInt32();
                MeshBone[] bones = bonesLength > 0 ? new MeshBone[bonesLength] : null;
                for (var j = 0; j < bonesLength; j++)
                {
                    var bone = new MeshBone
                    {
                        Name = reader.ReadString(),
                        BoneIndex = reader.ReadInt32(),
                        OffsetMatrix = ReadMatrix4X4(reader)
                    };
                    bones![j] = bone;
                }

                newMesh.Bones = bones;

                meshArray[i] = newMesh;
            }

            int animationCount = reader.ReadInt32();
            entity.Animations = animationCount > 0 ? new SkeletalAnimation[animationCount] : null;
            for (var i = 0; i < animationCount; i++)
            {
                var newAnim = new SkeletalAnimation
                {
                    Name = reader.ReadString(),
                    Duration = reader.ReadSingle()
                };

                int channelCount = reader.ReadInt32();
                newAnim.AnimChannels = new SkeletonAnimChannel[channelCount];
                for (var j = 0; j < channelCount; j++)
                {
                    var animChannel = new SkeletonAnimChannel
                    {
                        Name = reader.ReadString()
                    };

                    int positionsLength = reader.ReadInt32();
                    var positions = new MeshAnimBoneTranslation[positionsLength];
                    for (var k = 0; k < positionsLength; k++)
                    {
                        positions[k] = new MeshAnimBoneTranslation
                        {
                            Timestamp = reader.ReadSingle(),
                            Position = ReadVector3(reader)
                        };
                    }

                    animChannel.Positions = positions;

                    int rotationsLength = reader.ReadInt32();
                    var rotations = new MeshAnimBoneRotation[rotationsLength];
                    for (var k = 0; k < rotationsLength; k++)
                    {
                        rotations[k] = new MeshAnimBoneRotation
                        {
                            Timestamp = reader.ReadSingle(),
                            Rotation = ReadQuart(reader)
                        };
                    }

                    animChannel.Rotations = rotations;

                    int scalesLength = reader.ReadInt32();
                    var scales = new MeshAnimBoneScale[scalesLength];
                    for (var k = 0; k < scales.Length; k++)
                    {
                        scales[k] = new MeshAnimBoneScale
                        {
                            Timestamp = reader.ReadSingle(),
                            Scale = ReadVector3(reader)
                        };
                    }

                    animChannel.Scales = scales;

                    newAnim.AnimChannels[j] = animChannel;
                }

                entity.Animations![i] = newAnim;
            }

            bool hasRig = reader.ReadBoolean();
            if (hasRig) entity.AnimationRig = ReadSkeletonRig(reader);

            reader.Dispose();

            return entity;
        }

        public static byte[] EntityToByteArray(MeshEntity entity)
        {
            Debug.Assert(GLThread.IsGLThread());

            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);
            writer.Write('E');
            writer.Write('M');
            writer.Write('3');

            writer.Write((byte) 1); // Version

            writer.Write(entity.Name);
            writer.Write(entity.Scale);

            var writtenMaterials = new HashSet<string>();
            var writtenTextures = new HashSet<string>();

            writer.Write(entity.Meshes.Length);
            for (var i = 0; i < entity.Meshes.Length; i++)
            {
                Mesh mesh = entity.Meshes[i];
                writer.Write(mesh.Name);

                string materialName = mesh.Material.Name;
                writer.Write(materialName);
                if (!writtenMaterials.Contains(materialName))
                {
                    writer.Write(mesh.Material.DiffuseColor.ToUint());
                    writer.Write(mesh.Material.DiffuseTexture != null);
                    if (mesh.Material.DiffuseTexture != null)
                    {
                        string diffuseTextureName = mesh.Material.DiffuseTextureName!;
                        writer.Write(diffuseTextureName);
                        if (!writtenTextures.Contains(diffuseTextureName))
                        {
                            byte[] data = mesh.Material.DiffuseTexture.Download();
                            writer.Write(data.Length);

                            writer.Write(mesh.Material.DiffuseTexture.Size.X);
                            writer.Write(mesh.Material.DiffuseTexture.Size.Y);

                            writer.Write((int) mesh.Material.DiffuseTexture.PixelFormat);

                            writer.Write(data); // todo: compress this or something

                            writtenTextures.Add(diffuseTextureName);
                        }
                    }

                    writtenMaterials.Add(materialName);
                }

                if (mesh.VerticesWithBones == null)
                {
                    writer.Write((byte) 0);
                    writer.Write(mesh.Vertices!.Length);
                    for (var j = 0; j < mesh.Vertices.Length; j++)
                    {
                        VertexData vert = mesh.Vertices[j];

                        WriteVector3(writer, ref vert.Vertex);
                        WriteVector2(writer, ref vert.UV);

                        writer.Write(vert.Color);
                    }
                }
                else
                {
                    writer.Write((byte) 1);
                    writer.Write(mesh.VerticesWithBones.Length);
                    for (var j = 0; j < mesh.VerticesWithBones.Length; j++)
                    {
                        VertexDataWithBones vert = mesh.VerticesWithBones[j];

                        WriteVector3(writer, ref vert.Vertex);
                        WriteVector2(writer, ref vert.UV);
                        WriteVector4(writer, ref vert.BoneIds);
                        WriteVector4(writer, ref vert.BoneWeights);
                    }
                }

                writer.Write(mesh.Indices.Length);
                for (var j = 0; j < mesh.Indices.Length; j++)
                {
                    writer.Write(mesh.Indices[j]);
                }

                int bonesLength = mesh.Bones?.Length ?? 0;
                writer.Write(bonesLength);
                for (var j = 0; j < bonesLength; j++)
                {
                    MeshBone bone = mesh.Bones![j];
                    writer.Write(bone.Name);
                    writer.Write(bone.BoneIndex);
                    WriteMatrix4X4(writer, ref bone.OffsetMatrix);
                }
            }

            int animationCount = entity.Animations?.Length ?? 0;
            writer.Write(animationCount);
            for (var i = 0; i < animationCount; i++)
            {
                SkeletalAnimation animation = entity.Animations![i];
                writer.Write(animation.Name);
                writer.Write(animation.Duration);

                writer.Write(animation.AnimChannels.Length);
                for (var j = 0; j < animation.AnimChannels.Length; j++)
                {
                    SkeletonAnimChannel channel = animation.AnimChannels[j];
                    writer.Write(channel.Name);

                    MeshAnimBoneTranslation[] positions = channel.Positions;
                    writer.Write(positions.Length);
                    for (var k = 0; k < positions.Length; k++)
                    {
                        ref MeshAnimBoneTranslation position = ref positions[k];
                        writer.Write(position.Timestamp);
                        WriteVector3(writer, ref position.Position);
                    }

                    MeshAnimBoneRotation[] rotations = channel.Rotations;
                    writer.Write(rotations.Length);
                    for (var k = 0; k < rotations.Length; k++)
                    {
                        ref MeshAnimBoneRotation rotation = ref rotations[k];
                        writer.Write(rotation.Timestamp);
                        WriteQuart(writer, ref rotation.Rotation);
                    }

                    MeshAnimBoneScale[] scales = channel.Scales;
                    writer.Write(scales.Length);
                    for (var k = 0; k < scales.Length; k++)
                    {
                        ref MeshAnimBoneScale scale = ref scales[k];
                        writer.Write(scale.Timestamp);
                        WriteVector3(writer, ref scale.Scale);
                    }
                }
            }

            writer.Write(entity.AnimationRig != null);
            if (entity.AnimationRig != null) WriteSkeletonRig(writer, entity.AnimationRig);

            writer.Flush();
            writer.Dispose();

            byte[] byteArray = memoryStream.ToArray();
            memoryStream.Dispose();

            return byteArray;
        }

        private static void WriteSkeletonRig(BinaryWriter writer, SkeletonAnimRigNode rigNode)
        {
            writer.Write(rigNode.Name);
            WriteMatrix4X4(writer, ref rigNode.LocalTransform);
            writer.Write(rigNode.Children.Length);
            for (var i = 0; i < rigNode.Children.Length; i++)
            {
                WriteSkeletonRig(writer, rigNode.Children[i]);
            }
        }

        private static SkeletonAnimRigNode ReadSkeletonRig(BinaryReader reader)
        {
            var rigNode = new SkeletonAnimRigNode
            {
                Name = reader.ReadString(),
                LocalTransform = ReadMatrix4X4(reader)
            };

            int childNodes = reader.ReadInt32();
            if (childNodes > 0)
            {
                rigNode.Children = new SkeletonAnimRigNode[childNodes];
                for (var i = 0; i < childNodes; i++)
                {
                    rigNode.Children[i] = ReadSkeletonRig(reader);
                }
            }

            return rigNode;
        }

        private static void WriteMatrix4X4(BinaryWriter writer, ref Matrix4x4 mat)
        {
            writer.Write(mat.M11);
            writer.Write(mat.M12);
            writer.Write(mat.M13);
            writer.Write(mat.M14);

            writer.Write(mat.M21);
            writer.Write(mat.M22);
            writer.Write(mat.M23);
            writer.Write(mat.M24);

            writer.Write(mat.M31);
            writer.Write(mat.M32);
            writer.Write(mat.M33);
            writer.Write(mat.M34);

            writer.Write(mat.M41);
            writer.Write(mat.M42);
            writer.Write(mat.M43);
            writer.Write(mat.M44);
        }

        private static Matrix4x4 ReadMatrix4X4(BinaryReader reader)
        {
            var mat = new Matrix4x4
            {
                M11 = reader.ReadSingle(),
                M12 = reader.ReadSingle(),
                M13 = reader.ReadSingle(),
                M14 = reader.ReadSingle(),

                M21 = reader.ReadSingle(),
                M22 = reader.ReadSingle(),
                M23 = reader.ReadSingle(),
                M24 = reader.ReadSingle(),

                M31 = reader.ReadSingle(),
                M32 = reader.ReadSingle(),
                M33 = reader.ReadSingle(),
                M34 = reader.ReadSingle(),

                M41 = reader.ReadSingle(),
                M42 = reader.ReadSingle(),
                M43 = reader.ReadSingle(),
                M44 = reader.ReadSingle()
            };

            return mat;
        }

        private static void WriteVector2(BinaryWriter writer, ref Vector2 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }

        private static Vector2 ReadVector2(BinaryReader reader)
        {
            var vector = new Vector2
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };
            return vector;
        }

        private static void WriteVector3(BinaryWriter writer, ref Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            var vector = new Vector3
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
            return vector;
        }

        private static void WriteVector4(BinaryWriter writer, ref Vector4 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }

        private static Vector4 ReadVector4(BinaryReader reader)
        {
            var vector = new Vector4
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                W = reader.ReadSingle()
            };
            return vector;
        }

        private static void WriteQuart(BinaryWriter writer, ref Quaternion quart)
        {
            writer.Write(quart.X);
            writer.Write(quart.Y);
            writer.Write(quart.Z);
            writer.Write(quart.W);
        }

        private static Quaternion ReadQuart(BinaryReader reader)
        {
            var quart = new Quaternion
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                W = reader.ReadSingle()
            };
            return quart;
        }
    }
}