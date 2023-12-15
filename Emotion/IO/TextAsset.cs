#region Using

using System.Text;
using Emotion.Standard.XML;

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

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            ReadOnlySpan<byte> span = data.Span;
            Encoding encoding = XMLFormat.GuessStringEncoding(span); // Commonly the string is xml.
            Content = encoding.GetString(span);

            // Convert Windows new lines (\r\n) to Unix ones (\n) and remove BOM.
            Content = Content.Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}