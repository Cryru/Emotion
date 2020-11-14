#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.IO.AssetPack
{
    public class AssetBlob
    {
        public string Name;
        public int Index;
        public Dictionary<string, BlobFile> BlobMeta = new Dictionary<string, BlobFile>();
    }
}