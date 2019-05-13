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

        protected override void CreateInternal(byte[] data)
        {
            Content = Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}