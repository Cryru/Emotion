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
            return !File.Exists(fileName) ? Task.FromResult((byte[]) null) : File.ReadAllBytesAsync(fileName);
        }
    }
}