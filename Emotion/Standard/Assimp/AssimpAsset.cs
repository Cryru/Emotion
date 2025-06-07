#nullable enable

#region Using

using System.IO;
using Emotion.Common.Threading;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Silk.NET.Assimp;
using AssContext = Silk.NET.Assimp.Assimp;
using AssTexture = Silk.NET.Assimp.Texture;
using AssMesh = Silk.NET.Assimp.Mesh;
using Emotion.IO;

#endregion

namespace Emotion.Standard.Assimp;

/// <summary>
/// The Assimp library can ba used to load more mesh formats than Emotion natively supports.
/// Note that this isn't optimized and is to be used in development builds only.
/// </summary>
public static class AssimpFormat
{
    private static AssContext _assContext = AssContext.GetApi();

    private static PostProcessSteps _postProcFlags = PostProcessSteps.Triangulate |
                                                     PostProcessSteps.JoinIdenticalVertices |
                                                     PostProcessSteps.FlipUVs |
                                                     PostProcessSteps.SortByPrimitiveType |
                                                     PostProcessSteps.OptimizeGraph |
                                                     PostProcessSteps.OptimizeMeshes |
                                                     PostProcessSteps.MakeLeftHanded;

    private static PostProcessSteps _postProcFlagsRetry = PostProcessSteps.Triangulate |
                                                          PostProcessSteps.JoinIdenticalVertices |
                                                          PostProcessSteps.FlipUVs |
                                                          PostProcessSteps.SortByPrimitiveType |
                                                          PostProcessSteps.OptimizeMeshes |
                                                          PostProcessSteps.MakeLeftHanded;

    private const bool Y_TO_Z_UP = true;

    public static unsafe MeshEntity? CreateEntityFromDocument(ReadOnlyMemory<byte> data, string name)
    {
        // Initialize virtual file system.
        var fileSystem = new AssimpVirtualFileSystem();
        fileSystem.AddFile(name, data);

        ref FileIO nativeIO = ref fileSystem.NativeFileIO;

        string sceneDirectory = AssetLoader.GetDirectoryName(name);

        //_assContext.EnableVerboseLogging(1);
        //_assContext.AttachLogStream(_assContext.GetPredefinedLogStream(DefaultLogStream.Stdout, ""));

        Scene* scene = _assContext.ImportFileEx(name, (uint)_postProcFlags, ref nativeIO);
        if ((IntPtr)scene == IntPtr.Zero)
        {
            scene = _assContext.ImportFileEx(name, (uint)_postProcFlagsRetry, ref nativeIO);
            if ((IntPtr)scene == IntPtr.Zero)
            {
                Engine.Log.Error(_assContext.GetErrorStringS(), "Assimp");
                return null;
            }
        }

        bool isYUp = true;
        //if (scene->MMetaData != null)
        //{
        //    int? upAxis = GetMetadataInt(scene->MMetaData, "UpAxis");
        //    if (upAxis == 1) isYUp = true;
        //}

        // Get the animation rig
        Node* rootNode = scene->MRootNode;
        SkeletonAnimRigNode[] animRig = WalkNodesForSkeleton(rootNode);

        // Process materials, animations, and meshes.
        var materials = new List<MeshMaterial>();
        ProcessMaterials(sceneDirectory, scene, materials);

        var animations = new List<SkeletalAnimation>();
        ProcessAnimations(scene, animations, animRig);

        // Add animations from a _Animations folder.
        string animFolder = AssetLoader.GetFilePathNoExtension(name) + "_Animations/";
        string[] assetsInFolder = Engine.AssetLoader.GetAssetsInFolder(animFolder);
        for (var i = 0; i < assetsInFolder.Length; i++)
        {
            string assetPath = assetsInFolder[i];
            string assetPathRelative = assetPath.Replace(sceneDirectory, "");
            Scene* otherAssetScene = _assContext.ImportFileEx(assetPathRelative, (uint)_postProcFlags, ref nativeIO);
            if (otherAssetScene != null) ProcessAnimations(otherAssetScene, animations, animRig);
        }

        var meshes = new List<Mesh>();
        var skins = new List<SkeletalAnimationSkin>();

        WalkNodesForMeshes(scene, rootNode, meshes, materials, skins, animRig);

        var entity = new MeshEntity
        {
            Meshes = meshes.ToArray(),
            Animations = animations.ToArray(),
            AnimationRig = animRig,
            AnimationSkins = skins.ToArray()
        };

        // Convert to Z up, if not.
        //if (isYUp) Entity.LocalTransform = Matrix4x4.CreateRotationX(90 * Maths.DEG2_RAD);

        // Properties
        float? scaleF = GetMetadataFloat(rootNode->MMetaData, "UnitScaleFactor");
        if (scaleF != null)
            entity.Scale = scaleF.Value;

        fileSystem.Dispose();

        return entity;
    }

    #region Materials

    private static unsafe void ProcessMaterials(string sceneDirectory, Scene* scene, List<MeshMaterial> list)
    {
        var embeddedTextures = new List<Texture>();
        for (var i = 0; i < scene->MNumTextures; i++)
        {
            AssTexture* assTexture = scene->MTextures[i];

            if (assTexture->MHeight == 0) // compressed
            {
                // Data in embedded png
                var dataAsByte = new ReadOnlySpan<byte>(assTexture->PcData, (int)assTexture->MWidth);
                byte[] dataManaged = dataAsByte.ToArray();
                TextureAsset embeddedTexture = new TextureAsset();
                embeddedTexture.AssetLoader_CreateLegacy(dataManaged);
                embeddedTextures.Add(embeddedTexture.Texture);
            }
            else
            {
                // Texture data is ARGB8888
                var dataAsByte = new ReadOnlySpan<byte>(assTexture->PcData, (int)(assTexture->MWidth * assTexture->MHeight * 4));

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

        var materialNameDuplicatePrevent = new HashSet<string>();
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
                    diffuseTexture = embeddedTextures[(int)idx];
                }
                else if (!string.IsNullOrEmpty(diffuseTextureName))
                {
                    string diffuseTextureNameEnginePath = AssetLoader.NameToEngineName(diffuseTextureName);
                    string assetPath = AssetLoader.GetNonRelativePath(sceneDirectory, diffuseTextureNameEnginePath);
                    diffuseTextureName = assetPath;

                    TextureAsset? textureAsset = Engine.AssetLoader.Get<TextureAsset>(assetPath);
                    diffuseTexture = textureAsset?.Texture;
                }
            }

            string originalName = materialName;
            int counter = 2;
            while (materialNameDuplicatePrevent.Contains(materialName))
            {
                materialName = $"{originalName}_{counter}";
                counter++;
            }

            materialNameDuplicatePrevent.Add(materialName);

            if (diffuseTexture != null)
            {
                diffuseTexture.Smooth = true;
                diffuseTexture.Tile = true;
                diffuseTexture.Mipmap = true;
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
            (uint*)0, (float*)0, (TextureOp*)0, (TextureMapMode*)0, (uint*)0);
        if (result == Return.Success)
        {
            texturePath = path.AsString;
            return true;
        }

        return false;
    }

    #endregion

    #region Animations

    private static unsafe SkeletonAnimRigNode[] WalkNodesForSkeleton(Node* root)
    {
        List<SkeletonAnimRigNode> rig = new List<SkeletonAnimRigNode>();

        Queue<IntPtr> queue = new Queue<IntPtr>();
        queue.Enqueue((IntPtr)root);

        while (queue.TryDequeue(out IntPtr nextNodePtr))
        {
            Node* nodePtr = (Node*)nextNodePtr;

            int parentIdx = -1;
            IntPtr parentPtr = (IntPtr)nodePtr->MParent;
            if (parentPtr != IntPtr.Zero)
            {
                string parentName = nodePtr->MParent->MName.AsString;
                for (int r = 0; r < rig.Count; r++)
                {
                    var rigItem = rig[r];
                    if (rigItem.Name == parentName)
                    {
                        parentIdx = r;
                        break;
                    }
                }
            }

            string nodeName = nodePtr->MName.AsString;
            var myNode = new SkeletonAnimRigNode
            {
                Name = nodeName,
                LocalTransform = Matrix4x4.Transpose(nodePtr->MTransformation),
                ParentIdx = parentIdx,
                DontAnimate = nodeName.Contains("$AssimpFbx$")
            };
            rig.Add(myNode);

            for (var i = 0; i < nodePtr->MNumChildren; i++)
            {
                Node* child = nodePtr->MChildren[i];
                queue.Enqueue((IntPtr)child);
            }
        }

        return rig.ToArray();
    }

    private static unsafe void ProcessAnimations(Scene* scene, List<SkeletalAnimation> list, SkeletonAnimRigNode[] animRig)
    {
        var unnamedAnimations = 0;
        for (var i = 0; i < scene->MNumAnimations; i++)
        {
            Animation* anim = scene->MAnimations[i];

            string animName = anim->MName.AsString;
            var emotionAnim = new SkeletalAnimation
            {
                Name = string.IsNullOrEmpty(animName) ? $"UnnamedAnimation{unnamedAnimations++}" : animName,
                AnimChannels = new SkeletonAnimChannel[animRig.Length],
                Duration = (float)(anim->MDuration / anim->MTicksPerSecond) * 1000
            };
            list.Add(emotionAnim);

            uint channels = anim->MNumChannels;
            for (var j = 0; j < channels; j++)
            {
                NodeAnim* channel = anim->MChannels[j];

                var emotionChannel = new SkeletonAnimChannel
                {
                    Name = channel->MNodeName.AsString,
                    Positions = new MeshAnimBoneTranslation[channel->MNumPositionKeys],
                    Rotations = new MeshAnimBoneRotation[channel->MNumRotationKeys],
                    Scales = new MeshAnimBoneScale[channel->MNumScalingKeys]
                };

                for (var k = 0; k < channel->MNumPositionKeys; k++)
                {
                    VectorKey val = channel->MPositionKeys[k];
                    ref MeshAnimBoneTranslation translation = ref emotionChannel.Positions[k];
                    translation.Position = val.MValue;
                    translation.Timestamp = (float)(val.MTime / anim->MTicksPerSecond) * 1000;
                }

                for (var k = 0; k < channel->MNumRotationKeys; k++)
                {
                    QuatKey val = channel->MRotationKeys[k];
                    ref MeshAnimBoneRotation rotation = ref emotionChannel.Rotations[k];
                    rotation.Rotation = val.MValue.AsQuaternion;
                    rotation.Timestamp = (float)(val.MTime / anim->MTicksPerSecond) * 1000;
                }

                for (var k = 0; k < channel->MNumScalingKeys; k++)
                {
                    VectorKey val = channel->MScalingKeys[k];
                    ref MeshAnimBoneScale scale = ref emotionChannel.Scales[k];
                    scale.Scale = val.MValue;
                    scale.Timestamp = (float)(val.MTime / anim->MTicksPerSecond) * 1000;
                }

                var rigIdx = 0;
                for (int r = 0; r < animRig.Length; r++)
                {
                    SkeletonAnimRigNode rigItem = animRig[r];
                    if (rigItem.Name == emotionChannel.Name)
                    {
                        rigIdx = r;
                        break;
                    }
                }
                emotionAnim.AnimChannels[rigIdx] = emotionChannel;
            }
        }
    }

    #endregion

    #region Meshes

    private static unsafe void WalkNodesForMeshes(
        Scene* scene,
        Node* n,
        List<Mesh> list,
        List<MeshMaterial> materials,
        List<SkeletalAnimationSkin> skins,
        SkeletonAnimRigNode[] animRig
    )
    {
        if ((IntPtr)n == IntPtr.Zero) return;

        for (var i = 0; i < n->MNumMeshes; i++)
        {
            uint meshIdx = n->MMeshes[i];
            AssMesh* mesh = scene->MMeshes[meshIdx];
            Mesh emotionMesh = ProcessMesh(mesh, materials, skins, animRig);
            list.Add(emotionMesh);
        }

        for (var i = 0; i < n->MNumChildren; i++)
        {
            Node* child = n->MChildren[i];
            WalkNodesForMeshes(scene, child, list, materials, skins, animRig);
        }
    }

    private static unsafe Mesh ProcessMesh(
        AssMesh* m,
        List<MeshMaterial> materials,
        List<SkeletalAnimationSkin> skins,
        SkeletonAnimRigNode[] animRig
    )
    {
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
            if ((IntPtr)face.MIndices == IntPtr.Zero) continue;

            for (var j = 0; j < face.MNumIndices; j++)
            {
                emotionIndices[emoIdx] = (ushort)face.MIndices[j];
                emoIdx++;
            }
        }

        // Copy vertices (todo: separate path for boneless)
        var vertices = new VertexData[m->MNumVertices];
        var meshData = new VertexDataMesh3DExtra[m->MNumVertices];

        for (var i = 0; i < m->MNumVertices; i++)
        {
            ref Vector3 assVertex = ref m->MVertices[i];

            if (Y_TO_Z_UP)
            {
                float z = assVertex.Z;
                assVertex.Z = assVertex.Y;
                assVertex.Y = -z;
                //assVertex.X = assVertex.X;
            }

            var uv = new Vector2(0, 0);
            if ((IntPtr)m->MNumUVComponents != IntPtr.Zero && m->MNumUVComponents[0] >= 2)
            {
                Vector3 uv3 = m->MTextureCoords[0][i];
                uv = new Vector2(uv3.X, uv3.Y);
            }

            Color vertexColor = Color.White;
            if ((IntPtr)m->MColors[0] != IntPtr.Zero)
            {
                Vector4 assVertColor = m->MColors[0][i]; // RGBA
                vertexColor = new Color(assVertColor);
            }

            vertices[i] = new VertexData
            {
                Vertex = assVertex,
                UV = uv,
                Color = vertexColor.ToUint()
            };

            if ((IntPtr)m->MNormals != IntPtr.Zero)
            {
                Vector3 normal = m->MNormals[i];

                if (Y_TO_Z_UP)
                {
                    (normal.Z, normal.Y) = (normal.Y, -normal.Z);
                    (normal.X, normal.Y) = (normal.Y, normal.X);
                }

                meshData[i] = new VertexDataMesh3DExtra
                {
                    Normal = normal
                };
            }
            else
            {
                meshData[i] = new VertexDataMesh3DExtra
                {
                    Normal = RenderComposer.Up
                };
            }
        }

        var newMesh = new Mesh(m->MName.AsString, vertices, meshData, emotionIndices)
        {
            Material = materials[(int)m->MMaterialIndex]
        };

        if (m->MNumBones == 0) return newMesh;

        // Initialize bone data.
        var boneData = new Mesh3DVertexDataBones[m->MNumVertices];
        for (var i = 0; i < boneData.Length; i++)
        {
            boneData[i] = new Mesh3DVertexDataBones
            {
                BoneIds = new Vector4(0, 0, 0, 0),
                BoneWeights = new Vector4(1, 0, 0, 0)
            };
        }
        newMesh.BoneData = boneData;

        var animationSkin = new SkeletalAnimationSkin
        {
            Name = $"{m->MName.AsString} Skin",
            Joints = new SkeletalAnimationSkinJoint[m->MNumBones]
        };
        skins.Add(animationSkin);
        newMesh.AnimationSkin = skins.Count - 1;

        for (var i = 0; i < m->MNumBones; i++)
        {
            Bone* bone = m->MBones[i];
            string boneName = bone->MName.AsString;

            int rigIdx = i;
            for (int r = 0; r < animRig.Length; r++)
            {
                SkeletonAnimRigNode rigItem = animRig[r];
                if (rigItem.Name == boneName)
                {
                    rigIdx = r;
                    break;
                }
            }

            SkeletalAnimationSkinJoint newSkinJoint = new SkeletalAnimationSkinJoint()
            {
                RigNodeIdx = rigIdx,
                OffsetMatrix = Matrix4x4.Transpose(bone->MOffsetMatrix)
            };
            animationSkin.Joints[i] = newSkinJoint;

            int boneIdx = i;
            for (var j = 0; j < bone->MNumWeights; j++)
            {
                ref VertexWeight boneDef = ref bone->MWeights[j];
                float weight = boneDef.MWeight;

                // Sanity checks
                if (boneDef.MVertexId > vertices.Length - 1) continue;
                if (weight == 0) continue;

                ref Mesh3DVertexDataBones vertex = ref boneData[boneDef.MVertexId];

                var found = false;
                for (var dim = 0; dim < 4; dim++)
                {
                    if (vertex.BoneIds[dim] != 0) continue;

                    vertex.BoneIds[dim] = boneIdx;
                    vertex.BoneWeights[dim] = weight;
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

                    if (lowestWeight < weight)
                    {
                        vertex.BoneIds[lowestWeightIdx] = boneIdx;
                        vertex.BoneWeights[lowestWeightIdx] = weight + lowestWeight;
                    }
                }
            }
        }

        // Normalize bone weights to 1.
        //for (var i = 0; i < vertices.Length; i++)
        //{
        //    // The code below is written out with doubles due to models with
        //    // very tiny weights weights that cannot be ignored.

        //    ref Mesh3DVertexDataBones vertex = ref boneData[i];
        //    double sum = 0;
        //    for (var c = 0; c < 4; c++)
        //    {
        //        double weight = vertex.BoneWeights[c];
        //        if (weight == 0) continue;
        //        sum += weight;
        //    }

        //    double ratio = 1.0 / sum;
        //    for (var c = 0; c < 4; c++)
        //    {
        //        double weight = vertex.BoneWeights[c];
        //        if (weight == 0) continue;

        //        weight *= ratio;

        //        var weightFloat = (float)weight;
        //        if (weightFloat == 0) weightFloat = Maths.EPSILON; // Ensure it doesn't hit 0
        //        //vertex.BoneWeights[c] = weightFloat;
        //    }
        //}

        return newMesh;
    }

    #endregion

    private static unsafe float? GetMetadataFloat(Metadata* meta, string key)
    {
        if ((IntPtr)meta == IntPtr.Zero) return null;

        for (var i = 0; i < meta->MNumProperties; i++)
        {
            AssimpString k = meta->MKeys[i];
            if (k.AsString == key && meta->MValues->MType == MetadataType.Float)
                return *(float*)meta->MValues->MData;
        }

        return null;
    }

    private static unsafe int? GetMetadataInt(Metadata* meta, string key)
    {
        if ((IntPtr)meta == IntPtr.Zero) return null;

        for (var i = 0; i < meta->MNumProperties; i++)
        {
            AssimpString k = meta->MKeys[i];
            if (k.AsString == key && meta->MValues->MType == MetadataType.Int32)
                return *(int*)meta->MValues->MData;
        }

        return null;
    }

    public static unsafe void ExportEntity(MeshEntity entity, string formatId, string name)
    {
        Scene sc = new Scene();
        Node rootNode = new Node();

        sc.MRootNode = &rootNode;
        rootNode.MNumMeshes = (uint)entity.Meshes.Length;

        AssMesh[] meshes = new AssMesh[entity.Meshes.Length];
        for (int i = 0; i < meshes.Length; i++)
        {

        }


        ExportAs(&sc, formatId, name);
    }

    public static unsafe void ExportAs(Scene* scene, string formatId, string name)
    {
        ExportDataBlob* blob = _assContext.ExportSceneToBlob(scene, formatId, (uint)_postProcFlags);
        var str = new MemoryStream();
        using (var writer = new BinaryWriter(str))
        {
            while (true)
            {
                if (blob == null) return;

                bool hasNext = blob->Next != null;

                writer.Write(blob->Name.AsString);
                writer.Write(blob->Size);
                var data = new Span<byte>(blob->Data, (int)blob->Size);
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