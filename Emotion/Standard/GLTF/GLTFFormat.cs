#nullable enable

using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Serialization.JSON;
using Emotion.Utility;
using OpenGL;
using System.Runtime.InteropServices;

namespace Emotion.Standard.GLTF;

public static partial class GLTFFormat
{
    private const string BASE64_DATA_PREFIX = "data:application/gltf-buffer;base64,";
    private const string BASE64_DATA_PREFIX_2 = "data:application/octet-stream;base64,";
    private const float ANIM_TIME_SCALE = 1000; // We expect them to be in seconds, but emotion works in ms.

    public static GLTFDocument? Decode(ReadOnlyMemory<byte> fileData)
    {
        return JSONSerialization.From<GLTFDocument>(fileData.Span);
    }

    public static IEnumerable<string> ForEachBufferDependency(GLTFDocument gltfDoc, string rootFolder)
    {
        GLTFBuffer[] buffers = gltfDoc.Buffers;
        for (int i = 0; i < buffers.Length; i++)
        {
            GLTFBuffer buffer = buffers[i];
            string? uri = buffer.Uri;
            if (string.IsNullOrEmpty(uri)) continue;

            if (!uri.StartsWith(BASE64_DATA_PREFIX) && !uri.StartsWith(BASE64_DATA_PREFIX_2))
                yield return AssetLoader.GetNonRelativePath(rootFolder, uri);
        }
    }

    public static IEnumerable<string> ForEachImageDependency(GLTFDocument gltfDoc, string rootFolder)
    {
        GLTFImage[] images = gltfDoc.Images ?? Array.Empty<GLTFImage>();
        for (int i = 0; i < images.Length; i++)
        {
            GLTFImage image = images[i];
            string uri = image.Uri;
            if (string.IsNullOrEmpty(uri)) continue;

            yield return AssetLoader.GetNonRelativePath(rootFolder, uri);
        }
    }

    public static MeshEntity CreateEntityFromDocument(GLTFDocument gltfDoc, string rootFolder)
    {
        bool makeLeftHanded = true; // GLTF assets are by default right-handed, but Emotion is left-handed
        bool invertUVVertical = false; // OpenGL goes [0:1] up while DirectX goes [0:1]
        if (gltfDoc.Asset != null && gltfDoc.Asset.LeftHanded)
            makeLeftHanded = false;
        if (gltfDoc.Asset != null && gltfDoc.Asset.InvertUVVertical)
            invertUVVertical = true;

        // Read all byte buffers
        GLTFBuffer[] buffers = gltfDoc.Buffers;
        for (int i = 0; i < buffers.Length; i++)
        {
            GLTFBuffer buffer = buffers[i];
            string uri = buffer.Uri;

            ReadOnlyMemory<byte> content = ReadOnlyMemory<byte>.Empty;
            if (uri.StartsWith(BASE64_DATA_PREFIX))
            {
                // todo: Convert.FromBase64String that takes in a span to save an allocation here.
                uri = uri.Replace(BASE64_DATA_PREFIX, "");
                content = new ReadOnlyMemory<byte>(Convert.FromBase64String(uri));
            }
            else if (uri.StartsWith(BASE64_DATA_PREFIX_2))
            {
                uri = uri.Replace(BASE64_DATA_PREFIX_2, "");
                content = new ReadOnlyMemory<byte>(Convert.FromBase64String(uri));
            }
            else
            {
                uri = AssetLoader.GetNonRelativePath(rootFolder, uri);
                OtherAsset byteAsset = Engine.AssetLoader.ONE_Get<OtherAsset>(uri);
                Assert(byteAsset.Processed); // We expect it to be already requested via dependencies
                if (byteAsset.Loaded)
                    content = byteAsset.Content;
            }

            buffer.Data = content;
        }

        // Read animation rig
        GLTFNode[] gltfRig = gltfDoc.Nodes;
        SkeletonAnimRigNode[] rigNodes = new SkeletonAnimRigNode[gltfRig.Length];
        for (int i = 0; i < rigNodes.Length; i++)
        {
            rigNodes[i] = new SkeletonAnimRigNode();
        }

        for (int i = 0; i < gltfRig.Length; i++)
        {
            GLTFNode gltfRigItem = gltfRig[i];
            SkeletonAnimRigNode thisNode = rigNodes[i];
            thisNode.Name = gltfRigItem.Name;

            bool hasLocalTransform = gltfRigItem.Translation != null || gltfRigItem.Rotation != null || gltfRigItem.Scale != null;
            if (hasLocalTransform)
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
                    if (makeLeftHanded) quart.Z = -quart.Z;
                    rotMat = Matrix4x4.CreateFromQuaternion(quart);
                }

                Matrix4x4 transMat = Matrix4x4.Identity;
                float[]? trans = gltfRigItem.Translation;
                if (trans != null)
                {
                    Vector3 pos = new Vector3(trans[0], trans[1], trans[2]);
                    if (makeLeftHanded) pos.Z = -pos.Z;
                    transMat = Matrix4x4.CreateTranslation(pos);
                }

                thisNode.LocalTransform = scaleMat * rotMat * transMat;
            }

            // Attach as parent to my children.
            int[] children = gltfRigItem.Children;
            for (int cIdx = 0; cIdx < children.Length; cIdx++)
            {
                int childIdx = children[cIdx];
                rigNodes[childIdx].ParentIdx = i;
            }
        }

        // Read animations
        GLTFAnimation[] gltfAnimation = gltfDoc.Animations;
        SkeletalAnimation[] animations = gltfAnimation.Length > 0 ? new SkeletalAnimation[gltfAnimation.Length] : Array.Empty<SkeletalAnimation>();
        for (int i = 0; i < gltfAnimation.Length; i++)
        {
            GLTFAnimation gltfAnim = gltfAnimation[i];
            GLTFAnimationChannel[] gltfChannels = gltfAnim.Channels;

            // We allocate a channel for every node in the rig.
            // Most of these will be null though. This might be a bit wasteful for giant
            // rigs with animations that don't move many bones, but at the moment we're only concerned
            // with look up speed.
            SkeletonAnimChannel[] channels = new SkeletonAnimChannel[rigNodes.Length];

            float animDuration = 0;
            for (int c = 0; c < gltfChannels.Length; c++)
            {
                GLTFAnimationChannel gltfChannel = gltfChannels[c];

                GLTFAnimationChannelTarget? target = gltfChannel.Target;
                if (target == null) continue;

                int samplerIdx = gltfChannel.Sampler;
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
                    float timestamp = timestampData.ReadElement(t) * ANIM_TIME_SCALE;
                    if (timestamp > 100_000) timestamp /= ANIM_TIME_SCALE; // Disgusting hack for some broken models I used :P

                    animDuration = MathF.Max(animDuration, timestamp);
                }

                int dataId = sampler.Output;
                GLTFAccessor dataAccessor = gltfDoc.Accessors[dataId];

                int nodeIdx = target.Node;
                SkeletonAnimRigNode node = rigNodes[nodeIdx];

                // Check if emotion structure is initialized
                SkeletonAnimChannel? channel = channels[nodeIdx];
                if (channel == null)
                {
                    channel = new SkeletonAnimChannel();
                    channel.Name = node.Name ?? string.Empty; // todo
                    channels[nodeIdx] = channel;
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
                                if (makeLeftHanded)
                                {
                                    data.X = -data.X;
                                    data.Y = -data.Y;
                                }

                                float timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE;
                                if (timestamp > 100_000) timestamp /= ANIM_TIME_SCALE;

                                rotations[s] = new MeshAnimBoneRotation()
                                {
                                    Rotation = new Quaternion(data.X, data.Y, data.Z, data.W),
                                    Timestamp = timestamp,
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
                                if (makeLeftHanded) data.Z = -data.Z;

                                float timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE;
                                if (timestamp > 100_000) timestamp /= ANIM_TIME_SCALE;

                                translations[s] = new MeshAnimBoneTranslation()
                                {
                                    Position = data,
                                    Timestamp = timestamp,
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

                                float timestamp = timestampData.ReadElement(s) * ANIM_TIME_SCALE;
                                if (timestamp > 100_000) timestamp /= ANIM_TIME_SCALE;

                                scales[s] = new MeshAnimBoneScale()
                                {
                                    Scale = data,
                                    Timestamp = timestamp,
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

            SkeletalAnimation anim = new SkeletalAnimation()
            {
                Name = gltfAnim.Name,
                AnimChannels = channels,
                Duration = animDuration
            };
            animations[i] = anim;
        }

        // Read images
        GLTFImage[] images = gltfDoc.Images;
        GLTFTextureSampler[] samplers = gltfDoc.Samplers;
        Texture[] imagesRead = images.Length > 0 ? new Texture[images.Length] : Array.Empty<Texture>();
        for (int i = 0; i < images.Length; i++)
        {
            GLTFImage image = images[i];
            string uri = image.Uri;
            if (string.IsNullOrEmpty(uri)) continue;

            uri = AssetLoader.GetNonRelativePath(rootFolder, uri);
            TextureAsset textureAsset = Engine.AssetLoader.ONE_Get<TextureAsset>(uri);
            Assert(textureAsset.Processed); // We expect it to be already requested via dependencies
            if (textureAsset.Loaded)
            {
                // hmm?
                textureAsset.Texture.Smooth = true;
                textureAsset.Texture.Tile = true;
                textureAsset.Texture.Mipmap = true;
            }

            imagesRead[i] = textureAsset.Loaded ? textureAsset.Texture : Texture.EmptyWhiteTexture;
        }

        // Read materials
        GLTFMaterial[] gltfMaterials = gltfDoc.Materials;
        MeshMaterial[] materials = new MeshMaterial[gltfMaterials.Length];
        for (int i = 0; i < gltfMaterials.Length; i++)
        {
            GLTFMaterial gltfMaterial = gltfMaterials[i];

            Color diffuseColor = Color.White;
            GLTFTexture? gltfDiffuseTexture = null;
            if (gltfMaterial.PbrMetallicRoughness != null)
            {
                GLTFMaterialPBR pbr = gltfMaterial.PbrMetallicRoughness;
                GLTFBaseColorTexture? baseColorTexture = pbr.BaseColorTexture;
                if (baseColorTexture != null)
                {
                    int textureIndex = baseColorTexture.Index;
                    gltfDiffuseTexture = gltfDoc.Textures[textureIndex];
                }
            }
            else if (gltfMaterial.Values != null)
            {
                var pbr = gltfMaterial.Values;
                if (pbr.Diffuse.IsArray) // is color
                    diffuseColor = pbr.DiffuseColor;
                else // is reference to texture
                    gltfDiffuseTexture = pbr.Diffuse.ReferenceAsNameOrArray.GetReferenced(gltfDoc.Textures);
            }

            Texture? diffuseTexture = null;
            GLTFImage? image = gltfDiffuseTexture?.Source.GetReferenced(images);
            if (image != null)
            {
                int imageIndex = images.IndexOf(image);
                diffuseTexture = imageIndex == -1 ? null : imagesRead[imageIndex];
            }

            GLTFTextureSampler? sampler = gltfDiffuseTexture?.Sampler.GetReferenced(samplers);
            if (sampler != null && diffuseTexture != null)
            {
                if (sampler.WrapT == TextureWrapMode.Repeat && sampler.WrapS == TextureWrapMode.Repeat)
                    diffuseTexture.Tile = true;

                if (sampler.MagFilter == TextureMagFilter.Linear && sampler.MinFilter == TextureMagFilter.Linear)
                    diffuseTexture.Smooth = true;
            }

            MeshMaterial material = new MeshMaterial()
            {
                Name = gltfMaterial.Name,
                DiffuseColor = diffuseColor,

                DiffuseTexture = diffuseTexture ?? Texture.EmptyWhiteTexture,
                DiffuseTextureName = image == null ? null : AssetLoader.JoinPath(rootFolder, image.Uri),
            };
            materials[i] = material;
        }

        // Read meshes
        GLTFMesh[] gltfMeshes = gltfDoc.Meshes;

        // Dirty hack/optimization
        // Sometimes the data is stored in a single buffer and accessed by each mesh.
        // This is actually better than what we do (which is to have separate buffers for each mesh)
        // and allows for all the meshes to be batched in a single draw call.
        // The reason they are still separate meshes is probably to be able to turn off/on each one
        // individually. Currently Emotion doesn't support having this one buffer (todo) so we should
        // trim the single buffer into many smaller ones to avoid each mesh carrying all the data.
        bool mappingIntoSingleBuffer = true;
        if (gltfMeshes.Length > 1)
        {
            GLTFMesh firstMesh = gltfMeshes[0];
            GLTFMeshPrimitives[] primitives = firstMesh.Primitives;
            GLTFMeshPrimitives primitive = primitives[0];
            Dictionary<string, JSONArrayIndexOrName>? attributes = primitive.Attributes;
            if (attributes != null)
            {
                for (int m = 0; m < gltfMeshes.Length; m++)
                {
                    GLTFMesh otherMesh = gltfMeshes[m];
                    GLTFMeshPrimitives[] otherPrimitives = otherMesh.Primitives;
                    GLTFMeshPrimitives otherPrimitive = otherPrimitives[0];
                    Dictionary<string, JSONArrayIndexOrName>? otherAttributes = otherPrimitive.Attributes;
                    if (otherAttributes == null) continue;

                    foreach (KeyValuePair<string, JSONArrayIndexOrName> attribute in otherAttributes)
                    {
                        JSONArrayIndexOrName accessor = attribute.Value;
                        bool success = attributes.TryGetValue(attribute.Key, out JSONArrayIndexOrName firstMeshAccessor);
                        if (!success || accessor != firstMeshAccessor)
                        {
                            mappingIntoSingleBuffer = false;
                            break;
                        }
                    }

                    if (!mappingIntoSingleBuffer) break;
                }
            }
        }
        else
        {
            mappingIntoSingleBuffer = false;
        }

        int meshCount = 0;
        int currentMeshId = 0;
        for (int i = 0; i < gltfMeshes.Length; i++)
        {
            GLTFMesh gltfMesh = gltfMeshes[i];
            GLTFMeshPrimitives[] primitives = gltfMesh.Primitives;
            meshCount += primitives.Length;
        }

        Mesh[] meshes = new Mesh[meshCount];
        for (int m = 0; m < gltfMeshes.Length; m++)
        {
            GLTFMesh gltfMesh = gltfMeshes[m];
            GLTFMeshPrimitives[] primitives = gltfMesh.Primitives;

            for (int p = 0; p < primitives.Length; p++) // sucks
            {
                GLTFMeshPrimitives primitive = primitives[p];
                Dictionary<string, JSONArrayIndexOrName>? attributes = primitive.Attributes;
                if (attributes == null) continue;

                // Read indices
                GLTFAccessor? indexAccessor = primitive.Indices.GetReferenced(gltfDoc.Accessors);
                if (indexAccessor == null) continue;

                Assert(indexAccessor.Type == "SCALAR");
                ushort[] indices = new ushort[indexAccessor.Count];

                if (indexAccessor.ComponentType == Gl.UNSIGNED_BYTE)
                {
                    ReadOnlyMemory<byte> indicesData = GetAccessorData(gltfDoc, indexAccessor);
                    ReadOnlySpan<byte> indicesSpan = indicesData.Span;
                    for (int idx = 0; idx < indicesSpan.Length; idx++)
                        indices[idx] = indicesSpan[idx];
                }
                else
                {
                    Assert(indexAccessor.ComponentType == Gl.UNSIGNED_SHORT);
                    ReadOnlyMemory<byte> indicesData = GetAccessorData(gltfDoc, indexAccessor);
                    ReadOnlySpan<ushort> indicesAsUshort = MemoryMarshal.Cast<byte, ushort>(indicesData.Span);
                    indicesAsUshort = indicesAsUshort.Slice(0, indexAccessor.Count);
                    indicesAsUshort.CopyTo(indices);
                }

                // Determine vertex count from largest attribute, and whether we're expecting bones.
                bool isSkinned = false;
                int vertexCount = 0;
                foreach (KeyValuePair<string, JSONArrayIndexOrName> attribute in attributes)
                {
                    GLTFAccessor? accessor = attribute.Value.GetReferenced(gltfDoc.Accessors);
                    if (accessor == null) continue;

                    vertexCount = Math.Max(vertexCount, accessor.Count);

                    if (attribute.Key == "JOINTS_0" || attribute.Key == "WEIGHTS_0")
                        isSkinned = true;
                }

                // Split buffer if mapping into single buffer
                int vertexOffset = 0;
                if (mappingIntoSingleBuffer)
                {
                    int smallestVertexUsed = vertexCount;
                    int largestVertexUsed = 0;
                    for (int i = 0; i < indices.Length; i++)
                    {
                        ushort vIdx = indices[i];
                        if (vIdx > largestVertexUsed) largestVertexUsed = vIdx;
                        if (vIdx < smallestVertexUsed) smallestVertexUsed = vIdx;
                    }

                    vertexOffset = smallestVertexUsed;
                    vertexCount = (largestVertexUsed - smallestVertexUsed) + 1;

                    for (int i = 0; i < indices.Length; i++)
                    {
                        indices[i] -= (ushort)vertexOffset;
                    }
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
                    boneData = new Mesh3DVertexDataBones[vertexCount];

                foreach (KeyValuePair<string, JSONArrayIndexOrName> attribute in attributes)
                {
                    GLTFAccessor? accessor = attribute.Value.GetReferenced(gltfDoc.Accessors);
                    if (accessor == null) continue;

                    string attributeKey = attribute.Key;
                    switch (attributeKey)
                    {
                        case "POSITION":
                        case "NORMAL":
                            {
                                AccessorReader<Vector3> accessorData = GetAccessorDataAsType<Vector3>(gltfDoc, accessor);

                                for (int i = 0; i < vertexCount; i++)
                                {
                                    Vector3 pos = accessorData.ReadElement(vertexOffset + i);
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

                                for (int i = 0; i < vertexCount; i++)
                                {
                                    Vector2 pos = accessorData.ReadElement(vertexOffset + i);

                                    if (invertUVVertical)
                                        pos.Y = -pos.Y;

                                    if (attributeKey == "TEXCOORD_0")
                                    {
                                        ref VertexData vert = ref vertices[i];
                                        vert.UV = pos;
                                    }
                                }
                                break;
                            }
                        case "JOINTS_0" when isSkinned:
                            {
                                AssertNotNull(boneData);

                                AccessorReader<Vector4> accessorData = GetAccessorDataAsType<Vector4>(gltfDoc, accessor);

                                for (int i = 0; i < vertexCount; i++)
                                {
                                    Vector4 joints = accessorData.ReadElement(vertexOffset + i);

                                    ref Mesh3DVertexDataBones vert = ref boneData[i];
                                    vert.BoneIds = joints;
                                }
                                break;
                            }
                        case "WEIGHTS_0" when isSkinned:
                            {
                                AssertNotNull(boneData);

                                AccessorReader<Vector4> accessorData = GetAccessorDataAsType<Vector4>(gltfDoc, accessor);

                                for (int i = 0; i < vertexCount; i++)
                                {
                                    Vector4 weights = accessorData.ReadElement(vertexOffset + i);

                                    ref Mesh3DVertexDataBones vert = ref boneData[i];
                                    vert.BoneWeights = weights;
                                }
                                break;
                            }
                    }
                }

                MeshMaterial material = MeshMaterial.DefaultMaterial;
                if (primitive.Material.Valid)
                {
                    GLTFMaterial? gltfMaterial = primitive.Material.GetReferenced(gltfMaterials);
                    int materialIndex = gltfMaterials.IndexOf(gltfMaterial);
                    MeshMaterial? materialSet = materialIndex == -1 ? null : materials[materialIndex];
                    if (materialSet != null)
                        material = materialSet;
                }

                string meshName = gltfMesh.Name;
                if (gltfMesh.Name == string.Empty) meshName = $"Mesh {m}";
                if (p > 0) meshName = $" - Primitive {p}";

                Mesh mesh = new Mesh(meshName, vertices, verticesExtraData, indices)
                {
                    Material = material,
                    BoneData = boneData
                };
                meshes[currentMeshId] = mesh;
                currentMeshId++;
            }
        }

        GLTFSkins[] gltfSkins = gltfDoc.Skins ?? Array.Empty<GLTFSkins>();
        SkeletalAnimationSkin[] skins = new SkeletalAnimationSkin[gltfSkins.Length];
        for (int skinIdx = 0; skinIdx < gltfSkins.Length; skinIdx++)
        {
            GLTFSkins gltfSkin = gltfSkins[skinIdx];
            SkeletalAnimationSkin skin = new SkeletalAnimationSkin();
            skin.Name = gltfSkin.Name;

            int bindMatricesAccessorId = gltfSkin.InverseBindMatrices;
            GLTFAccessor bindMatrices = gltfDoc.Accessors[bindMatricesAccessorId];
            AccessorReader<Matrix4x4> bindMatrixData = GetAccessorDataAsType<Matrix4x4>(gltfDoc, bindMatrices);

            int[] gltfJoints = gltfSkin.Joints;
            var joints = new SkeletalAnimationSkinJoint[gltfJoints.Length];
            for (int jIdx = 0; jIdx < gltfJoints.Length; jIdx++)
            {
                ref SkeletalAnimationSkinJoint joint = ref joints[jIdx];
                int gltfJointId = gltfJoints[jIdx];
                joint.RigNodeIdx = gltfJointId;

                Matrix4x4 offsetMatrix = bindMatrixData.ReadElement(jIdx);
                if (makeLeftHanded)
                {
                    offsetMatrix.M13 *= -1;
                    offsetMatrix.M23 *= -1;
                    offsetMatrix.M43 *= -1;

                    offsetMatrix.M31 *= -1;
                    offsetMatrix.M32 *= -1;
                    offsetMatrix.M34 *= -1;
                }

                joint.OffsetMatrix = offsetMatrix;
            }

            skin.Joints = joints;
            skins[skinIdx] = skin;
        }

        MeshEntity entity = new MeshEntity()
        {
            Name = "Unknown Entity Name", // Overriden by MeshAsset
            LocalTransform = Matrix4x4.CreateRotationX(90 * Maths.DEG2_RAD), // Y up to Z up since GLTF is Y up
            Meshes = meshes,
            AnimationRig = rigNodes,
            Animations = animations,
            AnimationSkins = skins,
        };

        // Validate rig - no child should be prior to its parent
        bool rigUnordered = false;
        for (int i = 0; i < rigNodes.Length; i++)
        {
            SkeletonAnimRigNode rigNode = rigNodes[i];
            if (rigNode.ParentIdx > i)
            {
                rigUnordered = true;
                break;
            }
        }

        if (rigUnordered)
        {
            Engine.Log.Warning($"Reordering animation rig for optimized matrix update", "GLTF");
            MeshEntity.PostProcess_FixAnimationRigOrder(entity);
        }

        return entity;
    }

    public static GLTFDocument CreateDocumentFromEntity(MeshEntity entity)
    {
        var doc = new GLTFDocument();
        doc.Asset = new GLTFAssetMeta()
        {
            LeftHanded = true
        };

        Mesh[] entityMeshes = entity.Meshes;
        doc.Meshes = new GLTFMesh[entityMeshes.Length];
        for (int i = 0; i < entityMeshes.Length; i++)
        {
            var gltfMesh = new GLTFMesh();
            var emoMesh = entity.Meshes[i];

            gltfMesh.Name = emoMesh.Name;

            doc.Meshes[i] = gltfMesh;
        }

        return doc;
    }

    private static ReadOnlyMemory<byte> GetAccessorData(GLTFDocument gltfDoc, GLTFAccessor accessor)
    {
        GLTFBufferView? bufferView = accessor.BufferView.GetReferenced(gltfDoc.BufferViews);
        if (bufferView == null) return default;

        GLTFBuffer? buffer = bufferView.Buffer.GetReferenced(gltfDoc.Buffers);
        if (buffer == null) return default;

        ReadOnlyMemory<byte> data = buffer.Data;
        return data.Slice(bufferView.ByteOffset, bufferView.ByteLength).Slice(accessor.ByteOffset);
    }

    private static AccessorReader<T> GetAccessorDataAsType<T>(GLTFDocument gltfDoc, GLTFAccessor accessor) where T : unmanaged
    {
        string accessorType = accessor.Type;
        string requestedType = TypeToGlType<T>();

        // Type mismatch
        if (accessorType != requestedType) return AccessorReader<T>.Empty;

        GLTFBufferView? bufferView = accessor.BufferView.GetReferenced(gltfDoc.BufferViews);
        GLTFBuffer? buffer = bufferView?.Buffer.GetReferenced(gltfDoc.Buffers);
        if (buffer == null) return AccessorReader<T>.Empty;

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
        ReadOnlySpan<byte> span = data.Span;
        ReadOnlySpan<TActualType> spanActualType = MemoryMarshal.Cast<byte, TActualType>(span);
        TActualType actualTypeValue = spanActualType[0];

        float valAsFloat = float.CreateTruncating<TActualType>(actualTypeValue);

        // Cast the number to a float and divide it by its max value to normalize it.
        if (normalize && typeof(TActualType) != typeof(float))
        {
            float valMax = float.CreateTruncating<TActualType>(TActualType.MaxValue);
            valAsFloat /= valMax;
        }

        return valAsFloat;
    }
}
