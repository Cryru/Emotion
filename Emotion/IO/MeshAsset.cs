#nullable enable

#region Using

using Emotion.Graphics.ThreeDee;
using Emotion.Standard.GLTF;
using Emotion.Serialization.JSON;


#if MORE_MESH_TYPES
using Emotion.Standard.Assimp;
#endif

#endregion

namespace Emotion.IO;

/// <summary>
/// Represents an asset containing a 3D with meshes, materials, animations, and even textures.
/// Automatically matches to the file type and can conditionally use ASSIMP to support more formats.
/// .em3 is recommended for release mode loading.
/// </summary>
public class MeshAsset : Asset
{
    public MeshEntity? Entity { get; protected set; }

    public MeshAsset()
    {
        _useNewLoading = true;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        string directoryName = AssetLoader.GetDirectoryName(Name);

        MeshEntity? entity = null;
        if (Name.Contains(".gltf"))
        {
            GLTFDocument? gltfDoc = GLTFFormat.Decode(data);
            if (gltfDoc != null)
            {
                foreach (string dependencyPath in GLTFFormat.ForEachBufferDependency(gltfDoc, directoryName))
                {
                    LoadAssetDependency<OtherAsset>(dependencyPath);
                }

                foreach (string dependencyPath in GLTFFormat.ForEachImageDependency(gltfDoc, directoryName))
                {
                    LoadAssetDependency<TextureAsset>(dependencyPath);
                }

                yield return WaitAllDependenciesToLoad();

                entity = GLTFFormat.CreateEntityFromDocument(gltfDoc, directoryName);
            }
        }
#if MORE_MESH_TYPES
        else
        {
            entity = AssimpFormat.CreateEntityFromDocument(data, Name);
            if (entity != null && entity.Meshes.Length == 0)
                entity = null;

#if DEBUG
            // Try to save it as GLTF
            if (entity != null)
            {
                var gltfDoc = GLTFFormat.CreateDocumentFromEntity(entity);
                string asGltfFile = JSONSerialization.To(gltfDoc);
                string nameWithExt = AssetLoader.GetFilePathNoExtension(Name);
                byte[] bytes = System.Text.Encoding.Default.GetBytes(asGltfFile);
                Engine.AssetLoader.Save(bytes, nameWithExt);
            }
#endif
        }
#endif

        if (entity != null)
        {
            entity.Name = Name;
            Entity = entity;

            // Cache bounds for null animation at least to prevent loading on SetEntity.
            // Note that em3 loaded entity's will have all their bounds pre-calculated and loaded
            // while others will calculate their animations bounds in runtime when the animation is set.
            // Check SetAnimation and its bounds retrieval.
            entity.EnsureCachedBounds(null);
        }
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {

    }

    protected override void DisposeInternal()
    {

    }
}