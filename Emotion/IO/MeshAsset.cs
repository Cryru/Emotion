#region Using

using Emotion.Graphics.ThreeDee;
using Emotion.IO.MeshAssetTypes;
#if ASSIMP
using Emotion.IO.MeshAssetTypes.Assimp;
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
            if (Name.Contains(".em3"))
            {
                var asset = new EmotionMeshAsset
                {
                    Name = Name
                };
                asset.Create(data);
                Entity = asset.Entity;
                _underlyingAsset = asset;
            }
            else if (Name.Contains(".obj"))
            {
                var asset = new ObjMeshAsset
                {
                    Name = Name
                };
                asset.Create(data);
                Entity = asset.Entity;
                _underlyingAsset = asset;
            }
            else
            {
#if ASSIMP
                var asset = new AssimpAsset
                {
                    Name = Name
                };
                asset.Create(data);
                Entity = asset.Entity;
                _underlyingAsset = asset;
#endif
            }
        }

        protected override void DisposeInternal()
        {
            _underlyingAsset?.Dispose();
            _underlyingAsset = null;
        }
    }
}