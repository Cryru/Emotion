#region Using

using System.IO;
using System.Threading.Tasks;

#endregion

namespace Emotion.IO.AssetPack
{
    public class FilePackedAssetSource : PackedAssetSource
    {
        public FilePackedAssetSource(string directory) : base(directory)
        {
        }

        protected override Task<byte[]> GetFileContent(string fileName)
        {
            if (!File.Exists(fileName)) return Task.FromResult((byte[]) null);
            return File.ReadAllBytesAsync(fileName);
        }
    }
}