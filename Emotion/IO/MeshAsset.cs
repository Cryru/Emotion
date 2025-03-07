#region Using

using Emotion.Graphics.ThreeDee;
using Emotion.IO.MeshAssetTypes;
using Emotion.Standard.GLTF;
#if MORE_MESH_TYPES
using Emotion.IO.MeshAssetTypes.Assimp;
using System.Threading.Tasks;
#endif

#endregion

#nullable enable

namespace Emotion.IO
{
    /// <summary>
    /// Represents an asset containing a 3D with meshes, materials, animations, and even textures.
    /// Automatically matches to the file type and can conditionally use ASSIMP to support more formats.
    /// .em3 is recommended for release mode loading.
    /// </summary>
    public class MeshAsset : Asset
    {
        public MeshEntity? Entity { get; protected set; }

        private Asset? _underlyingAsset;

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            //if (Name.Contains(".em3"))
            //{
            //    var asset = new EmotionMeshAsset
            //    {
            //        Name = Name
            //    };
            //    asset.Create(data);
            //    Entity = asset.Entity;
            //    _underlyingAsset = asset;
            //}
            // Our native support for .obj is trash
            //else if (Name.Contains(".obj"))
            //{
            //    var asset = new ObjMeshAsset
            //    {
            //        Name = Name
            //    };
            //    asset.Create(data);
            //    Entity = asset.Entity;
            //    _underlyingAsset = asset;
            //}
            if (Name.Contains(".gltf"))
            {
                MeshEntity? entity = GLTFFormat.Decode(AssetLoader.GetDirectoryName(Name), data);
                if (entity != null)
                {
                    entity.Name = Name;
                    Entity = entity;
                }
            }
            else
            {
#if MORE_MESH_TYPES
                var asset = new AssimpAsset
                {
                    Name = Name
                };
                asset.AssetLoader_CreateLegacy(data);
                Entity = asset.Entity;
                _underlyingAsset = asset;
#endif
            }

            // Cache bounds for null animation at least to
            // prevent loading on SetEntity.
            // Note that em3 loaded entity's will have all their bounds pre-calculated and loaded
            // while others will calculate their animations bounds in runtime when the animation is set.
            // Check GameObject3D.SetAnimation and its bounds retrieval.
            Entity?.EnsureCachedBounds(null);
        }

        protected override void DisposeInternal()
        {
            _underlyingAsset?.Dispose();
            _underlyingAsset = null;
        }
    }
}