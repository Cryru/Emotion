#region Using

using System;
using System.Text;
using Emotion.IO;

#endregion

namespace Emotion.Web.Platform
{
    public class WebAssetStore : AssetSource, IAssetStore
    {
        public string Folder { get; } = "Web Download";

        private WebHost _host;

        public WebAssetStore(WebHost webHost)
        {
            _host = webHost;
        }

        public void SaveAsset(byte[] data, string name, bool backup)
        {
            string str = Encoding.UTF8.GetString(data);
            _host.JsRuntime.InvokeUnmarshalled<string, string, bool>("downloadTextFile", name, str);
        }

        public override ReadOnlyMemory<byte> GetAsset(string enginePath)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        public override string[] GetManifest()
        {
            return Array.Empty<string>();
        }
    }
}