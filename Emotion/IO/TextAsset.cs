#region Using

using System;
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

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            ReadOnlySpan<byte> span = data.Span;
            Encoding encoding = Helpers.GuessStringEncoding(span);
            Content = encoding.GetString(span);
            // Remove windows new lines and BOM.
            Content = Content.Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        protected override void DisposeInternal()
        {
            Content = null;
        }
    }
}