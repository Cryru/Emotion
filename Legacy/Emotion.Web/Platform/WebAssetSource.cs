#region Using

using System.Net.Http;
using System.Threading.Tasks;
using Emotion.IO.AssetPack;

#endregion

namespace Emotion.Web.Platform
{
    public class WebAssetSource : PackedAssetSource
    {
        private HttpClient _httpClient;

        public WebAssetSource(string folder, HttpClient httpClient) : base(folder)
        {
            _httpClient = httpClient;
        }

        protected override Task<byte[]> GetFileContent(string fileName)
        {
            return _httpClient.GetByteArrayAsync(fileName);
        }
    }
}