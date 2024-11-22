#nullable enable

using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Emotion.Standard.GLTF;

public static partial class GLTFFormat
{
    public static MeshEntity? Decode(string rootFolder, ReadOnlyMemory<byte> fileData)
    {
        GLTFDocument? gltfDoc = JsonSerializer.Deserialize<GLTFDocument>(fileData.Span);
        if (gltfDoc == null) return null;

        bool makeLeftHanded = false;

        // Read all byte buffers
        const string base64DataPrefix = "data:application/gltf-buffer;base64,";
        GLTFBuffer[] buffers = gltfDoc.Buffers;
        for (int i = 0; i < buffers.Length; i++)
        {
            GLTFBuffer buffer = buffers[i];
            string uri = buffer.Uri;

            ReadOnlyMemory<byte> content;
            if (uri.StartsWith(base64DataPrefix))
            {
                // todo: Convert.FromBase64String that takes in a span to save on allocation here.
                uri = uri.Replace(base64DataPrefix, "");
                content = new ReadOnlyMemory<byte>(Convert.FromBase64String(uri));
            }
            else
            {
                // todo: ONE assetloader async
                uri = AssetLoader.GetNonRelativePath(rootFolder, uri);
                OtherAsset? byteAsset = Engine.AssetLoader.Get<OtherAsset>(uri, false);
                if (byteAsset == null) return null;
                content = byteAsset.Content;
            }

            buffer.Data = content;
        }

        // Read animation rig
        GLTFNode[] gltfRig = gltfDoc.Nodes;
        SkeletonAnimRigNode[] rigNodes = new SkeletonAnimRigNode[gltfRig.Length];
        SkeletonAnimRigRoot? rigRoot = null;
        for (int i = 0; i < gltfRig.Length; i++)
        {
            if (i == 0) continue;

            GLTFNode gltfRigItem = gltfRig[i];
            SkeletonAnimRigNode rigItem;
            if (rigRoot == null)
            {
                rigRoot = new SkeletonAnimRigRoot();
                rigItem = rigRoot;
            }
            else
            {
                rigItem = new SkeletonAnimRigNode();
            }

            rigItem.Name = gltfRigItem.Name;

            if (gltfRigItem.Translation != null ||
                gltfRigItem.Rotation != null ||
                gltfRigItem.Scale != null)
            {
                Matrix4x4 scaleMat = Matrix4x4.Identity;
                float[]? scale = gltfRigItem.Scale;
                if (scale != null)
                {
                    scaleMat = Matrix4x4.CreateScale(scale[0], scale[1], scale[2]);
                }

                Matrix4x4 rotMat = Matrix4x4.Identity;
                float[]? rot = gltfRigItem.Rotation;
                if (rot != null)
                {
                    Quaternion quart = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
                    rotMat = Matrix4x4.CreateFromQuaternion(quart);
                }

                Matrix4x4 transMat = Matrix4x4.Identity;
                float[]? trans = gltfRigItem.Translation;
                if (trans != null)
                {
                    transMat = Matrix4x4.CreateTranslation(trans[0], trans[1], trans[2]);
                }

                rigItem.LocalTransform = scaleMat * rotMat * transMat;
            }
            else
            {
                rigItem.LocalTransform = Matrix4x4.Identity;
            }

            rigNodes[i] = rigItem;
        }

        // Stitch animation rig tree
        if (rigRoot != null)
        {
            for (int i = 0; i < rigNodes.Length; i++)
            {
                if (i == 0) continue;

                SkeletonAnimRigNode node = rigNodes[i];
                GLTFNode gltfNode = gltfRig[i];

                if (gltfNode.Children != null)
                {
                    SkeletonAnimRigNode[] children = new SkeletonAnimRigNode[gltfNode.Children.Length];
                    for (int ii = 0; ii < gltfNode.Children.Length; ii++)
                    {
                        int childIdx = gltfNode.Children[ii];
                        children[ii] = rigNodes[childIdx];
                    }
                    node.Children = children;
                }
            }
        }

        // Read animations
        SkeletalAnimation[]? animations = null;
        if (gltfDoc.Animations != null)
        {
            const float ANIM_TIME_SCALE = 1000; // We except them to be in seconds, but emotion works in ms.

            animations = new SkeletalAnimation[gltfDoc.Animations.Length];
            for (int i = 0; i < gltfDoc.Animations.Length; i++)
            {
                GLTFAnimation gltfAnim = gltfDoc.Animations[i];
                GLTFAnimationChannel[] gltfChannels = gltfAnim.Channels;

                int nextChannelAlloc = 0;
                SkeletonAnimChannel[] channels = new SkeletonAnimChannel[gltfChannels.Length];
                float animDuration = 0;

                for (int c = 0; c < gltfChannels.Length; c++)
                {
                    GLTFAnimationChannel gltf = gltfChannels[c];
                    int samplerIdx = gltf.Sampler;
                    GLTFAnimationSampler sampler = gltfAnim.Samplers[samplerIdx];

                    bool dontInterpolate = false;
                    string? interpolationMode = sampler.Interpolation;
                    switch (interpolationMode)
                    {
                        case "LINEAR": // Lerp
                            break;
                        case "STEP": // Constant value until next keyframe
                            dontInterpolate = true;
                            break;
                        default:
                            Assert(false, $"Unknown sampler interpolation type {interpolationMode} in animation {gltfAnim.Name}");
                            break;
                    }

                    int timestampAccessorId = sampler.Input;
                    GLTFAccessor timestampAccessor = gltfDoc.Accessors[timestampAccessorId];
                    AccessorReader<float> timestampData = GetAccessorDataAsType<float>(gltfDoc, timestampAccessor);
                    for (int t = 0; t < timestampData.Count; t++)
                    {
                        float time = timestampData.ReadElement(t) * ANIM_TIME_SCALE;
                        animDuration = MathF.Max(animDuration, time);
                    }

                    //ReadOnlySpan<ushort> indicesAsUshort = MemoryMarshal.Cast<byte, ushort>(indicesData.Span);

                    int dataId = sampler.Output;
                    GLTFAccessor dataAccessor = gltfDoc.Accessors[dataId];

                    GLTFAnimationChannel.GLTFAnimationChannelTarget target = gltf.Target;
                    int nodeIdx = target.Node;
                    SkeletonAnimRigNode node = rigNodes[nodeIdx];

                    SkeletonAnimChannel? channel = null;
                    foreach (SkeletonAnimChannel existingChannel in channels)
                    {
                        if (existingChannel != null && existingChannel.Name == node.Name)
                        {
                            channel = existingChannel;
                            break;
                        }
                    }
                    if (channel == null)
                    {
                        channel = new SkeletonAnimChannel();
                        channel.Name = node.Name; // todo
                        channels[nextChannelAlloc] = channel;
                        nextChannelAlloc++;
                    }

                    string path = target.Path;
                    switch (path)
                    {
                        case "rotation":
                            {
                                AccessorReader<Vector4> samplerData = GetAccessorDataAsType<Vector4>(gltfDoc, dataAccessor);
                                MeshAnimBoneRotation[] rotations = new MeshAnimBoneRotation[samplerData.Count];
                                for (int s = 0; s < samplerData.Count; s++)
                                {
                                    Vector4 data = samplerData.ReadElement(s);
                                    //if (makeLeftHanded) data.Z = -data.Z;

                                    rotations[s] = new MeshAnimBoneRotation()
                                    {
                                        Rotation = new Quaternion(data.X, data.Y, data.Z, data.W),
                                        Timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE,
                                        DontInterpolate = dontInterpolate
                                    };
                                }
                                channel.Rotations = rotations;
                                break;
                            }
                        case "translation":
                            {
                                AccessorReader<Vector3> samplerData = GetAccessorDataAsType<Vector3>(gltfDoc, dataAccessor);
                                MeshAnimBoneTranslation[] translations = new MeshAnimBoneTranslation[samplerData.Count];
                                for (int s = 0; s < samplerData.Count; s++)
                                {
                                    Vector3 data = samplerData.ReadElement(s);
                                    //if (makeLeftHanded) data.Z = -data.Z;

                                    translations[s] = new MeshAnimBoneTranslation()
                                    {
                                        Position = data,
                                        Timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE,
                                        DontInterpolate = dontInterpolate
                                    };
                                }
                                channel.Positions = translations;
                                break;
                            }
                        case "scale":
                            {
                                AccessorReader<Vector3> samplerData = GetAccessorDataAsType<Vector3>(gltfDoc, dataAccessor);
                                MeshAnimBoneScale[] scales = new MeshAnimBoneScale[samplerData.Count];
                                for (int s = 0; s < samplerData.Count; s++)
                                {
                                    Vector3 data = samplerData.ReadElement(s);
                                    //if (makeLeftHanded) data.Z = -data.Z;

                                    scales[s] = new MeshAnimBoneScale()
                                    {
                                        Scale = data,
                                        Timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE,
                                        DontInterpolate = dontInterpolate
                                    };
                                }
                                channel.Scales = scales;
                                break;
                            }
                        default:
                            Assert(false, $"Unknown channel path {path} in animation {gltfAnim.Name}");
                            break;
                    }
                }

                Array.Resize(ref channels, nextChannelAlloc);
                SkeletalAnimation anim = new SkeletalAnimation()
                {
                    Name = gltfAnim.Name,
                    AnimChannels = channels,
                    Duration = animDuration
                };
                animations[i] = anim;
            }
        }

        // Read images
        GLTFImage[] images = gltfDoc.Images;
        Texture[] imagesRead = new Texture[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            GLTFImage image = images[i];
            string uri = image.Uri;

            uri = AssetLoader.GetNonRelativePath(rootFolder, uri);
            TextureAsset? textureAsset = Engine.AssetLoader.Get<TextureAsset>(uri, false);
            if (textureAsset != null) textureAsset.Texture.Smooth = true; // todo

            imagesRead[i] = textureAsset == null ? Texture.EmptyWhiteTexture : textureAsset.Texture;
        }

        // Read materials
        GLTFMaterial[] gltfMaterials = gltfDoc.Materials;
        MeshMaterial[] materials = new MeshMaterial[gltfMaterials.Length];
        for (int i = 0; i < gltfMaterials.Length; i++)
        {
            GLTFMaterial gltfMaterial = gltfMaterials[i];
            GLTFMaterial.GLTFPBRMetallicRoughness pbr = gltfMaterial.PBRMetallicRoughness;
            AssertNotNull(pbr);

            GLTFMaterial.GLTFBaseColorTexture baseColorTexture = pbr.BaseColorTexture;
            AssertNotNull(baseColorTexture);

            int textureIndex = baseColorTexture.Index;
            GLTFTexture texture = gltfDoc.Textures[textureIndex];
            GLTFImage image = gltfDoc.Images[texture.Source];
            string assetPath = AssetLoader.JoinPath(rootFolder, image.Uri);

            Texture imageRead = imagesRead[texture.Source];

            MeshMaterial material = new MeshMaterial()
            {
                Name = gltfMaterial.Name,
                DiffuseColor = Color.White,

                DiffuseTexture = imageRead,
                DiffuseTextureName = assetPath,
            };
            materials[i] = material;
        }

        // Read meshes
        GLTFMesh[] gltfMeshes = gltfDoc.Meshes;
        Mesh[] meshes = new Mesh[gltfMeshes.Length];
        for (int m = 0; m < gltfMeshes.Length; m++)
        {
            GLTFMesh gltfMesh = gltfMeshes[m];
            GLTFMeshPrimitives[] primitives = gltfMesh.Primitives;
            GLTFMeshPrimitives primitive = primitives[0]; // ??? What does it mean to have multiple
            Dictionary<string, int> attributes = primitive.Attributes;

            // Read indices
            GLTFAccessor indexAccessor = gltfDoc.Accessors[primitive.Indices];
            Assert(indexAccessor.ComponentType == Gl.UNSIGNED_SHORT);
            Assert(indexAccessor.Type == "SCALAR");

            ReadOnlyMemory<byte> indicesData = GetAccessorData(gltfDoc, indexAccessor);
            ReadOnlySpan<ushort> indicesAsUshort = MemoryMarshal.Cast<byte, ushort>(indicesData.Span);
            ushort[] indices = new ushort[indexAccessor.Count];
            indicesAsUshort.CopyTo(indices);

            // Determine vertex count from largest attribute
            bool isSkinned = false;
            int vertexCount = 0;
            foreach (KeyValuePair<string, int> attribute in attributes)
            {
                GLTFAccessor accessor = gltfDoc.Accessors[attribute.Value];
                vertexCount = Math.Max(vertexCount, accessor.Count);

                if (attribute.Key == "JOINTS_0" || attribute.Key == "WEIGHTS_0")
                    isSkinned = true;
            }

            // Initialize vertices array
            VertexData[] vertices = new VertexData[vertexCount];
            VertexDataMesh3DExtra[] verticesExtraData = new VertexDataMesh3DExtra[vertexCount];
            for (int i = 0; i < vertices.Length; i++)
            {
                ref VertexData vert = ref vertices[i];
                vert.Color = Color.WhiteUint;
            }

            Mesh3DVertexDataBones[]? boneData = null;
            if (isSkinned)
            {
                boneData = new Mesh3DVertexDataBones[vertexCount];

                foreach (KeyValuePair<string, int> attribute in attributes)
                {
                    GLTFAccessor accessor = gltfDoc.Accessors[attribute.Value];
                    string attributeKey = attribute.Key;
                    switch (attributeKey)
                    {
                        case "POSITION":
                        case "NORMAL":
                            {
                                AccessorReader<Vector3> accessorData = GetAccessorDataAsType<Vector3>(gltfDoc, accessor);

                                for (int i = 0; i < accessorData.Count; i++)
                                {
                                    Vector3 pos = accessorData.ReadElement(i);
                                    if (makeLeftHanded) pos.Z = -pos.Z;

                                    if (attributeKey == "POSITION")
                                    {
                                        ref VertexData vert = ref vertices[i];
                                        vert.Vertex = pos;
                                    }
                                    else if (attributeKey == "NORMAL")
                                    {
                                        ref VertexDataMesh3DExtra vert = ref verticesExtraData[i];
                                        vert.Normal = pos;
                                    }
                                }
                                break;
                            }
                        case "TEXCOORD_0":
                        case "TEXCOORD_1": // todo
                            {
                                AccessorReader<Vector2> accessorData = GetAccessorDataAsType<Vector2>(gltfDoc, accessor);

                                for (int i = 0; i < accessorData.Count; i++)
                                {
                                    Vector2 pos = accessorData.ReadElement(i);

                                    if (attributeKey == "TEXCOORD_0")
                                    {
                                        ref VertexData vert = ref vertices[i];
                                        vert.UV = pos;
                                    }
                                }
                                break;
                            }
                        case "JOINTS_0":
                            {
                                AccessorReader<Vector4> accessorData = GetAccessorDataAsType<Vector4>(gltfDoc, accessor);

                                for (int i = 0; i < accessorData.Count; i++)
                                {
                                    Vector4 joints = accessorData.ReadElement(i);

                                    ref Mesh3DVertexDataBones vert = ref boneData[i];
                                    vert.BoneIds = joints;

                                }
                                break;
                            }
                        case "WEIGHTS_0":
                            {
                                AccessorReader<Vector4> accessorData = GetAccessorDataAsType<Vector4>(gltfDoc, accessor);

                                for (int i = 0; i < accessorData.Count; i++)
                                {
                                    Vector4 weights = accessorData.ReadElement(i);

                                    ref Mesh3DVertexDataBones vert = ref boneData[i];
                                    vert.BoneWeights = weights;

                                }
                                break;
                            }
                    }
                }
            }

            GLTFSkins[] skins = gltfDoc.Skins;
            GLTFSkins? primarySkin = skins.Length > 0 ? skins[0] : null; // Todo
            MeshBone[]? bones = null;
            if (primarySkin != null)
            {
                int bindMatricesAccessorId = primarySkin.InverseBindMatrices;
                GLTFAccessor bindMatrices = gltfDoc.Accessors[bindMatricesAccessorId];
                AccessorReader<Matrix4x4> bindMatrixData = GetAccessorDataAsType<Matrix4x4>(gltfDoc, bindMatrices);

                bones = new MeshBone[primarySkin.Joints.Length];
                for (int i = 0; i < primarySkin.Joints.Length; i++)
                {
                    int jointId = primarySkin.Joints[i];
                    SkeletonAnimRigNode joint = rigNodes[jointId];

                    Matrix4x4 offsetMatrix = bindMatrixData.ReadElement(i);
                    bones[i] = new MeshBone()
                    {
                        BoneIndex = i,
                        Name = joint.Name,
                        OffsetMatrix = offsetMatrix
                    };
                }
            }

            int materialIndex = primitive.Material;
            MeshMaterial material = materials[materialIndex];

            Mesh mesh = new Mesh($"Mesh {m}", vertices, verticesExtraData, indices);
            mesh.Material = material;
            mesh.Bones = bones;
            if (isSkinned) mesh.BoneData = boneData;
            meshes[m] = mesh;
        }

        MeshEntity entity = new MeshEntity()
        {
            Name = "Unknown Entity Name",
            LocalTransform = Matrix4x4.CreateRotationX(90 * Maths.DEG2_RAD), // Y up to Z up
            Meshes = meshes,
            AnimationRig = rigRoot,
            Animations = animations,
        };

        return entity;
    }

    private static ReadOnlyMemory<byte> GetAccessorData(GLTFDocument gltfDoc, GLTFAccessor accessor)
    {
        int bufferViewIdx = accessor.BufferView;
        GLTFBufferView bufferView = gltfDoc.BufferViews[bufferViewIdx];
        int bufferIdx = bufferView.Buffer;
        GLTFBuffer buffer = gltfDoc.Buffers[bufferIdx];
        ReadOnlyMemory<byte> data = buffer.Data;

        return data.Slice(bufferView.ByteOffset, bufferView.ByteLength).Slice(accessor.ByteOffset);
    }

    private static AccessorReader<T> GetAccessorDataAsType<T>(GLTFDocument gltfDoc, GLTFAccessor accessor) where T : unmanaged
    {
        string accessorType = accessor.Type;
        string requestedType = TypeToGlType<T>();

        // Type mismatch
        if (accessorType != requestedType) return AccessorReader<T>.Empty;

        int bufferViewIdx = accessor.BufferView;
        GLTFBufferView bufferView = gltfDoc.BufferViews[bufferViewIdx];

        int bufferIdx = bufferView.Buffer;
        GLTFBuffer buffer = gltfDoc.Buffers[bufferIdx];

        ReadOnlyMemory<byte> data = buffer.Data;
        ReadOnlyMemory<byte> dataInView = data.Slice(bufferView.ByteOffset, bufferView.ByteLength).Slice(accessor.ByteOffset);

        int componentType = accessor.ComponentType;
        AccessorReader<T> reader = new AccessorReader<T>
        {
            Data = dataInView,
            ReaderFunc = GetComponentReader(componentType),
            ComponentCount = GetComponentCount(accessorType),
            ComponentSize = GetComponentSize(componentType),
            Stride = Math.Max(bufferView.ByteStride, GetComponentCount(accessorType) * GetComponentSize(componentType)),
            Count = accessor.Count,
            Normalized = accessor.Normalized,
        };

        return reader;
    }

    public static int GetComponentSize(int componentType)
    {
        switch (componentType)
        {
            case Gl.FLOAT:
                return 4;
            case Gl.UNSIGNED_BYTE:
                return 1;
            case Gl.UNSIGNED_SHORT:
                return 2;
        }

        return 0;
    }

    public static int GetComponentCount(string type)
    {
        switch (type)
        {
            case "VEC4":
                return 4;
            case "VEC3":
                return 3;
            case "VEC2":
                return 2;
            case "SCALAR":
                return 1;
            case "MAT4":
                return 4 * 4;
        }

        return 0;
    }

    public static string TypeToGlType<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(Vector4))
            return "VEC4";
        else if (typeof(T) == typeof(Vector3))
            return "VEC3";
        else if (typeof(T) == typeof(Vector2))
            return "VEC2";
        else if (typeof(T) == typeof(float))
            return "SCALAR";
        else if (typeof(T) == typeof(Matrix4x4))
            return "MAT4";

        return string.Empty;
    }

    public static Func<ReadOnlyMemory<byte>, bool, float> GetComponentReader(int componentType)
    {
        switch (componentType)
        {
            case Gl.FLOAT:
                return GLTFAccessorReadComponent<float>;
            case Gl.UNSIGNED_SHORT:
                return GLTFAccessorReadComponent<ushort>;
            case Gl.UNSIGNED_BYTE:
                return GLTFAccessorReadComponent<byte>;
        }

        return null!;
    }

    private static unsafe float GLTFAccessorReadComponent<TActualType>(ReadOnlyMemory<byte> data, bool normalize)
        where TActualType : unmanaged, INumber<TActualType>, IMinMaxValue<TActualType>
    {
        var span = data.Span;
        var spanActualType = MemoryMarshal.Cast<byte, TActualType>(span);
        TActualType actualTypeValue = spanActualType[0];

        float valAsFloat = (float)Convert.ChangeType(actualTypeValue, typeof(float));

        // Cast the number to a float and divide it by its max value to normalize it.
        if (normalize && typeof(TActualType) != typeof(float))
        {
            float valMax = (float)Convert.ChangeType(TActualType.MaxValue, typeof(float));
            valAsFloat = valAsFloat / valMax;
        }

        return valAsFloat;
    }
}
