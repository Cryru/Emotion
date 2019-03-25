#region Using

using System.Text;

#endregion

namespace Adfectus.IO
{
    /// <summary>
    /// A text file asset.
    /// </summary>
    public class TextFile : Asset
    {
        /// <summary>
        /// The context of the text file.
        /// </summary>
        public string Content { get; private set; }

        internal override void CreateAsset(byte[] data)
        {
            Content = Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        internal override void DestroyAsset()
        {
            Content = null;
        }
    }
}