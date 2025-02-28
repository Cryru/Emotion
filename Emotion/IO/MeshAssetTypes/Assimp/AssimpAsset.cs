﻿#region Using

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
using System.Text.Json;
using Emotion.IO.MeshAssetTypes.Assimp.GLTF;
using static Emotion.Utility.Helpers;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.Assimp;

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
                                              PostProcessSteps.OptimizeMeshes |
                                              PostProcessSteps.MakeLeftHanded;

    protected override unsafe void CreateInternal(ReadOnlyMemory<byte> data)
    {
        _thisData = data;

        // Initialize virtual file system.
        var thisStream = new AssimpStream(Name);
        thisStream.AddMemory(_thisData);
        _loadedFiles.Add(thisStream);

        //_assContext.EnableVerboseLogging(1);
        //_assContext.AttachLogStream(_assContext.GetPredefinedLogStream(DefaultLogStream.Stdout, ""));

        var customIO = new FileIO
        {
            OpenProc = PfnFileOpenProc.From(OpenFileCallback),
            CloseProc = PfnFileCloseProc.From(CloseFileCallback)
        };

        PostProcessSteps postProcFlags = _postProcFlags;
        Scene* scene = _assContext.ImportFileEx(Name, (uint)postProcFlags, ref customIO);
        if ((IntPtr)scene == IntPtr.Zero)
        {
            Engine.Log.Error(_assContext.GetErrorStringS(), "Assimp");
            return;
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
        ProcessMaterials(scene, materials);

        var animations = new List<SkeletalAnimation>();
        ProcessAnimations(scene, animations, animRig);

        // Add animations from a _Animations folder.
        string myFolder = AssetLoader.GetDirectoryName(Name);
        string animFolder = AssetLoader.GetFilePathNoExtension(Name) + "_Animations/";
        string[] assetsInFolder = Engine.AssetLoader.GetAssetsInFolder(animFolder);
        for (var i = 0; i < assetsInFolder.Length; i++)
        {
            string asset = assetsInFolder[i];
            Scene* otherAssetScene = _assContext.ImportFileEx(asset.Replace(myFolder, ""), (uint)_postProcFlags, ref customIO);
            if (otherAssetScene != null) ProcessAnimations(otherAssetScene, animations, animRig);
        }

        var meshes = new List<Mesh>();
        var skins = new List<SkeletalAnimationSkin>();

        WalkNodesForMeshes(scene, rootNode, meshes, materials, skins, animRig);

        Entity = new MeshEntity
        {
            Name = Name,
            Meshes = meshes.ToArray(),
            Animations = animations.ToArray(),
            AnimationRig = animRig,
            AnimationSkins = skins.ToArray()
        };

        // Convert to Z up, if not.
        if (isYUp) Entity.LocalTransform = Matrix4x4.CreateRotationX(90 * Maths.DEG2_RAD);

        // Properties
        float? scaleF = GetMetadataFloat(rootNode->MMetaData, "UnitScaleFactor");
        if (scaleF != null)
            Entity.Scale = scaleF.Value;

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
        string readMode = NativeHelpers.StringFromPtr((IntPtr)arg2);
        if (readMode != "rb")
        {
            Engine.Log.Error("Only read-binary file mode is supported.", "Assimp");
            return null;
        }

        string fileName = NativeHelpers.StringFromPtr((IntPtr)arg1);
        for (var i = 0; i < _loadedFiles.Count; i++)
        {
            AssimpStream alreadyOpenFile = _loadedFiles[i];
            if (alreadyOpenFile.Name == fileName) return (File*)alreadyOpenFile.Memory;
        }

        string assetPath = AssetLoader.GetDirectoryName(Name);
        fileName = AssetLoader.GetNonRelativePath(assetPath, fileName);
        var byteAsset = Engine.AssetLoader.Get<OtherAsset>(fileName, false);
        if (byteAsset == null) return null;

        var assimpStream = new AssimpStream(fileName);
        assimpStream.AddMemory(byteAsset.Content);
        _loadedFiles.Add(assimpStream);
        return (File*)assimpStream.Memory;
    }

    private unsafe void CloseFileCallback(FileIO* arg0, File* arg1)
    {
        // When Assimp wants to close a file we'll keep it loaded
        // but reset its position instead as it can be requested again and
        // we want to reduce IO. At the end of the asset parsing all loaded
        // files will be properly unloaded.
        var filePtr = (IntPtr)arg1;
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
                var dataAsByte = new ReadOnlySpan<byte>(assTexture->PcData, (int)assTexture->MWidth);
                byte[] dataManaged = dataAsByte.ToArray();
                var embeddedTexture = new TextureAsset();
                embeddedTexture.Create(dataManaged);
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
                    string assetPath = AssetLoader.GetDirectoryName(Name);
                    assetPath = AssetLoader.GetNonRelativePath(assetPath, diffuseTextureName);
                    var textureAsset = Engine.AssetLoader.Get<TextureAsset>(assetPath, false);
                    diffuseTexture = textureAsset?.Texture;
                }
            }

            string originalName = materialName;
            var counter = 2;
            while (materialNameDuplicatePrevent.Contains(materialName))
            {
                materialName = $"{originalName}_{counter}";
                counter++;
            }

            materialNameDuplicatePrevent.Add(materialName);

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

    protected unsafe SkeletonAnimRigNode[] WalkNodesForSkeleton(Node* root)
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

    private unsafe void ProcessAnimations(Scene* scene, List<SkeletalAnimation> list, SkeletonAnimRigNode[] animRig)
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

    protected unsafe void WalkNodesForMeshes(
        Scene* scene,
        Node* n,
        List<Mesh> list,
        List<MeshMaterial> materials,
        List<SkeletalAnimationSkin> skins,
        SkeletonAnimRigNode[] animRig)
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

    protected unsafe Mesh ProcessMesh(AssMesh* m, List<MeshMaterial> materials, List<SkeletalAnimationSkin> skins, SkeletonAnimRigNode[] animRig)
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
                ref Vector3 normal = ref m->MNormals[i];
                meshData[i] = new VertexDataMesh3DExtra
                {
                    Normal = normal
                };
            }
            else
            {
                meshData[i] = new VertexDataMesh3DExtra
                {
                    Normal = RenderComposer.Forward
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

    private unsafe float? GetMetadataFloat(Metadata* meta, string key)
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

    private unsafe int? GetMetadataInt(Metadata* meta, string key)
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

    protected override void DisposeInternal()
    {
    }

    public unsafe void ExportEntity(MeshEntity entity, string formatId, string name)
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

    public unsafe void ExportAs(Scene* scene, string formatId, string name)
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