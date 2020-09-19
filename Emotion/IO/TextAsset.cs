#region Using

using System.Text;
using Emotion.Utility;

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
            Encoding encoding = Helpers.GuessStringEncoding(data);
            Content = encoding.GetString(data);
            // Remove windows new lines and BOM.
            Content = Content.Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}