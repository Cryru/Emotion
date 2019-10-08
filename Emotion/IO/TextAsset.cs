#region Using

using System.Text;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A text file asset.
    /// </summary>
    public class TextAsset : Asset
    {
        /// <summary>
        /// The context of the text file.
        /// </summary>
        public string Content { get; private set; }

        protected override void CreateInternal(byte[] data)
        {
            // Remove new lines and BOM.
            Content = Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}